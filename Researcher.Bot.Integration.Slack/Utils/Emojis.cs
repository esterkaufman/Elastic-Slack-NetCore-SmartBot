using System;
using System.Collections.Generic;
using System.Text;
using Researcher.Bot.Integration.Slack.Views;

namespace Researcher.Bot.Integration.Slack.Utils
{
    public static class Emojis
    {
        private static readonly string[] WorkingEmojis = new[]{ ":male-detective:",":blobthumbsup:"};

        public static string GetRandomWorkingEmoji()
        {
            var index = new Random().Next(0, WorkingEmojis.Length);
            return WorkingEmojis[index];
        }
    }
}
