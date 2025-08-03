namespace BasicDLL.Utilities
{
    /// <summary>
    /// Utility class for common string operations
    /// </summary>
    public static class StringHelper
    {
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public static string Capitalize(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            return char.ToUpper(input[0]) + input[1..].ToLower();
        }

        public static string[] SplitAndTrim(string input, char separator)
        {
            if (string.IsNullOrWhiteSpace(input))
                return Array.Empty<string>();

            return input.Split(separator, StringSplitOptions.RemoveEmptyEntries)
                       .Select(s => s.Trim())
                       .Where(s => !string.IsNullOrEmpty(s))
                       .ToArray();
        }

        public static string MaskEmail(string email)
        {
            if (!IsValidEmail(email))
                return email;

            var parts = email.Split('@');
            var localPart = parts[0];
            var domain = parts[1];

            if (localPart.Length <= 2)
                return $"{localPart[0]}***@{domain}";

            return $"{localPart[0]}***{localPart[^1]}@{domain}";
        }
    }

    /// <summary>
    /// Utility class for date and time operations
    /// </summary>
    public static class DateTimeHelper
    {
        public static bool IsWeekend(DateTime date)
        {
            return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
        }

        public static int CalculateAge(DateTime birthDate)
        {
            var today = DateTime.Today;
            var age = today.Year - birthDate.Year;
            
            if (birthDate.Date > today.AddYears(-age))
                age--;
                
            return age;
        }

        public static string FormatRelativeTime(DateTime dateTime)
        {
            var timeSpan = DateTime.UtcNow - dateTime;

            if (timeSpan.TotalDays >= 365)
                return $"{(int)(timeSpan.TotalDays / 365)} year(s) ago";
            if (timeSpan.TotalDays >= 30)
                return $"{(int)(timeSpan.TotalDays / 30)} month(s) ago";
            if (timeSpan.TotalDays >= 1)
                return $"{(int)timeSpan.TotalDays} day(s) ago";
            if (timeSpan.TotalHours >= 1)
                return $"{(int)timeSpan.TotalHours} hour(s) ago";
            if (timeSpan.TotalMinutes >= 1)
                return $"{(int)timeSpan.TotalMinutes} minute(s) ago";
            
            return "Just now";
        }
    }
}
