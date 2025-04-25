using EtsyWooSync.Inerface;
using EtsyWooSync.Models.EtsyModels;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EtsyWooSync.Services
{
    namespace EtsyWooSync.Services
    {
        public class EtsyOAuthService : IEtsyOAuthService
        {
            private readonly HttpClient _httpClient;
            private readonly IConfiguration _configuration;

            private string _clientId;
            private string _clientSecret;
            private string _redirectUri;

            public EtsyOAuthService(HttpClient httpClient, IConfiguration configuration)
            {
                _httpClient = httpClient;
                _configuration = configuration;

                // načteme konfiguraci
                _clientId = _configuration["EtsyOAuth:ClientId"];
                _clientSecret = _configuration["EtsyOAuth:ClientSecret"];
                _redirectUri = _configuration["EtsyOAuth:RedirectUri"];
            }

            public string GetAuthorizationUrl(string state)
            {
                var scopes = _configuration["EtsyOAuth:Scopes"] ?? "listings_r";
                return $"https://www.etsy.com/oauth/connect?response_type=code&redirect_uri={Uri.EscapeDataString(_redirectUri)}&scope={Uri.EscapeDataString(scopes)}&client_id={_clientId}&state={state}";
            }

            public async Task ExchangeCodeForTokenAsync(string code)
            {
                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.etsy.com/v3/public/oauth/token");

                var content = new FormUrlEncodedContent(new[]
                {
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("client_id", _clientId),
                new KeyValuePair<string, string>("redirect_uri", _redirectUri),
                new KeyValuePair<string, string>("code", code)
            });

                request.Content = content;

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    throw new ApplicationException($"Etsy token exchange failed: {response.StatusCode}, {error}");
                }

                var responseStream = await response.Content.ReadAsStreamAsync();
                var tokenData = await JsonSerializer.DeserializeAsync<EtsyTokenResponse>(responseStream);

                // TODO: uložit tokenData do úložiště (zatím jen výpis)
                Console.WriteLine($"AccessToken: {tokenData?.access_token}");
                Console.WriteLine($"RefreshToken: {tokenData?.refresh_token}");
            }

            public async Task<string> GetAccessTokenAsync()
            {
                // Placeholder – vrací zatím prázdný string
                return await Task.FromResult(string.Empty);
            }

            public async Task<bool> RefreshTokenAsync()
            {
                // Placeholder – přidáme později
                return await Task.FromResult(false);
            }
        }
    }
}
