using AspNetCore.Authentication.Basic;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Options;

namespace A2AAgent
{
    public class BasicUserValidationService : IBasicUserValidationService
    {
        public Task<bool> IsValidAsync(string username, string password)
        {
            // TODO: Replace with your own user validation logic
            // For demo: username = "a2auser", password = "a2apass"
            return Task.FromResult(username == "a2auser" && password == "a2apass");
        }
    }
}
