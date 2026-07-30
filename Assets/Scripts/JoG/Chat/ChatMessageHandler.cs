using System;

namespace JoG.Chat {

    public delegate void ChatMessageHandler(ulong clientId, byte type, ReadOnlySpan<char> message);
}
