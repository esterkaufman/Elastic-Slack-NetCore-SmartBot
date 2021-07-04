using Newtonsoft.Json;
using Researcher.Bot.Integration.Slack.Interfaces;
using Researcher.Bot.Integration.Slack.Models.Events;
using Researcher.Bot.Integration.Slack.Models.Interactive;
using Researcher.Bot.Integration.Slack.Utils;
using Researcher.Bot.Integration.Slack.Views;
using System;
using System.Threading.Tasks;

namespace Researcher.Bot.Implementations.Slack
{
    public class AppHomeHandler : IAppHomeHandler
    {

        public Task<ViewPublishRequest> Handle(SlackApiMetaData slackApiMetaData, AppHomeEventBody payload)
        {
            var request = new ViewPublishRequest(payload.User)
            {
                View = new View
                {
                    Type = ViewsTypes.Home,
                    Blocks = new IBlock[]
                    {       
                        new Block {
                            type= "header",
                            text =  new Text{
                                text = "Hi! what's up?"
                            } 
                        },new SectionBlock() {
                            text =  new Text{
                                type = TextTypes.Markdown,
                                text = "I'm so excited. you're going to do an amazing things. \nThese are just a few things which you will be able to do:"
                            } 
                        },
                        new SectionBlock {
                            text= new Text {
                                type =   TextTypes.Markdown,
                                text = ResearcherBotRequestHandler.BuildDescription()
                            }
                        },
                        new SectionBlock() {
                            text= new Text {
                                type=  TextTypes.Markdown,
                                text= "So great to see you here and I'm going to help you stay up-to-date with any data on any service right here, at Slack."
                            }
                        },
                        new SectionBlock() {
                            text= new Text {
                                type=  TextTypes.Markdown,
                                text= "Thanks for joining us :blobwave:"
                            }
                        },
                        new DividerBlock() {
                        }
                        ,new SectionBlock() {
                            text= new Text {
                                type=  TextTypes.Markdown,
                                text= "Don't forget to check the *About* tab, and use the *Commands* there."
                            }
                        },
                        new Block {
                            type= "header",
                            text= new Text {
                                text= "Enjoy!"
                            }
                        }
                    }
                }
            };

            return Task.FromResult(request);
        }
    }
}
