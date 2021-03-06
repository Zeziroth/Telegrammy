﻿using Newtonsoft.Json.Linq;
using System;
using System.Text.RegularExpressions;

namespace MainWindow
{
    public static class Core
    {
        internal static long LongRandom(long min, long max)
        {
            Random rand = new Random();
            long result = rand.Next((Int32)(min >> 32), (Int32)(max >> 32));
            result = (result << 32);
            result = result | (long)rand.Next((Int32)min, (Int32)max);
            return result;
        }
        internal static bool isInnerWeek(string day)
        {
            switch (day.ToLower())
            {
                case "montag":
                case "dienstag":
                case "mittwoch":
                case "donnerstag":
                case "freitag":
                    return true;
                default:
                    return false;
            }
        }
        internal static string StripHTML(string input)
        {
            return Regex.Replace(input, "<.*?>", String.Empty);
        }
        internal static string GetTimestamp(DateTime value)
        {
            return value.ToString("yyyyMMddHHmmssffff");
        }

        internal static int IntRandom(int min, int max)
        {
            Random rand = new Random();
            int result = rand.Next(min, max);
            return result;
        }
        internal static bool ResultRandom(int percentage)
        {
            long result = IntRandom(0, 100);
            return result < percentage;
        }
        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
        public static double DateTimeToUnixTime()
        {
            return (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }

        public static decimal USD2EUR(decimal d)
        {
            return Math.Round(d / GetEURUSD(), 2);
        }

        public static decimal EUR2USD(decimal d)
        {
            return Math.Round(d * GetEURUSD(), 2);
        }

        private static decimal GetEURUSD()
        {
            string chart = HTTPRequester.SimpleRequest("http://data.fixer.io/api/latest?access_key=" + Settings.FIXER_IO_API_KEY + "&symbols=EUR,USD");
            dynamic chartJson = JObject.Parse(chart);
            return chartJson.rates.USD;
        }
    }
}
