using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Oauth2.v2;
using Google.Apis.Services;
using MediatR;
using System.Diagnostics;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;

namespace PersonalAudioAssistant.Services
{
    public class GoogleUserService
    {
        private readonly IMediator _mediator;
        private readonly string _windowsClientId = "745633619579-s3kbqo3gqmj0kna5ksbrhv2s6tvqa3r2.apps.googleusercontent.com";
        private readonly string _androidClientId = "745633619579-f0utdblmj8fac1110o6isgav0km4kc8g.apps.googleusercontent.com";
        private readonly string _androidRedirectScheme = AppInfo.Current.PackageName;

        private Oauth2Service? _oauth2Service;
        private DriveService? _driveService;
        private GoogleCredential? _credential;
        private string? _email;

        public string? Email => _email;

        public async Task<TokenResponse> SignInAsync()
        {
            TokenResponse response = new();

            List<string> tokens;

            if (DeviceInfo.Current.Platform == DevicePlatform.WinUI)
            {
                response = await DoAuthCodeFlowWindowsAsync();
            }
            else if (DeviceInfo.Current.Platform == DevicePlatform.Android)
            {
                response = await DoAuthCodeFlowAndroidAsync();
            }
            else
            {
                throw new NotImplementedException($"Auth flow for platform {DeviceInfo.Current.Platform} not implemented.");
            }

            var accessToken = response.AccessToken;
            _credential = GoogleCredential.FromAccessToken(accessToken);
            _oauth2Service = new Oauth2Service(new BaseClientService.Initializer
            {
                HttpClientInitializer = _credential,
                ApplicationName = AppInfo.Current.Name
            });
            _driveService = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = _credential,
                ApplicationName = AppInfo.Current.Name
            });

            var userInfo = await _oauth2Service.Userinfo.Get().ExecuteAsync();
            _email = userInfo.Email;

            response.Email = _email;

            return response;
        }

        private async Task<TokenResponse> DoAuthCodeFlowWindowsAsync()
        {
            var authUrl = "https://accounts.google.com/o/oauth2/v2/auth";
            var clientId = _windowsClientId;
            var localPort = 12345;
            var redirectUri = $"http://localhost:{localPort}";
            var codeVerifier = GenerateCodeVerifier();
            var codeChallenge = GenerateCodeChallenge(codeVerifier);
            var parameters = GenerateAuthParameters(redirectUri, clientId, codeChallenge);
            var queryString = string.Join("&", parameters.Select(param => $"{param.Key}={param.Value}"));
            var fullAuthUrl = $"{authUrl}?{queryString}";

            await Launcher.OpenAsync(fullAuthUrl);
            var authorizationCode = await StartLocalHttpServerAsync(localPort);
            return await ExchangeAuthCodeForTokenAsync(authorizationCode, redirectUri, clientId, codeVerifier);
        }

        private async Task<TokenResponse> DoAuthCodeFlowAndroidAsync()
        {
            var authUrl = "https://accounts.google.com/o/oauth2/v2/auth";
            var clientId = _androidClientId;
            var redirectUri = $"{_androidRedirectScheme}://";
            var codeVerifier = GenerateCodeVerifier();
            var codeChallenge = GenerateCodeChallenge(codeVerifier);
            var parameters = GenerateAuthParameters(redirectUri, clientId, codeChallenge);
            var queryString = string.Join("&", parameters.Select(param => $"{param.Key}={param.Value}"));
            var fullAuthUrl = $"{authUrl}?{queryString}";

#pragma warning disable CA1416
            var authCodeResponse = await WebAuthenticator.AuthenticateAsync(new Uri(fullAuthUrl), new Uri(redirectUri));
#pragma warning restore CA1416
            var authorizationCode = authCodeResponse.Properties["code"];
            return await ExchangeAuthCodeForTokenAsync(authorizationCode, redirectUri, clientId, codeVerifier);
        }

        private static Dictionary<string, string> GenerateAuthParameters(string redirectUri, string clientId, string codeChallenge)
        {
            return new Dictionary<string, string>
            {
                { "scope", string.Join(' ', new[] { Oauth2Service.Scope.UserinfoProfile, Oauth2Service.Scope.UserinfoEmail, DriveService.Scope.Drive, DriveService.Scope.DriveFile, DriveService.Scope.DriveAppdata }) },
                { "access_type", "offline" },
                { "include_granted_scopes", "true" },
                { "response_type", "code" },
                { "redirect_uri", redirectUri },
                { "client_id", clientId },
                { "code_challenge_method", "S256" },
                { "code_challenge", codeChallenge }
            };
        }

        private static async Task<TokenResponse> ExchangeAuthCodeForTokenAsync(string authorizationCode, string redirectUri, string clientId, string codeVerifier)
        {
            var tokenEndpoint = "https://oauth2.googleapis.com/token";
            using var client = new HttpClient();
            var tokenRequest = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint)
            {
                Content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "authorization_code"),
                    new KeyValuePair<string, string>("code", authorizationCode),
                    new KeyValuePair<string, string>("redirect_uri", redirectUri),
                    new KeyValuePair<string, string>("client_id", clientId),
                    new KeyValuePair<string, string>("code_verifier", codeVerifier)
                })
            };

            var response = await client.SendAsync(tokenRequest);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Error requesting token: {responseBody}");

            var jsonToken = JsonObject.Parse(responseBody);
            var accessToken = jsonToken["access_token"]!.ToString();
            var refreshToken = jsonToken["refresh_token"]!.ToString();

            TokenResponse googleResponse = new()
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
            };

            return googleResponse;
        }

        private static async Task<string> StartLocalHttpServerAsync(int port)
        {
            using var listener = new HttpListener();
            listener.Prefixes.Add($"http://localhost:{port}/");
            listener.Start();
            Debug.WriteLine($"Listening on http://localhost:{port}/...");
            var context = await listener.GetContextAsync();
            var code = context.Request.QueryString["code"];
            var response = context.Response;
            var responseString = "Authorization complete. You can close this window.";
            var buffer = Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            await response.OutputStream.WriteAsync(buffer);
            response.OutputStream.Close();
            listener.Stop();
            if (code is null) throw new Exception("Authorization code not returned");
            return code;
        }

        private static string GenerateCodeVerifier()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[32];
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        private static string GenerateCodeChallenge(string codeVerifier)
        {
            var hash = SHA256.HashData(Encoding.ASCII.GetBytes(codeVerifier));
            return Convert.ToBase64String(hash)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }
    }

    public class TokenResponse
    {
        public string AccessToken {  get; set; }
        public string RefreshToken {  get; set; }
        public string Email {  get; set; }
    }
}
