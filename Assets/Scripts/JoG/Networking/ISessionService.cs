using Cysharp.Threading.Tasks;
using Unity.Services.Multiplayer;

namespace JoG.Networking {

    public interface ISessionService {
        ISession Session { get; }

        UniTask CreateSessionAsync(string sessionName, string password = null, int maxPlayers = 4, bool isPrivate = false);

        UniTask JoinSessionByCodeAsync(string sessionCode, string password = null);

        UniTask JoinSessionByIdAsync(string sessionId, string password = null);

        UniTask LeaveSessionAsync();

        UniTask<QuerySessionsResults> QuerySessions();
    }
}
