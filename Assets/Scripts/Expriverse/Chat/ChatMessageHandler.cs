using System;

namespace Expriverse.Chat {

    public delegate void ChatMessageHandler(ulong clientId, byte type, ReadOnlySpan<char> message);
}
