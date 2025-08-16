namespace A2AAgent.Services
{
    public class TopHeadlinesResponseDto
    {
        public string Status { get; set; }                // "ok" or "error"
        public int TotalResults { get; set; }             // Total number of results
        public List<ArticleDto> Articles { get; set; }    // List of articles
    }

    public class ArticleDto
    {
        public SourceDto Source { get; set; }             // Source details
        public string Author { get; set; }                // Author of the article
        public string Title { get; set; }                 // Headline or title
        public string Description { get; set; }           // Article description/snippet
        public string Url { get; set; }                   // Direct URL to article
        public string UrlToImage { get; set; }            // Relevant image URL
        public DateTime PublishedAt { get; set; }         // Published date & time (UTC)
        public string Content { get; set; }               // Unformatted content (truncated to 200 chars)
    }

    public class NewsSourcesResponseDto
    {
        public string Status { get; set; }               // "ok" or "error"
        public List<SourceDto> Sources { get; set; }     // List of news sources
    }

    public class SourceDto
    {
        public string Id { get; set; }                   // Source identifier
        public string Name { get; set; }                 // Name of the news source
        public string Description { get; set; }          // Description of the news source
        public string Url { get; set; }                  // Homepage URL
        public string Category { get; set; }             // Type/category of news
        public string Language { get; set; }             // Language code
        public string Country { get; set; }              // Country code
    }
}
