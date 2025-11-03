using System;
using Microsoft.Extensions.Logging;
using back.Exceptions;

namespace back.Validators
{
    public interface IYoutubeValidator
    {
        void ValidateLink(string link);
    }

    public class YoutubeValidator : IYoutubeValidator
    {
        private readonly ILogger<YoutubeValidator> _logger;

        public YoutubeValidator(ILogger<YoutubeValidator> logger)
        {
            _logger = logger;
        }

        public void ValidateLink(string link)
        {
            try
            {
                if (string.IsNullOrEmpty(link))
                {
                    _logger.LogWarning("Empty YouTube link received");
                    throw new YoutubeValidationException(link, "YouTube link cannot be empty");
                }

                if (!link.Contains("youtube.com/watch") && !link.Contains("youtu.be/"))
                {
                    _logger.LogWarning("Invalid YouTube link format: {Link}", link);
                    throw new YoutubeValidationException(link, "Invalid YouTube link format. Must contain 'youtube.com/watch' or 'youtu.be/'");
                }
            }
            catch (Exception ex) when (ex is not YoutubeValidationException)
            {
                _logger.LogError(ex, "Unexpected error during YouTube link validation: {Link}", link);
                throw new YoutubeValidationException(link, "An unexpected error occurred while validating the YouTube link", ex);
            }
        }
    }
}