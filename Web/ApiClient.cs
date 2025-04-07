using Blazored.LocalStorage;
using Communication.Requests;
using Communication.Responses;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Localization;
using Newtonsoft.Json;
using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Web.Authentication;

namespace Web
{
    public class ApiClient
    {
        private readonly HttpClient httpClient;
        private readonly ILocalStorageService localStorage;
        private readonly NavigationManager navigationManager;
        private readonly AuthenticationStateProvider authStateProvider;

        public ApiClient(
            HttpClient httpClient,
            ILocalStorageService localStorage,
            NavigationManager navigationManager,
            AuthenticationStateProvider authStateProvider)
        {
            this.httpClient = httpClient;
            this.localStorage = localStorage;
            this.navigationManager = navigationManager;
            this.authStateProvider = authStateProvider;
        }

        public async Task SetAuthorizeHeader()
        {
            try
            {
                var sessionState = await localStorage.GetItemAsync<ResponseAuthLogin>("sessionState");
                if (sessionState != null && !string.IsNullOrEmpty(sessionState.Token))
                {
                    if (sessionState.TokenExpired < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                    {
                        await ((CustomAuthStateProvider)authStateProvider).MarkUserAsLoggedOut();
                        navigationManager.NavigateTo("/login");
                    }
                    //else if (sessionState.TokenExpired < DateTimeOffset.UtcNow.AddMinutes(10).ToUnixTimeSeconds())
                    //{
                    //    var res = await httpClient.GetFromJsonAsync<RequestAuthLogin>($"/api/auth/loginByRefeshToken?refreshToken={sessionState.RefreshToken}");
                    //    if (res != null)
                    //    {
                    //        await ((CustomAuthStateProvider)authStateProvider).MarkUserAsAuthenticated(res);
                    //        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", res.Token);
                    //    }
                    //    else
                    //    {
                    //        await ((CustomAuthStateProvider)authStateProvider).MarkUserAsLoggedOut();
                    //        navigationManager.NavigateTo("/login");
                    //    }
                    //}
                    else
                    {
                        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessionState.Token);
                    }

                    var requestCulture = new RequestCulture(
                        CultureInfo.CurrentCulture,
                        CultureInfo.CurrentUICulture
                    );
                    var cultureCookieValue = CookieRequestCultureProvider.MakeCookieValue(requestCulture);

                    httpClient.DefaultRequestHeaders.Add("Cookie", $"{CookieRequestCultureProvider.DefaultCookieName}={cultureCookieValue}");
                }
            }
            catch
            {
                navigationManager.NavigateTo("/login");
            }
        }

        public async Task<T> GetFromJsonAsync<T>(string path)
        {
            await SetAuthorizeHeader();
            return await httpClient.GetFromJsonAsync<T>(path);
        }

        public async Task<T1> PostAsync<T1, T2>(string path, T2 postModel)
        {
            await SetAuthorizeHeader();

            var res = await httpClient.PostAsJsonAsync(path, postModel);
            if (res != null && res.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<T1>(await res.Content.ReadAsStringAsync());
            }
            return default;
        }

        public async Task<T1> PutAsync<T1, T2>(string path, T2 postModel)
        {
            await SetAuthorizeHeader();
            var res = await httpClient.PutAsJsonAsync(path, postModel);
            if (res != null && res.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<T1>(await res.Content.ReadAsStringAsync());
            }
            return default;
        }

        public async Task<T> DeleteAsync<T>(string path)
        {
            await SetAuthorizeHeader();
            return await httpClient.DeleteFromJsonAsync<T>(path);
        }
    }
}
