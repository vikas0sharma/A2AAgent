using Microsoft.SemanticKernel;
using RestEase;
using System.ComponentModel;

namespace A2AAgent.Services
{
    public class NewsPlugin(INewsApi _api, ILogger<NewsPlugin> _logger)
    {
        [KernelFunction("get_top_headlines")]
        [Description("Gets live top and breaking headlines for a country, specific category in a country")]
        public async Task<TopHeadlinesResponseDto> GetTopHeadlinesAsync(
            [Description("The 2-letter ISO 3166-1 code of the country you want to get headlines for")] string country = "us",
            [Description("The category you want to get headlines for. Possible options: business,entertainment,general,health,science,sports,technology")] string? category = null,
            [Description("Keywords or a phrase to search for.")] string? query = null,
            int pageSize = 20,
            int page = 1)
        {
            try
            {
                _logger.LogInformation($"GetTopHeadlinesAsync(country:{country}, category:{category}, query:{query})");
                var response = await _api.GetHeadlinesResponseDto(country, category, query, pageSize, page);
                return response;
            }
            catch (ApiException e)
            {
                _logger.LogError($"API call failed: {e.Content}|{e.Message}");
                return null;
            }
        }
    }
}
