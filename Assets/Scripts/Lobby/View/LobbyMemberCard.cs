using EditorAttributes;
using JoG.UnityObjectExtensions;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SImage = Steamworks.Data.Image;

namespace JoG.Lobby.View {

    public class LobbyMemberCard : MonoBehaviour {
        private ulong currentMemberId;
        [field: SerializeField, Required] public RawImage MemberAvatarImage { get; private set; }
        [field: SerializeField, Required] public TMP_Text MemberNameText { get; private set; }
        [field: SerializeField, Required] public Button SteamStatsButton { get; private set; }

        public static Texture2D ImageToTexture2D(SImage image) {
            var avatar = new Texture2D((int)image.Width, (int)image.Height, TextureFormat.ARGB32, false) {
                filterMode = FilterMode.Trilinear
            };
            for (int x = 0; x < image.Width; ++x) {
                for (int y = 0; y < image.Height; ++y) {
                    var p = image.GetPixel(x, y);
                    avatar.SetPixel(x, y, new(p.r / 255.0f, p.g / 255.0f, p.b / 255.0f, p.a / 255.0f));
                }
            }
            avatar.Apply();
            return avatar;
        }

        public async void UpdateCard(Friend member) {
            if (currentMemberId == member.Id) return;
            var sImage = await member.GetMediumAvatarAsync();
            var avatar = default(Texture);
            if (sImage.HasValue) {
                avatar = ImageToTexture2D(sImage.Value);
            }
            SteamStatsButton.onClick.RemoveAllListeners();
            SteamStatsButton.onClick.AddListener(() => {
                SteamFriends.OpenUserOverlay(member.Id, "stats");
            });
            UpdateCard(avatar, member.Name);
        }

        public void UpdateCard(Texture avatar, string name) {
            if (MemberAvatarImage.texture) {
                MemberAvatarImage.texture.Destroy();
            }
            MemberAvatarImage.texture = avatar;
            MemberNameText.text = name;
        }
    }
}