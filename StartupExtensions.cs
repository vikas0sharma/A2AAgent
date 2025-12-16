using A2A;
using A2AAgent.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using RestEase;

namespace A2AAgent
{
    public static class StartupExtensions
    {

        public static IServiceCollection ConfigureNewsApiService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton(_ =>
            {
                INewsApi api = RestClient.For<INewsApi>(configuration["NewsApi:BaseUrl"]);
                api.ApiKey = string.IsNullOrEmpty(configuration["NewsApi:ApiKey"]) ? Environment.GetEnvironmentVariable("NEWSAPI_APIKEY") : configuration["NewsApi:ApiKey"];
                return api;
            });
            services.AddSingleton<NewsPlugin>();
            return services;
        }

        public static KeyValuePair<string, SecurityScheme> ConfigureAuthentication(IServiceCollection services, IConfiguration configuration)
        {
            var authType = configuration["Authentication:Type"]!;
            SecurityScheme scheme = null;
            string schemeName = null;

            switch (authType)
            {
                case "Basic":
                    // Add Basic authentication
                    services
                        .AddAuthentication("Basic")
                        .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("Basic", options => { });

                    schemeName = "http";
                    scheme = new HttpAuthSecurityScheme("Basic");

                    break;
                case "ApiKey":
                    services
                        .AddAuthentication("ApiKey")
                        .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>("ApiKey", options => { });

                    schemeName = "apiKey";
                    scheme = new ApiKeySecurityScheme(configuration["Authentication:ApiKey:HeaderName"]!, "header");

                    break;
                case "OAuth2":

                    services
                        .AddAuthentication(options =>
                        {
                            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                        }).AddJwtBearer(options =>
                        {
                            options.Authority = configuration["Authentication:OAuth2:AuthorizationEndpoint"];
                            options.Audience = configuration["Authentication:OAuth2:Audience"]; ;
                        });

                    schemeName = "oauth2";
                    scheme = new OAuth2SecurityScheme(new OAuthFlows
                    {
                        ClientCredentials = new ClientCredentialsOAuthFlow(new Uri(configuration["Authentication:OAuth2:TokenEndpoint"]!), new Dictionary<string, string>()) { }
                    });

                    break;
                case "OpenIdConnect":

                    services
                        .AddAuthentication(options =>
                        {
                            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                        }).AddJwtBearer(options =>
                        {
                            options.Authority = configuration["Authentication:OpenIdConnect:AuthorizationEndpoint"];
                            options.Audience = configuration["Authentication:OpenIdConnect:Audience"]; ;
                        });


                    schemeName = "openIdConnect";
                    scheme = new OpenIdConnectSecurityScheme(new Uri(configuration["Authentication:OpenIdConnect:OpenIdConnectUrl"]!));
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported authentication type: {authType}");
            }

            return new KeyValuePair<string, SecurityScheme>(schemeName, scheme);
        }
    }
}
