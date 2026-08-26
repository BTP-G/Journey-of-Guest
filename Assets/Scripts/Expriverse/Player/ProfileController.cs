using Expriverse.Localization;
using Expriverse.UI.Popup;
using TMPro;
using UnityEngine;
using VContainer;
using Xoderony.Localization;

namespace Expriverse.Player {

    public class ProfileController : MonoBehaviour {
        [Inject] internal IProfileService _profileService;
        [Inject] internal ToastPopupController _toastPopupController;
        [SerializeField] private TMP_InputField _inputField;

        private void Awake() {
            _inputField.onEndEdit.AddListener(SetNickname);
            _inputField.SetTextWithoutNotify(_profileService.Nickname);
        }

        private async void SetNickname(string nickname) {
            try {
                if (nickname == _profileService.Nickname) {
                    return;
                }

                await _profileService.SetNicknameAsync(nickname);
            } catch (NicknameException e) {
                var error = e.Message;
                switch (e.ExceptionType) {
                    case NicknameExceptionType.EmptyOrWhitespace:
                        error = Localizer.GetString(L10nKeys.Profile.Name.Error.EmptyOrWhitespace);
                        break;

                    case NicknameExceptionType.TooShort:
                        error = Localizer.GetString(L10nKeys.Profile.Name.Error.TooShort);
                        break;

                    case NicknameExceptionType.TooLong:
                        error = Localizer.GetString(L10nKeys.Profile.Name.Error.TooLong);
                        break;

                    case NicknameExceptionType.InvalidCharacters:
                        error = Localizer.GetString(L10nKeys.Profile.Name.Error.InvalidCharacters);
                        break;

                    case NicknameExceptionType.NoValidLetters:
                        error = Localizer.GetString(L10nKeys.Profile.Name.Error.NoValidLetters);
                        break;
                }
                _toastPopupController.Show(error, MessageLevel.Error, ToastPosition.TopRight, 5);
            }
            _inputField.SetTextWithoutNotify(_profileService.Nickname);
        }
    }
}
