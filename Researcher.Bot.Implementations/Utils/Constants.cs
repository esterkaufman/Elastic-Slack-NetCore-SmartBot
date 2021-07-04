using System;
using System.Collections.Generic;
using System.Text;
using Researcher.Bot.Integration.ElasticSearch.Models;

namespace Researcher.Bot.Implementations.Utils
{
    public static class SlashCommandsEnum
    {
        public const string GUI = "/gui";
        public const string LogDog = "/logdog";
        public const string GetStatistics = "/get-statistics";
        public const string GetBlamer = "/get-blamer";
        public const string Links = "/links";
        public const string MonitorLinks = "/monitor-links";
    }

    public static class BotActionsEnum
    {
        public const string ACTION_GUI = "gui";
        public const string ACTION_LINKS = "links";
        public const string ACTION_MONITOR_LINKS = "monitor links";


        public const string ACTION_LOG_DOG = "logdog ";
        public const string ACTION_LOG_DOG_TEMPLATE = "*logdog* [callID] [dc (default='_all')]";

        public const string ACTION_GET_BLAMER = "get blamer ";
        public const string ACTION_GET_BLAMER_TEMPLATE = "*get blamer* [callID] [dc (default='_all')]";

        public const string ACTION_GET_STATS = "get stats ";
        public const string ACTION_GET_STATS_TEMPLATE = "*get stats* [dc] [service] [from (default='now-1h')] [to (default='now')]";

        //public const string ACTION_GET_AMOUNT_TEMPLATE = "Get amount [dc] [service] [query=('requests','errors','status')]";

    }
}
