using MediatR;
using PersonalAudioAssistant.Services.Api;

namespace PersonalAudioAssistant.Services
{
    public class AuthTokenManager
    {
        private readonly GoogleUserService _googleAuthService;
        private readonly IMediator _mediator;
        private readonly AuthApiClient _authApiClient;

        public event EventHandler<bool> UserSignInStatusChanged;

        private void OnUserSignInStatusChanged(bool isSignedIn)
        {
            UserSignInStatusChanged?.Invoke(this, isSignedIn);
        }

        public async Task<bool> IsSignedInAsync()
        {
            return await CheckIfUserIsSignedIn();
        }

        public AuthTokenManager(GoogleUserService googleAuthService, IMediator mediator, AuthApiClient authApiClient)
        {
            _googleAuthService = googleAuthService;
            _mediator = mediator;
            _authApiClient = authApiClient;
        }

        public async Task InitializeAsync()
        {
            var refreshToken = await GetRefreshTokenAsync();
            var a = await IsAccessTokenExpired();
            var b = !string.IsNullOrEmpty(refreshToken);
            if (a && b)
            {
                await RefreshTokenAsync();
            }
        }

        public async Task Sign_In_Up_AsyncGoogle()
        {
            var response = await _googleAuthService.SignInAsync();
            DateTime expiryTime = DateTime.UtcNow.AddHours(1);

            var userId = await _authApiClient.LoginWithGoogleAsync(response.Email, response.RefreshToken);

            await SecureStorage.SetAsync("is_google", "true");

            await SignIn(response, userId, expiryTime);
        }

        public async Task SignInWithPasswordAsync(string email, string password)
        {
            var tokens = await _authApiClient.LoginAsync(email, password);

            TokenResponse response = new()
            {
                AccessToken = tokens.AccessToken,
                RefreshToken = tokens.RefreshToken,
                Email = email
            };

            if (response == null || string.IsNullOrEmpty(response.AccessToken))
            {
                throw new Exception("Помилка авторизації. Перевірте введені дані.");
            }
            await SecureStorage.SetAsync("is_google", "false");

            await SignIn(response, tokens.UserId, tokens.RefreshExpiresAt);
        }

        private async Task SignIn(TokenResponse response, string userId, DateTimeOffset expiryTime)
        {
            if (response.RefreshToken == null || response.AccessToken == null || response.Email == null)
            {
                throw new Exception("Недостатньо даних для збереження токенів.");
            }

            await SaveTokensAsync(response.AccessToken, response.RefreshToken, expiryTime);
            await SecureStorage.SetAsync("user_email", response.Email);
            await SecureStorage.SetAsync("user_id", userId);

            OnUserSignInStatusChanged(true);
        }

        public async Task SignUpWithPasswordAsync(string email, string password)
        {
            var response = await _authApiClient.RegistrationAsync(email, password);
            
            TokenResponse tokenResponse = new()
            {
                AccessToken = response.AccessToken,
                RefreshToken = response.RefreshToken,
                Email = email
            };
            await SecureStorage.SetAsync("is_google", "false");

            await SignIn(tokenResponse, response.Id, response.RefreshExpiresAt);
        }

        public async Task SignOutAsync()
        {
            SecureStorage.Remove("access_token");
            SecureStorage.Remove("refresh_token");
            SecureStorage.Remove("access_token_expires_at");
            SecureStorage.Remove("user_email");
            SecureStorage.Remove("user_id");

            OnUserSignInStatusChanged(false);
            await Task.CompletedTask;
        }

        public async Task<string?> GetAccessTokenAsync()
        {
            if (await IsAccessTokenExpired())
            {
                await RefreshTokenAsync();
            }
            try
            {
                return await SecureStorage.GetAsync("access_token");
            }
            catch (Exception)
            {
                return null;
            }
        }

        private async Task<string?> GetRefreshTokenAsync()
        {
            try
            {
                return await SecureStorage.GetAsync("refresh_token");
            }
            catch (Exception)
            {
                return null;
            }
        }

        private async Task RefreshTokenAsync()
        {
            var currentAccessToken = await SecureStorage.GetAsync("access_token");
            var currentRefreshToken = await GetRefreshTokenAsync();

            if (string.IsNullOrEmpty(currentAccessToken) || string.IsNullOrEmpty(currentRefreshToken))
            {
                throw new Exception("Неможливо оновити токени, оскільки відсутні дані для оновлення.");
            }

            try
            {
                var tokenResponse = await _authApiClient.RefreshTokenAsync(currentAccessToken, currentRefreshToken);

                DateTime expiryTime = DateTime.UtcNow.AddHours(1);

                await SaveTokensAsync(tokenResponse.AccessToken, tokenResponse.RefreshToken, expiryTime);
            }
            catch (Exception ex)
            {
                await SignOutAsync();
                await InitializeAsync();
                //throw new Exception($"Помилка оновлення токену: {ex.Message}");
            }
        }

        private async Task<bool> IsAccessTokenExpired()
        {
            var expiryString = await SecureStorage.GetAsync("access_token_expires_at");
            if (string.IsNullOrEmpty(expiryString))
            {
                // Якщо значення не знайдено, вважаємо, що токен прострочено
                return true;
            }
            if (!long.TryParse(expiryString, out long expiryUnixTime))
            {
                // Якщо неможливо розпарсити значення, також вважаємо токен простроченим
                return true;
            }

            // Додатковий буфер у 10 секунд для уникнення проблем через затримки
            long currentUnixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return currentUnixTime >= (expiryUnixTime - 10);
        }

        public async Task<bool> IsUserSignedInAsync()
        {
            var accessToken = await SecureStorage.GetAsync("access_token");
            var refreshToken = await SecureStorage.GetAsync("refresh_token");
            var isExpired = await IsAccessTokenExpired();
            return !string.IsNullOrEmpty(accessToken) && !isExpired && !string.IsNullOrEmpty(refreshToken);
        }

        private async Task<bool> CheckIfUserIsSignedIn()
        {
            var accessToken = await SecureStorage.GetAsync("access_token");
            var refreshToken = await SecureStorage.GetAsync("refresh_token");
            return !string.IsNullOrEmpty(accessToken) && !await IsAccessTokenExpired();
        }


        public async Task SaveTokensAsync(string accessToken, string refreshToken, DateTimeOffset expiryTime)
        {
            await SecureStorage.SetAsync("access_token", accessToken);
            await SecureStorage.SetAsync("refresh_token", refreshToken);
            await SecureStorage.SetAsync("access_token_expires_at", expiryTime.ToUnixTimeSeconds().ToString());
        }
    }
}
