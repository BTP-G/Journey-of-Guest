using System.Collections.Generic;

namespace JoG.Lobby.Controller {

    public interface ILobbyController {
        IReadOnlyList<LobbyMember> Mmembers { get; }
        string Id { get; }
        string Name { get; }

        bool TryCreateLobby(string lobbyName, int maxMembers = 4, ELobbyType lobbyType = ELobbyType.Private);

        void LeaveLobby();
    }
}