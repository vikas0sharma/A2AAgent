using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace A2AAgent
{
    public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IConfiguration _configuration;

        public ApiKeyAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            IConfiguration configuration)
            : base(options, logger, encoder)
        {
            _configuration = configuration;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var headerName = _configuration["Authentication:ApiKey:HeaderName"]!;
            var expectedKey = _configuration["Authentication:ApiKey:Key"]!;

            if (!Request.Headers.TryGetValue(headerName, out var apiKeyHeader))
            {
                return Task.FromResult(AuthenticateResult.Fail($"Missing {headerName} header."));
            }

            var providedKey = apiKeyHeader.ToString();
            if (string.IsNullOrWhiteSpace(providedKey) || !TimeConstantEquals(providedKey, expectedKey))
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid API key."));
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "ApiKeyUser"),
                new Claim(ClaimTypes.Name, "ApiKeyUser")
            };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Response.StatusCode = 401;
            Response.Headers["WWW-Authenticate"] = "ApiKey";
            return Task.CompletedTask;
        }

        private static bool TimeConstantEquals(string a, string b)
        {
            var aBytes = System.Text.Encoding.UTF8.GetBytes(a);
            var bBytes = System.Text.Encoding.UTF8.GetBytes(b);
            if (aBytes.Length != bBytes.Length) return false;
            var result = 0;
            for (int i = 0; i < aBytes.Length; i++)
            {
                result |= aBytes[i] ^ bBytes[i];
            }
            return result == 0;
        }
    }
}
