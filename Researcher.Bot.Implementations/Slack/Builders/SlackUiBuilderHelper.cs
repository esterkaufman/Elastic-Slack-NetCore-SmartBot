using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Researcher.Bot.Implementations.Utils;
using Researcher.Bot.Integration.ElasticSearch.Models;
using Researcher.Bot.Integration.Slack.Models.Interactive;

namespace Researcher.Bot.Implementations.Slack.Builders
{
    public static class SlackUiBuilderHelper
    {
        public static string GetLogDogUiResponse(Integration.ElasticSearch.Models.LogDogEsResponse logDogEsResponse, string dc, string callId, IList<IBlock> blocks, IList<Attachment> attachments)
        {
            var title = $":page_facing_up:LogDog for DC.CallId: {dc ?? "All Dcs"}.{callId}: \n\n";

            blocks.Add(new SectionBlock { text = new Text { type = TextTypes.Markdown, text = $"{title}" } });
            blocks.Add(new SectionBlock { text = new Text { type = TextTypes.Markdown, text = $"*Found in Dc:* {logDogEsResponse.Dc}" } });
            blocks.Add(new SectionBlock { text = new Text { type = TextTypes.Markdown, text = $"*CallId:* {logDogEsResponse.CallId}" } });
            blocks.Add(new SectionBlock { text = new Text { type = TextTypes.Markdown, text = $"*ErrorCode:* {logDogEsResponse.ErrorCode}" } });
            blocks.Add(new SectionBlock { text = new Text { type = TextTypes.Markdown, text = $"*ErrorMessage:* {logDogEsResponse.ErrorMessage}" } });
            blocks.Add(new SectionBlock { text = new Text { type = TextTypes.Markdown, text = $"*ServiceNames:* {logDogEsResponse.ServiceNames}" } });
            blocks.Add(new SectionBlock { text = new Text { type = TextTypes.Markdown, text = $"*Endpoint:* {logDogEsResponse.Endpoint}" } });
            blocks.Add(new SectionBlock { text = new Text { type = TextTypes.Markdown, text = $"*Time:* {logDogEsResponse.Time}" } });
            blocks.Add(new SectionBlock { text = new Text { type = TextTypes.Markdown, text = $"*EndTime:* {logDogEsResponse.EndTime}" } });
            blocks.Add(new SectionBlock { text = new Text { type = TextTypes.Markdown, text = $"*Duration:* {logDogEsResponse.Duration}" } });
            blocks.Add(new SectionBlock { text = new Text { type = TextTypes.Markdown, text = $"*Logs:* {logDogEsResponse.Logs}" } });
            blocks.Add(new SectionBlock { text = new Text { type = TextTypes.Markdown, text = $"\n:small_orange_diamond::small_orange_diamond::small_orange_diamond:\n" } });
            
            blocks.Add(new DividerBlock());
            blocks.Add(new ActionsBlock()
            {
                elements = new[]
                {
                    new ButtonElement()
                    {
                        text= new Text {
                            text="View in LogDog"
                        },
                        url= $"http://build1.gigya.net/LogDog/show-log.html?callID={callId}"
                    },
                    new ButtonElement()
                    {
                        text= new Text {
                            text="View in Kibana"
                        },
                        url= $"http://kibana.gigya.net/kibana3/#/dashboard/elasticsearch/logdog_dashboard?query=callID:{callId}&from={logDogEsResponse.Time}&to={logDogEsResponse.EndTime}"
                    }
                }
            });
            attachments.Add(new Attachment
            {
                blocks = logDogEsResponse.Errors.Select(line => new SectionBlock()
                {
                    text = new Text { type = TextTypes.Markdown, text = line }

                }).ToArray()

            });



            return title;
        }

        public static string GetNocBlamerUiResponse(BlamerEsResponse blamerRes, string dc, string callId, IList<IBlock> blocks, IList<Attachment> attachments)
        {
            var title =
                $"Hi NOC, I found the service to blame,:github-check-mark: \nFor DC.CallId: {dc??"All Dcs"}, callID: {callId} \n\n";

            blocks.Add(new SectionBlock { text = new Text { type = TextTypes.Markdown, text = $"{title}" } });
            blocks.Add(new SectionBlock { text = new Text { type = TextTypes.Markdown, text = $"*Found in Dc:* {blamerRes.Dc}" } });
            blocks.Add(new SectionBlock { text = new Text { type = TextTypes.Markdown, text = $"*Service:* {blamerRes.ServiceName}" } });
            blocks.Add(new SectionBlock { text = new Text { type = TextTypes.Markdown, text = $"*Endpoint:* {blamerRes.Endpoint}" } });
            blocks.Add(new SectionBlock { text = new Text { type = TextTypes.Markdown, text = $"*Type:* {blamerRes.ExceptionType}" } });
            blocks.Add(new SectionBlock { text = new Text { type = TextTypes.Markdown, text = $"*Time:* {blamerRes.Time}" } });
            blocks.Add(new SectionBlock { text = new Text { type = TextTypes.Markdown, text = $"*Exception Message:* {Regex.Replace(blamerRes.ExceptionMessage, "(?<=^.{470}).*", "...")}" } });
            blocks.Add(new SectionBlock { text = new Text { type = TextTypes.Markdown, text = $"\n:small_orange_diamond::small_orange_diamond::small_orange_diamond:\n" } });
            blocks.Add(new DividerBlock());
            attachments.Add(new Attachment
            {
                blocks = blamerRes.CallsStack.Select(line => new SectionBlock()
                {
                    text = new Text { type = TextTypes.Markdown, text = line }

                }).ToArray()

            });

            return title;
        }

