using Cysharp.Threading.Tasks;

namespace JoG.Player {

    public interface IProfileService {
        string Profile { get; set; }
        string Nickname { get; set; }

        UniTask<string> GetNicknameAsync();

        UniTask SetNicknameAsync(string nickname);
    }
}
