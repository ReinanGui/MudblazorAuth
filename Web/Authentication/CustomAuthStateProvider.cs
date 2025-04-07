using Blazored.LocalStorage;
using Communication.Responses;
using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Web.Authentication
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage;

        public CustomAuthStateProvider(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var sessionModel = await _localStorage.GetItemAsync<ResponseAuthLogin>("sessionState");

                var identity = sessionModel == null
                    ? new ClaimsIdentity()
                    : GetClaimsIdentity(sessionModel.Token);

                var user = new ClaimsPrincipal(identity);
                return new AuthenticationState(user);
            }
            catch
            {
                await MarkUserAsLoggedOut();
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }

        public async Task MarkUserAsAuthenticated(ResponseAuthLogin model)
        {
            await _localStorage.SetItemAsync("sessionState", model);

            var identity = GetClaimsIdentity(model.Token);
            var user = new ClaimsPrincipal(identity);

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }

        public async Task MarkUserAsLoggedOut()
        {
            await _localStorage.RemoveItemAsync("sessionState");

            var identity = new ClaimsIdentity();
            var user = new ClaimsPrincipal(identity);

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }

        private ClaimsIdentity GetClaimsIdentity(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var claims = jwtToken.Claims;
            return new ClaimsIdentity(claims, "jwt");
        }
    }
}
