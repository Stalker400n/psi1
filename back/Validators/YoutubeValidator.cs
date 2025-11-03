using System;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using back.Exceptions;

namespace back.Validators
{
    public interface IYoutubeValidator
    {
        Task ValidateLink(string link);
    }

    public class YoutubeValidator : IYoutubeValidator
    {
        private readonly ILogger<YoutubeValidator> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private static readonly Regex YoutubeVideoIdRegex = new(
            @"(?:youtube\.com\/(?:[^\/\n\s]+\/\S+\/|(?:v|e(?:mbed)?)\/|\S*?[?&]v=)|youtu\.be\/)([a-zA-Z0-9_-]{11})",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public YoutubeValidator(ILogger<YoutubeValidator> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task ValidateLink(string link)
        {
            try
            {
                if (string.IsNullOrEmpty(link))
                {
                    _logger.LogWarning("Empty YouTube link received");
                    throw new YoutubeValidationException(link, "YouTube link cannot be empty");
                }

                var videoId = ExtractVideoId(link);
                if (videoId == null)
                {
                    _logger.LogWarning("Invalid YouTube link format: {Link}", link);
                    throw new YoutubeValidationException(link, "Invalid YouTube link format. Could not extract video ID.");
                }

                await ValidateVideoAccessibility(videoId, link);
            }
            catch (Exception ex) when (ex is not YoutubeValidationException)
            {
                _logger.LogError(ex, "Unexpected error during YouTube link validation: {Link}", link);
                throw new YoutubeValidationException(link, "An unexpected error occurred while validating the YouTube link", ex);
            }
        }

        private string? ExtractVideoId(string link)
        {
            var match = YoutubeVideoIdRegex.Match(link);
            return match.Success ? match.Groups[1].Value : null;
        }

        private async Task ValidateVideoAccessibility(string videoId, string originalLink)
        {
            try
            {
                using var client = _httpClientFactory.CreateClient();
                var response = await client.GetAsync($"https://img.youtube.com/vi/{videoId}/0.jpg");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("YouTube video not accessible: {VideoId}", videoId);
                    throw new YoutubeValidationException(originalLink,
                        "The YouTube video appears to be private, deleted, or not available.");
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "Failed to validate YouTube video accessibility: {VideoId}", videoId);
                throw new YoutubeValidationException(originalLink,
                    "Could not verify if the YouTube video is accessible. Please check if the video exists and is public.");
            }
        }
    }
}