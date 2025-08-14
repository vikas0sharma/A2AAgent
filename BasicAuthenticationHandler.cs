using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
namespace A2AAgent
{

    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly string _user;
        private readonly string _password;
        public BasicAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            IConfiguration configuration)
            : base(options, logger, encoder)
        {
            _user = configuration["Authentication:Basic:UserName"]!;
            _password = configuration["Authentication:Basic:Password"]!;
        }

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Response.StatusCode = 401;
            var realm = "A2A";
            Response.Headers["WWW-Authenticate"] = $"Basic realm=\"{realm}\", charset=\"UTF-8\"";
            return Task.CompletedTask;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Expect header: Authorization: Basic base64(username:password)
            if (!Request.Headers.TryGetValue("Authorization", out var values))
            {
                return Task.FromResult(AuthenticateResult.Fail("Missing Authorization header."));
            }

            var authHeader = values.ToString();
            if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid or missing Authorization header."));
            }

            var encoded = authHeader["Basic ".Length..].Trim();
            string decoded;
            try
            {
                var bytes = Convert.FromBase64String(encoded);
                decoded = Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid Base64 encoding."));
            }

            var sepIndex = decoded.IndexOf(':');
            if (sepIndex <= 0)
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid Basic credential format."));
            }

            var username = decoded[..sepIndex];
            var password = decoded[(sepIndex + 1)..];

            var configured = new { Username = _user, Password = _password };
            if (string.IsNullOrEmpty(configured.Username) || string.IsNullOrEmpty(configured.Password))
            {
                return Task.FromResult(AuthenticateResult.Fail("Basic authentication not configured."));
            }

            if (!TimeConstantEquals(username, configured.Username) || !TimeConstantEquals(password, configured.Password))
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid username or password."));
            }

            var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, configured.Username),
            new Claim(ClaimTypes.Name, configured.Username)
        };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }

        private static bool TimeConstantEquals(string a, string b)
        {
            // Constant-time comparison to avoid timing attacks
            var aBytes = Encoding.UTF8.GetBytes(a);
            var bBytes = Encoding.UTF8.GetBytes(b);
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
