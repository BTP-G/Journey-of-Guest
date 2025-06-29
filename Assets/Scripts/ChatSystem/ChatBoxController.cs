using EditorAttributes;
using GuestUnion;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace JoG.ChatSystem {

    public class ChatBoxController : MonoBehaviour {
        public const int messageCapacity = 20;
        public TMP_Text messageItemPrefab;
        public float nextDeactiveTime;
        private readonly Queue<TMP_Text> messageItems = new(messageCapacity);
        [field: SerializeField, Required] public Transform MessageContainer { get; private set; }

        [Button]
        public void AddMessage(string message) {
            if (message.IsNullOrEmpty()) return;
            TMP_Text messageItem;
            if (messageItems.Count < messageCapacity) {
                messageItem = Instantiate(messageItemPrefab, MessageContainer);
            } else {
                messageItem = messageItems.Dequeue();
                messageItem.transform.SetAsLastSibling();
            }
            messageItem.text = message;
            messageItems.Enqueue(messageItem);
            nextDeactiveTime = Time.time + 10f;
        }

        private void Update() {
            if (gameObject.activeSelf && Time.time > nextDeactiveTime) {
                gameObject.SetActive(false);
            }
        }
    }
}