        public static string GetHelpUiResponse(string actionsDescription, List<IBlock> blocks, List<Attachment> attachments)
        {
            var title = "Hi! :grinning: I'm a smart bot, and i can help with the following commands list.\nDon't forget to mention me first with an @.";

            blocks.Add(new SectionBlock() { text = new Text { type = TextTypes.Markdown, text = $"{title}" } });
            blocks.Add(new SectionBlock() { text = new Text { type = TextTypes.Markdown, text = $"\n:small_orange_diamond::small_orange_diamond::small_orange_diamond:\n" } });
            blocks.Add(new DividerBlock());
            attachments.Add(new Attachment
            {
                blocks = new[]{ new SectionBlock()
                {
                    text = new Text { type =  TextTypes.Markdown, text = actionsDescription }

                }}

            });
            return title;
        }

        public static string GetGUIActionsModal(List<IBlock> blocks)
        {
            var title = "Run Bot Actions";

            blocks.Add(new SectionBlock() { text = new Text { type = TextTypes.Markdown, text = ":bulb: Select an action and fill related parameters \n(*Datacenter* and *CallId* might be mandatory, *Time period * is optional" }, });
            blocks.Add(new DividerBlock());
            blocks.Add(new InputBlock()
            {

                element = new StaticSelectElement()
                {
                    placeholder = new Text() { text = "Select action" },
                    options = new[]
                    {
                        new Option(){text = new Text(){emoji = true,text = "Get Useful Links" },value = BotActionsEnum.ACTION_LINKS},
                        new Option(){text = new Text(){emoji = true,text = "Get Monitoring Service Links" },value = BotActionsEnum.ACTION_MONITOR_LINKS},
                        new Option(){text = new Text(){emoji = true,text = "Get NOC Blamer Service (for a callID)" },value = BotActionsEnum.ACTION_GET_BLAMER},
                        new Option(){text = new Text(){emoji = true,text = "Call LogDog" },value = BotActionsEnum.ACTION_LOG_DOG},
                        //new Option(){text = new Text(){emoji = true,text = "Get service statistics" },value = BotActionsEnum.ACTION_GET_STATS} //need to add new input for serviceName
                    },
                    action_id = "action"

                },
                label = new Text() { text = "Action" }
            });
            blocks.Add(new DividerBlock());
            blocks.Add(new InputBlock()
            {

                element = new StaticSelectElement()
                {
                    placeholder = new Text() { text = "Select datacenter" },
                    options = new[]
                {
                    new Option(){text = new Text(){emoji=true,text = ":earth_africa:  Global" },value = "all"},
                    new Option(){text = new Text(){emoji=true,text = ":flag-us: USA US1D" },value = "us1d"},
                    new Option(){text = new Text(){emoji=true,text = ":flag-cn: China CN1E" },value = "cn1e"},
                    new Option(){text = new Text(){emoji=true,text = ":flag-cn: China CN1F" },value = "cn1f"},
                    new Option(){text = new Text(){emoji=true,text = ":flag-eu: Europe EU1" },value = "eu1"},
                    new Option(){text = new Text(){emoji=true,text = ":flag-eu: Europe EU5 (CDP)" },value = "eu5"},
                    new Option(){text = new Text(){emoji=true,text = ":flag-au: Australia AU1" },value = "au1"},
                    new Option(){text = new Text(){emoji=true,text = ":flag-ru: Russia RU1E" },value = "ru1e"},
                    new Option(){text = new Text(){emoji=true,text = ":flag-ru: Russia RU1F" },value = "ru1f"},
                },
                    action_id = "dc"

                },
                label = new Text() { text = "Datacenter" }
            });
            blocks.Add(new InputBlock()
            {
                element = new PlainTextInputElement() { initial_value = "", action_id = "callId" },
                label = new Text() { text = "Call ID" }

            });
            blocks.Add(new DividerBlock());
            blocks.Add(new InputBlock()
            {
                element = new PlainTextInputElement() { initial_value = "now-1h", action_id = "from" },
                label = new Text() {text = "From Date" }

            });
            blocks.Add(new InputBlock()
            {
                element = new PlainTextInputElement() { initial_value = "now", action_id = "to" },
                label = new Text() {text = "To Date" }

            });

            return title;

        }

        public static string GetOpenGUIActionsButton(List<IBlock> blocks)
        {
            var title = "Get all Researcher-Bot actions in popup";

            blocks.Add(new SectionBlock()
            {
                text = new Text { type = TextTypes.Markdown, text = $"{title}" },
                accessory = new ButtonElement()
                {
                    text = new Text { emoji=true,text = ":small_blue_diamond: Open :small_blue_diamond:" },
                    value = "gui",
                    action_id = "gui"
                }
            });
            return title;
        }
    }
}
