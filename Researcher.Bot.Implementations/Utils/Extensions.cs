using System;
using System.Collections.Generic;
using System.Text;

namespace Researcher.Bot.Implementations.Utils
{
    public static class Extensions
    {
        public static string[] SplitWithDelimiters(this string v)
        {
            return v.Split(new Char[] { ',', '.', ' ' ,'+'},
                                 StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
