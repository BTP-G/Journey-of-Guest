using Cysharp.Threading.Tasks;
using System.Text.RegularExpressions;
using Unity.Services.Authentication;
using VContainer;
using Xoderony.Extensions;

namespace JoG.Player {

    internal class UnityProfileService : IProfileService {
        public const int MinNameLength = 3;
        public const int MaxNameLength = 16;
        [Inject] internal IAuthenticationService _authenticationService;

        public string Profile {
            get => _authenticationService.Profile;
            set => _authenticationService.SwitchProfile(value);
        }

        public string Nickname {
            get => _authenticationService.PlayerName[..^5];
            set => SetNicknameAsync(value).Forget();
        }

        public async UniTask<string> GetNicknameAsync() {
            var nickname = await _authenticationService.GetPlayerNameAsync();
            return nickname[..^5];
        }

        public async UniTask SetNicknameAsync(string nickname) {
            CheckNameValid(nickname);
            await _authenticationService.UpdatePlayerNameAsync(nickname);
        }

        private void CheckNameValid(string name) {
            if (name.IsNullOrWhiteSpace()) {
                throw new NicknameException(NicknameExceptionType.EmptyOrWhitespace);
            }
            if (name.Length < MinNameLength) {
                throw new NicknameException(NicknameExceptionType.TooShort);
            }
            if (name.Length > MaxNameLength) {
                throw new NicknameException(NicknameExceptionType.TooLong);
            }
            if (!Regex.IsMatch(name, @"^[\w\u4e00-\u9fa5\s]+$")) {
                throw new NicknameException(NicknameExceptionType.InvalidCharacters);
            }
            if (!Regex.IsMatch(name, @"[\p{L}\u4e00-\u9fa5]")) {
                throw new NicknameException(NicknameExceptionType.NoValidLetters);
            }
        }
    }
}
