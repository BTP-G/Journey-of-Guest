using Cysharp.Text;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using EditorAttributes;
using MessagePipe;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using Xoderony.Extensions;

namespace JoG.Chat {

    public class ChatBoxController : MonoBehaviour {
        [Required] public TMP_Text messageItemTemplate;
        [Required] public CanvasGroup canvasGroup;
        [Required] public TMP_InputField inputField;
        [Min(1)] public int messageCapacity = 100;

        [Inject, Key(Constants.InputAction.Chat)]
        internal InputAction toggleInput;

        [Inject] internal IPublisher<UIStateChangedMessage> publisher;
        [Inject] internal IChatService chatService;
        [Inject] internal IPlayerRegistry playerRegistry;
        private readonly Queue<TMP_Text> _messageItems = new();
        private TweenerCore<float, float, FloatOptions> _fadeTween;

        public void Enqueue(in Utf16ValueStringBuilder stringBuilder) {
            TMP_Text messageItem;
            if (_messageItems.Count < messageCapacity) {
                messageItem = Instantiate(messageItemTemplate, messageItemTemplate.transform.parent);
                messageItem.gameObject.SetActive(true);
            } else {
                messageItem = _messageItems.Dequeue();
                messageItem.transform.SetAsLastSibling();
            }
            messageItem.SetText(stringBuilder);
            _messageItems.Enqueue(messageItem);
            if (enabled) {
                return;
            }

            _fadeTween.Restart();
        }

        public void Clear() {
            foreach (var item in _messageItems) {
                Destroy(item.gameObject);
            }
            _messageItems.Clear();
        }

        protected void Awake() {
            inputField.onSubmit.AddListener(OnSubmit);
            toggleInput.performed += OnToggle;
            chatService.OnReceivedChatMessage += OnReceivedChatMessage;
            _fadeTween = canvasGroup.DOFade(0, 1)
                .From(1)
                .SetEase(Ease.InOutQuad)
                .SetDelay(10f)
                .SetAutoKill(false);
        }

        protected void OnEnable() {
            _fadeTween.Restart();
            _fadeTween.Pause();
            inputField.ActivateInputField();
            publisher.Publish(new("ChatBox", UILayer.Overlay, true));
        }

        protected void OnDisable() {
            _fadeTween.Play();
            publisher.Publish(new("ChatBox", UILayer.Overlay, false));
        }

        protected void OnDestroy() {
            toggleInput.performed -= OnToggle;
            chatService.OnReceivedChatMessage -= OnReceivedChatMessage;
            _fadeTween.Kill();
        }

        protected void Reset() {
            canvasGroup = GetComponentInChildren<CanvasGroup>();
            inputField = GetComponentInChildren<TMP_InputField>(true);
        }

        private void EnqueuePlayerMessage(ReadOnlySpan<char> message, IPlayerIdentity player) {
            using var sb = ZString.CreateStringBuilder(true);
            var color = player.IsOwner ? "#00FFFF" : "#00FF00";
            sb.Append("<color=");
            sb.Append(color);
            sb.Append('>');
            sb.Append(player.PlayerName);
            sb.Append("</color>");
            sb.Append(": ");
            var prefixLength = sb.Length;
            sb.Append(message);
            sb.Replace('<', '《', prefixLength, message.Length);
            sb.Replace('>', '》', prefixLength, message.Length);
            Enqueue(sb);
        }

        private void EnqueueSystemMessage(ReadOnlySpan<char> message) {
            using var sb = ZString.CreateStringBuilder(true);
            sb.Append("<color=red><size=16>[SYSTEM]: ");
            sb.Append(message);
            sb.Append("</size></color>");
            Enqueue(sb);
        }

        private void OnReceivedChatMessage(ulong clientId, byte type, ReadOnlySpan<char> message) {
            switch (type) {
                case ChatMessageTypes.System:
                    EnqueueSystemMessage(message);
                    break;

                case ChatMessageTypes.Player:
                    var sender = playerRegistry.GetPlayer(clientId);
                    EnqueuePlayerMessage(message, sender);
                    break;
            }
        }

        private void OnSubmit(string text) {
            if (text.IsNullOrWhiteSpace()) {
                return;
            }

            EnqueuePlayerMessage(text, playerRegistry.LocalPlayer);
            chatService.SendMessage(text, ChatMessageTypes.Player);
            inputField.SetTextWithoutNotify(string.Empty);
        }

        private void OnToggle(InputAction.CallbackContext _) {
            enabled = !enabled;
        }
    }
}
