using System;

namespace NetatmoLib
{
    public class DateTimeOps
    {
        /// <summary>
        ///     Convert unixTimestamp (integer) to DateTimeOffset
        /// </summary>
        public static DateTimeOffset GetDateTimeOffset(int unixTimestamp)
        {
            var timeStamp = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp);
            return timeStamp;
        }

        /// <summary>
        ///     Calculate the difference between "now" and the provided timestamp
        /// </summary>
        public static TimeSpan GetTimeDelta(DateTimeOffset timeStamp)
        {
            var now = DateTime.Now;
            var diff = now - timeStamp;
            return diff;
        }

        /// <summary>
        ///     Determine if the data is fresh by looking at timestamps
        ///     Fresh here means, not older than 20 minutes (by default).
        /// </summary>
        public static bool IsDataFresh(int unixTimestamp, int maxAge = 1200)
        {
            var timeStamp = DateTimeOps.GetDateTimeOffset(unixTimestamp);
            var diff = DateTimeOps.GetTimeDelta(timeStamp);
            if (diff.TotalSeconds < maxAge) return true;
            return false;
        }

        /// <summary>
        ///     Return a human readable string, such as "5 minutes" or "1 minute" etc.
        ///     based on the unix timestamp
        /// </summary>
        /// <param name="unixTimestamp"></param>
        /// <returns>String representation of last update time</returns>
        public static string GetLastUpdateString(int unixTimestamp)
        {
            var timeStamp = DateTimeOps.GetDateTimeOffset(unixTimestamp);
            var delta = DateTimeOps.GetTimeDelta(timeStamp);
            var humanReadableString = "unknown";
            var deltaMinutes = Math.Round(delta.TotalMinutes);
            if (delta.TotalSeconds < 60) humanReadableString = "less than a minute";
            else if (deltaMinutes == 1) humanReadableString = "a minute";
            else if ((deltaMinutes > 1 ) && (deltaMinutes  <= 60))  humanReadableString = $"{deltaMinutes} minutes";
            else if (deltaMinutes > 60) humanReadableString = $"more than an hour";
            return humanReadableString;
        }
    }
}