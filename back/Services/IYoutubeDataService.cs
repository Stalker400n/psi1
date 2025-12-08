using System.Threading.Tasks;

namespace back.Services
{
    public interface IYoutubeDataService
    {
        Task<YouTubeVideoData> GetVideoDataAsync(string videoUrl);
    }

    public class YouTubeVideoData
    {
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string ThumbnailUrl { get; set; } = string.Empty;
        public int DurationSeconds { get; set; } = 0;
    }
}
