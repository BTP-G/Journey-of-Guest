namespace JoG.Chat {

    public interface IChatService {

        event ChatMessageHandler OnReceivedChatMessage;

        void SendMessage(string message, byte type);
    }
}
