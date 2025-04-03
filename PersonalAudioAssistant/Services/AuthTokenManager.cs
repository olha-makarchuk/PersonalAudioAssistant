using Azure.Core;
using MediatR;
using PersonalAudioAssistant.Application.PlatformFeatures.Commands.Auth;
using System;
using System.Threading.Tasks;

namespace PersonalAudioAssistant.Services
{
    public class AuthTokenManager
    {
        private readonly GoogleUserService _googleAuthService;
        private readonly IMediator _mediator;

        public event EventHandler<bool> UserSignInStatusChanged;

        private void OnUserSignInStatusChanged(bool isSignedIn)
        {
            UserSignInStatusChanged?.Invoke(this, isSignedIn);
        }

        public async Task<bool> IsSignedInAsync()
        {
            return await CheckIfUserIsSignedIn();
        }

        public AuthTokenManager(GoogleUserService googleAuthService, IMediator mediator)
        {
            _googleAuthService = googleAuthService ?? throw new ArgumentNullException(nameof(googleAuthService));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task InitializeAsync()
        {
            var refreshToken = await GetRefreshTokenAsync();
            if (await IsAccessTokenExpired() && !string.IsNullOrEmpty(refreshToken))
            {
                await RefreshTokenAsync();
            }
        }

        public async Task Sign_In_Up_AsyncGoogle()
        {
            var response = await _googleAuthService.SignInAsync();
            DateTime expiryTime = DateTime.UtcNow.AddHours(1);

            var command = new LoginWithGoogleCommand()
            {
                Email = response.Email,
                RefreshToken = response.RefreshToken
            };

            await SecureStorage.SetAsync("is_google", "true");

            await _mediator.Send(command);

            await SignIn(response, expiryTime);
        }

        public async Task SignInWithPasswordAsync(string email, string password)
        {
            var command = new LoginCommand
            {
                Email = email,
                Password = password
            };

            var tokens = await _mediator.Send(command);

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

            await SignIn(response, tokens.RefreshExpiresAt);
        }

        private async Task SignIn(TokenResponse response, DateTimeOffset expiryTime)
        {
            if (response.RefreshToken == null || response.AccessToken == null || response.Email == null)
            {
                throw new Exception("Недостатньо даних для збереження токенів.");
            }

            await SaveTokensAsync(response.AccessToken, response.RefreshToken, expiryTime);
            await SecureStorage.SetAsync("user_email", response.Email);
            OnUserSignInStatusChanged(true);
        }

        public async Task SignUpWithPasswordAsync(string email, string password)
        {
            var command = new RegistrationCommand()
            {
                Email = email,
                Password = password
            };

            var response = await _mediator.Send(command);

            TokenResponse tokenResponse = new()
            {
                AccessToken = response.AccessToken,
                RefreshToken = response.RefreshToken,
                Email = email
            };
            await SecureStorage.SetAsync("is_google", "false");

            await SignIn(tokenResponse, response.RefreshExpiresAt);
        }

        public async Task SignOutAsync()
        {
            SecureStorage.Remove("access_token");
            SecureStorage.Remove("refresh_token");
            SecureStorage.Remove("access_token_expires_at");
            SecureStorage.Remove("user_email");

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
                var refreshCommand = new RefreshTokenCommand()
                {
                    AccessToken = currentAccessToken,
                    RefreshToken = currentRefreshToken
                };

                var tokenResponse = await _mediator.Send(refreshCommand);
                DateTime expiryTime = DateTime.UtcNow.AddHours(1);

                await SaveTokensAsync(tokenResponse.AccessToken, tokenResponse.RefreshToken, expiryTime);
            }
            catch (Exception ex)
            {
                //await SignOutAsync();
                await InitializeAsync();
                throw new Exception($"Помилка оновлення токену: {ex.Message}");
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
