using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace back.Services
{
    public class YoutubeDataService : IYoutubeDataService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<YoutubeDataService> _logger;

        public YoutubeDataService(IHttpClientFactory httpClientFactory, ILogger<YoutubeDataService> logger)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<YouTubeVideoData> GetVideoDataAsync(string videoUrl)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var oEmbedUrl = $"https://www.youtube.com/oembed?url={Uri.EscapeDataString(videoUrl)}&format=json";
                
                var response = await client.GetAsync(oEmbedUrl);
                response.EnsureSuccessStatusCode();
                
                var json = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<OEmbedResponse>(json, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });

                if (data == null)
                {
                    throw new InvalidOperationException("Failed to deserialize YouTube oEmbed response");
                }
                
                return new YouTubeVideoData
                {
                    Title = data.Title ?? "Unknown Title",
                    Author = data.AuthorName ?? "Unknown Artist",
                    ThumbnailUrl = data.ThumbnailUrl ?? string.Empty,
                    
                    // oEmbed doesn't provide duration -> if we want this we should think about using different API
                    DurationSeconds = 0
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed when extracting YouTube data for {Url}", videoUrl);
                throw new Exception("Failed to connect to YouTube API", ex);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse YouTube API response for {Url}", videoUrl);
                throw new Exception("Failed to parse YouTube API response", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to extract YouTube data for {Url}", videoUrl);
                throw;
            }
        }
    }

    internal class OEmbedResponse
    {
        [JsonPropertyName("title")]
        public string? Title { get; set; }
        
        [JsonPropertyName("author_name")]
        public string? AuthorName { get; set; }
        
        [JsonPropertyName("thumbnail_url")]
        public string? ThumbnailUrl { get; set; }
    }
}
