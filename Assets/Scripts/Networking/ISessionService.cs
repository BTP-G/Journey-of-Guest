using Cysharp.Threading.Tasks;
using Unity.Services.Multiplayer;

namespace JoG.Networking {
    public interface ISessionService {
        string SessionCode { get; }
        string SessionName { get; }

        UniTask<string> CreateSessionAsync(string sessionName, string password = null, byte maxPlayers = 4, bool isPrivate = false);
        UniTask<string> JoinSessionAsync(string sessionCode, string password = null);
        UniTask<string> JoinSessionByIdAsync(string sessionId, string password = null);
        UniTask LeaveSessionAsync();
        UniTask<QuerySessionsResults> QuerySessions();
    }
}