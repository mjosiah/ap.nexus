namespace ap.nexus.agents.website.Extensions
{
    /// <summary>
    /// Extension methods for DateTime to provide user-friendly string representations
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Converts a DateTime to a friendly string like "just now", "5 minutes ago", etc.
        /// </summary>
        /// <param name="dateTime">The DateTime to convert</param>
        /// <returns>A user-friendly string representation of the time elapsed</returns>
        public static string ToFriendlyString(this DateTime dateTime)
        {
            var now = DateTime.Now;
            var timeSpan = now - dateTime;

            // Future dates
            if (timeSpan.TotalSeconds < 0)
            {
                return "just now";
            }

            // Less than a minute
            if (timeSpan.TotalSeconds < 60)
            {
                return "just now";
            }

            // Less than an hour
            if (timeSpan.TotalMinutes < 60)
            {
                var minutes = (int)timeSpan.TotalMinutes;
                return $"{minutes} {(minutes == 1 ? "minute" : "minutes")} ago";
            }

            // Less than a day
            if (timeSpan.TotalHours < 24)
            {
                var hours = (int)timeSpan.TotalHours;
                return $"{hours} {(hours == 1 ? "hour" : "hours")} ago";
            }

            // Yesterday
            if (dateTime.Date == now.Date.AddDays(-1))
            {
                return "yesterday";
            }

            // Less than a week
            if (timeSpan.TotalDays < 7)
            {
                var days = (int)timeSpan.TotalDays;
                return $"{days} {(days == 1 ? "day" : "days")} ago";
            }

            // Less than a month
            if (timeSpan.TotalDays < 30)
            {
                var weeks = (int)(timeSpan.TotalDays / 7);
                return $"{weeks} {(weeks == 1 ? "week" : "weeks")} ago";
            }

            // Less than a year
            if (timeSpan.TotalDays < 365)
            {
                var months = (int)(timeSpan.TotalDays / 30);
                return $"{months} {(months == 1 ? "month" : "months")} ago";
            }

            // More than a year
            var years = (int)(timeSpan.TotalDays / 365);
            return $"{years} {(years == 1 ? "year" : "years")} ago";
        }

        /// <summary>
        /// Formats a DateTime as a short date and/or time string depending on how recent it is
        /// </summary>
        /// <param name="dateTime">The DateTime to format</param>
        /// <returns>A formatted date/time string</returns>
        public static string ToShortFriendlyString(this DateTime dateTime)
        {
            var now = DateTime.Now;

            // Today - just show time
            if (dateTime.Date == now.Date)
            {
                return dateTime.ToString("h:mm tt");
            }

            // Yesterday
            if (dateTime.Date == now.Date.AddDays(-1))
            {
                return "Yesterday";
            }

            // Within last week
            if ((now.Date - dateTime.Date).TotalDays < 7)
            {
                return dateTime.ToString("dddd"); // Day name
            }

            // This year
            if (dateTime.Year == now.Year)
            {
                return dateTime.ToString("MMM d"); // Month day
            }

            // Different year
            return dateTime.ToString("MMM d, yyyy"); // Month day, year
        }
    }
}
