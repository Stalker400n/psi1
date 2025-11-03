using System;

namespace back.Exceptions
{
    public class YoutubeValidationException : Exception
    {
        public string InvalidLink { get; }

        public YoutubeValidationException(string link)
            : base($"Invalid YouTube link format: {link}")
        {
            InvalidLink = link;
        }

        public YoutubeValidationException(string link, string message)
            : base(message)
        {
            InvalidLink = link;
        }

        public YoutubeValidationException(string link, string message, Exception inner)
            : base(message, inner)
        {
            InvalidLink = link;
        }
    }
}