using RestEase;

namespace A2AAgent.Services
{
    public interface INewsApi
    {
        [Header("X-Api-Key")]
        string ApiKey { get; set; }

        [Header("User-Agent", "A2A Agent/1.0.0")]
        string UserAgent { get; set; }

        [Get("top-headlines")]
        Task<TopHeadlinesResponseDto> GetHeadlinesResponseDto(
        [Query("country")] string country = "us",
        [Query("category")] string? category = null,
        [Query("q")] string? query = null,
        [Query("pageSize")] int pageSize = 20,
        [Query("page")] int page = 1);

        [Get("top-headlines/sources")]
        Task<NewsSourcesResponseDto> GetNewsSourcesResponseDto(
            [Query("category")] string? category = null,
            [Query("language")] string? language = null,
            [Query("country")] string? country = null
            );
    }
}
