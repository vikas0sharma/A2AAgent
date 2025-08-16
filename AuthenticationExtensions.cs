using A2A;
using A2A.Models;
using A2A.Server.Infrastructure.Services;
using A2AAgent.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.SemanticKernel;
using RestEase;

namespace A2AAgent
{
    public static class AuthenticationExtensions
    {
        public static IServiceCollection ConfigureSemanticKernel(this IServiceCollection services, IConfiguration configuration)
        {
            //services.AddHuggingFaceChatCompletion(configuration["HuggingFace:ModelName"]!, new Uri(configuration["HuggingFace:BaseUrl"]!), configuration["HuggingFace:ApiKey"]!);
            services.AddGoogleAIGeminiChatCompletion(configuration["Google:ModelName"]!, apiKey: configuration["Google:ApiKey"]!);
            services.AddScoped<NewsPlugin>();
            services.AddScoped(s =>
            {
                var kernel = new Kernel(s);
                kernel.Plugins.AddFromObject(s.GetRequiredService<NewsPlugin>());
                return kernel;
            });

            return services;
        }

        public static IServiceCollection ConfigureNewsApi(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped(_ =>
            {
                INewsApi api = RestClient.For<INewsApi>(configuration["NewsApi:BaseUrl"]);
                api.ApiKey = configuration["NewsApi:ApiKey"]!;
                return api;
            });

            return services;
        }

        public static (SecurityScheme scheme, string schemeName) ConfigureAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var authType = configuration["Authentication:Type"]!;
            GenericSecuritySchemeBuilder securitySchemeBuilder = new();
            SecurityScheme scheme = null;
            string schemeName = null;

            switch (authType)
            {
                case "Basic":
                    // Add Basic authentication
                    services
                        .AddAuthentication("Basic")
                        .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("Basic", options => { });

                    schemeName = SecuritySchemeType.Http;
                    scheme = securitySchemeBuilder.UseHttp()
                        .WithScheme("basic")
                        .Build();

                    break;
                case "ApiKey":
                    services
                        .AddAuthentication("ApiKey")
                        .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>("ApiKey", options => { });

                    schemeName = SecuritySchemeType.ApiKey;
                    scheme = securitySchemeBuilder.UseApiKey()
                        .WithName(configuration["Authentication:ApiKey:HeaderName"]!)
                        .WithLocation("header")
                        .Build();

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

                    schemeName = SecuritySchemeType.OAuth2;
                    scheme = securitySchemeBuilder.UseOAuth2().WithClientCredentialsFlow(new OAuthFlow
                    {
                        AuthorizationUrl = new Uri(configuration["Authentication:OAuth2:AuthorizationEndpoint"]!),
                        TokenUrl = new Uri(configuration["Authentication:OAuth2:TokenEndpoint"]!),

                    }).Build();

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


                    schemeName = SecuritySchemeType.OpenIdConnect;
                    scheme = securitySchemeBuilder.UseOpenIdConnect()
                        .WithUrl(new Uri(configuration["Authentication:OpenIdConnect:OpenIdConnectUrl"]!))
                        .Build();
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported authentication type: {authType}");
            }

            return (scheme, schemeName);
        }
    }
}
