namespace back.Extensions
{
    public static class StringExtensions
    {
        public static bool IsValidYoutubeLink(this string link)
        {
            return !string.IsNullOrEmpty(link) && 
                  (link.Contains("youtube.com/watch") || 
                   link.Contains("youtu.be/"));
        }
        
        public static string TruncateWithEllipsis(this string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
                return text;
                
            return text.Substring(0, maxLength) + "...";
        }
    }
}