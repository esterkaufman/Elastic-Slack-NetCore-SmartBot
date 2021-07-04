using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nest;
using Newtonsoft.Json.Linq;
using Researcher.Bot.Implementations.Slack.Builders;
using Researcher.Bot.Implementations.Utils;
using Researcher.Bot.Integration.ElasticSearch.Interfaces;
using Researcher.Bot.Integration.ElasticSearch.Models;
using Researcher.Bot.Integration.Slack.Interfaces;
using Researcher.Bot.Integration.Slack.Models.Events;
using Researcher.Bot.Integration.Slack.Models.Interactive;
using Attachment = Researcher.Bot.Integration.Slack.Models.Interactive.Attachment;

namespace Researcher.Bot.Implementations.Slack
{
    public class ResearcherBotRequestHandler : IBotRequestHandler
    {
        private readonly IElasticSearchHandler _elasticSearch;
        private readonly ILogger<ResearcherBotRequestHandler> _logger;

        public ResearcherBotRequestHandler(IElasticSearchHandler elasticSearch,

            ILogger<ResearcherBotRequestHandler> logger)
        {
            _logger = logger;
            _elasticSearch = elasticSearch;
        }
        public async Task<HandlerResponse> HandleInteraction(InteractionPayloadBody interaction)
        {
            var res = "";

            try
            {
                switch (interaction)
                {
                    case MessageActionInteraction _:
                        return await HandleAction(SlashCommandsEnum.GUI);
                    case BlockActionInteraction actionInteraction when actionInteraction
                        .Actions.Any(a => a.action_id == BotActionsEnum.ACTION_GUI && a.type == ElementTypes.Button):
                        return await HandleAction(BotActionsEnum.ACTION_GUI);
                    case ViewSubmission @viewSubmission:
                        {
                            {
                                var actionObj = @viewSubmission.ViewState["values"]
                                    ?.FirstOrDefault(m => m.First()["action"] != null);
                                if (actionObj != null)
                                {
                                    var parames = @viewSubmission.ViewState["values"].Children().Select(obj =>
                                    {
                                        var o = obj.First().First().First();
                                        var action = (obj.First().First() as JProperty).Name;
                                        var val = o.Value<string>("value") ?? o["selected_option"]["value"].ToString();

                                        return (action, val);
                                    }).ToDictionary(k => k.action, v => v.val);
                                    var query = parames["action"];
                                    
                                    if (!new[] { BotActionsEnum.ACTION_LINKS, BotActionsEnum.ACTION_MONITOR_LINKS }.Contains(parames["action"]))
                                        query = $"{query} {parames["dc"]} {parames["callId"]} {parames["from"]} {parames["to"]}";

                                    var response =  await HandleAction(query);
                                    
                                    if (response.Blocks == null || !response.Blocks.Any())
                                    {
                                        response.Blocks = new IBlock[] { new SectionBlock() { text = new Text { type = TextTypes.Markdown, text = response.Response} } };
                                        response.Response = parames["action"].ToUpper();
                                        return response;
                                    }
                                }
                            }
                            break;
                        }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error when trying to get response in bot handler, {e.InnerException}");
                res = $":worried: I've got an error when trying to get results for you.\nException:\n{e.Message}";
            }
            return new HandlerResponse(res);
        }

        public async Task<HandlerResponse> HandleCommand(SlashCommandPayloadBody slackCommand, Action<string> postToBot = null)
        {
            var actionFromCommand = slackCommand.Command
                .Replace(SlashCommandsEnum.LogDog, BotActionsEnum.ACTION_LOG_DOG)
                .Replace(SlashCommandsEnum.GetStatistics, BotActionsEnum.ACTION_GET_STATS)
                .Replace(SlashCommandsEnum.GetBlamer, BotActionsEnum.ACTION_GET_BLAMER)
                .Replace(SlashCommandsEnum.Links, BotActionsEnum.ACTION_LINKS)
                .Replace(SlashCommandsEnum.MonitorLinks, BotActionsEnum.ACTION_MONITOR_LINKS);

            try
            {
                return await HandleAction($"{actionFromCommand}{slackCommand.Text}",postToBot);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error when trying to get response in bot handler, {e.InnerException}");
                var res = $":worried: I've got an error when trying to get results for you.\nException:\n{e.Message}";
                return new HandlerResponse(res);
            }
        }

        public async Task<HandlerResponse> Handle(SlackApiMetaData slackApiMetaData, AppMentionEventBody slackEvent,Action<string> postToBot=null)
        {
            var query = RemoveBotIdFromAppMentionQuery(slackEvent.Text).Trim().Trim('*').Trim();
            
            try
            {
                return await HandleAction(query,postToBot);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error when trying to get response in bot handler, {e.InnerException}");
                var res = $":worried: I've got an error when trying to get results for you.\nException:\n{e.Message}";
                return new HandlerResponse(res);
            }
        }

        private string RemoveBotIdFromAppMentionQuery(string query)
        {
            
            return query.IndexOf("<@") >= 0 ? Regex.Replace(query, @"\<\@\w+\>", "", RegexOptions.IgnoreCase) : query;
        }

        private async Task<HandlerResponse> HandleAction(string query, Action<string> postToBot = null)
        {
            var res = "Sorry,:disappointed: I failed to find some answers, can you plz try any other command?";
            var blocks = new List<IBlock>();
            var attachments = new List<Attachment>();
            bool? isModal = null;

            _logger.LogInformation( $"Inside HandleAction, query: {query}");
            if (IsHelp(query))
            {
                res = SlackUiBuilderHelper.GetHelpUiResponse(BuildDescription(), blocks, attachments);
            }
            else
            {
                switch (query.ToLower())
                {
                    case SlashCommandsEnum.GUI:
                        res = SlackUiBuilderHelper.GetGUIActionsModal(blocks);
                        isModal = true;
                        break;
                    case BotActionsEnum.ACTION_GUI:
                        res = SlackUiBuilderHelper.GetOpenGUIActionsButton(blocks);
                        break;
                    case BotActionsEnum.ACTION_LINKS:
                        res = GetUsefuleLinks(blocks, attachments);
                        break;
                    case BotActionsEnum.ACTION_MONITOR_LINKS:
                        res = GetMonitoringLinks(blocks, attachments);
                        break;
                    default:
                        if (IsActionSupported(query, ref res, out var paramStr, out var parameters, out var action))
                        {
                            res = action switch
                            {
                                BotActionsEnum.ACTION_GET_STATS => await GetStatisticsAsync(paramStr, parameters),
                                BotActionsEnum.ACTION_GET_BLAMER => await GetNocBlamerAsync(paramStr, parameters, blocks, attachments,postToBot),
                                BotActionsEnum.ACTION_LOG_DOG => await GetLogDogAsync(paramStr, parameters, blocks, attachments, postToBot),
                                _ => res
                            };
                        }
                        break;
                }
            }
            return new HandlerResponse(res, blocks.ToArray(), attachments.ToArray(), isModal);
        }

        private string GetMonitoringLinks(IList<IBlock> blocks, IList<Attachment> attachments)
        {
            return new StringBuilder()
                .AppendLine("https://grafana.gigya.net/d/dJ3tX9Czz/service-system-metrics")
                .AppendLine("https://grafana.gigya.net/d/dASNzMzGk/service-overview")
                .AppendLine("https://grafana.gigya.net/d/CAMm6jCkk/service-metrics")
                .AppendLine("https://grafana.gigya.net/d/fK9T2e0mk/noc-dashboard")
                .AppendLine("http://kibana/kibana3/#/dashboard/elasticsearch/Ester%20-%20Service%20Monitor")
                .ToString();
        }

        private string GetUsefuleLinks(IList<IBlock> blocks, IList<Attachment> attachments)
        {
            return new StringBuilder()
                .AppendLine("http://build1.gigya.net/LogDog/show-log.html?callID=")
                .AppendLine("http://kibana/kibana3/#/dashboard/file/default.json")
                .AppendLine("https://teamcity.gigya.net/project.html?projectId=Configuration_Config")
                .AppendLine("https://gigya.tpondemand.com/RestUI/Board.aspx#page=profile")
                .AppendLine("http://dor.gigya-cs.com/")
                .AppendLine("https://sm.gigya.net/#/")
                .AppendLine("http://build1.gigya.net/dev.htm")
                .AppendLine("https://help.sap.com/viewer/8b8d6fffe113457094a17701f63e3d6a/GIGYA/en-US/414f36ba70b21014bbc5a10ce4041860.html")
                .ToString();
        }


        private async Task<string> GetLogDogAsync(string paramStr, string[] parameters, IList<IBlock> blocks, IList<Attachment> attachments, Action<string> postToBot = null)
        {
            var dc = parameters.Length > 1 ? parameters[1] : null;
            if (dc == null && postToBot != null)
                postToBot("Searching in all DCs. this could take up to a minute...");

            var logDogEsResponse = await _elasticSearch.GetLogDog(parameters[0], dc);

            if (logDogEsResponse != null)
            {
                return SlackUiBuilderHelper.GetLogDogUiResponse(logDogEsResponse, dc,parameters[0], blocks,attachments);
            }

            return GetEmptyResponseMsg(paramStr);
        }

        private async Task<string> GetNocBlamerAsync(string paramStr, string[] parameters, IList<IBlock> blocks, IList<Attachment> attachments, Action<string> postToBot = null)
        {
            var dc = parameters.Length > 1 ? parameters[1] : null;
            if (dc == null && postToBot != null)
                postToBot("Searching in all DCs. this could take up to a minute...");
            var blamerRes = await _elasticSearch.GetNocBlamer(parameters[0], dc);

            if (blamerRes != null)
            {
                return SlackUiBuilderHelper.GetNocBlamerUiResponse(blamerRes, dc, parameters[0], blocks, attachments);
            }
            return GetEmptyResponseMsg(paramStr);
        }

        private async Task<string> GetStatisticsAsync(string paramStr, string[] parameters)
        {
            var statsEsResponse = await _elasticSearch.GetStatistics(new StatsEsRequest
            {
                Dc = parameters[0],
                ServiceName = parameters[1],
                From = parameters.Length > 2 ? parameters[2] : "now-1h",
                To = parameters.Length > 3 ? parameters[3] : "now"
            });
            var res = $"Statistics for {parameters[0]}.{parameters[1]}: \n\n";

            return statsEsResponse != null ? $"{res}{statsEsResponse.ToJson()}" : GetEmptyResponseMsg(paramStr);
        }

        private bool IsActionSupported(string query, ref string res, out string paramsString, out string[] parameters, out string action)
        {
            paramsString = action = "";
            parameters = null;

            (string foundAction, string template) = new (string ac, string template)[]
            {
                (BotActionsEnum.ACTION_GET_STATS, BotActionsEnum.ACTION_GET_STATS_TEMPLATE),
                (BotActionsEnum.ACTION_GET_BLAMER,BotActionsEnum.ACTION_GET_BLAMER_TEMPLATE),
                (BotActionsEnum.ACTION_LOG_DOG, BotActionsEnum.ACTION_LOG_DOG_TEMPLATE)
            }.FirstOrDefault(l => query.ToLower().Contains(l.ac.Trim()));

            if (!string.IsNullOrEmpty(foundAction))
            {
                action = foundAction;
                paramsString = GetParamsString(query, action);
                parameters = paramsString.SplitWithDelimiters();

                var isValid = IsValidParams(action, parameters);

                res = isValid ? res : GetMissingParamsMsg(template);
                return isValid;
            }
            return false;
        }

        private bool IsValidParams(string action, string[] parameters)
        {
            return action switch
            {
                BotActionsEnum.ACTION_GET_STATS => parameters.Length >= 2,
                BotActionsEnum.ACTION_GET_BLAMER => parameters.Length == 1 || parameters.Length == 2,
                BotActionsEnum.ACTION_LOG_DOG => parameters.Length == 1 || parameters.Length == 2,
                _ => false
            };
        }

        private bool IsHelp(string query)
        {
            query = query.ToLower();

            return query.Equals("/help") || query.StartsWith("help") || query.StartsWith("/help") || query.EndsWith("help") ||
                   query.EndsWith("/help") || query.Substring(query.LastIndexOf('>') + 1).TrimStart(' ').StartsWith("help");
        }

        public static string BuildDescription()
        {

            return new List<(string HandlerTrigger, string Description)>
            {
                (BotActionsEnum.ACTION_GET_BLAMER_TEMPLATE,
                    "Found the NOC Blamer service for a callID, example: *Get blamer* 967b2b9c42434f53be88d4c88936103a us1"),
                (BotActionsEnum.ACTION_LOG_DOG_TEMPLATE,
                    "Get LogDog data for a callID, example: *logdog* 967b2b9c42434f53be88d4c88936103a.eu1"),
                (BotActionsEnum.ACTION_GET_STATS_TEMPLATE,
                    "Returns statistics about the service, example: *Get stats us1d.UserIdService now-1h,now*"),
                ("*help*", "Get the list of triggers for the bot"),
                ("*links*", "Get list of useful links"),
                ("*monitor links*", "Get links used to monitor a service (Grafana, etc)"),
                (":small_orange_diamond::small_orange_diamond:",":small_orange_diamond::small_orange_diamond:"),
                ("*Allowed Delimiters*", "between every parameter, can use this delimiters:* [,] [.] [ ] [+] *"),
            }.Aggregate(
                "",
                (curr, kvPair) => $"{curr}\n\n• {kvPair.HandlerTrigger}\n{kvPair.Description}"); ;
        }

        private string GetParamsString(string txt, string splitter)
        {
            return Regex.Split(txt, splitter, RegexOptions.IgnoreCase)[1];
        }

        private string GetEmptyResponseMsg(string parames)
        {
            return $"*Sorry :( , Could not find results using this parameters:*\n{parames}";
        }

        private string GetMissingParamsMsg(string action)
        {
            return
                $"*The parameters were invalid or not as expected*.\nPlease try again, and make it match exactly this structure:\n{action}";
        }


    }
}