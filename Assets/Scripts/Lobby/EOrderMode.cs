using System;

namespace JoG.Lobby {

    [Serializable]
    public enum EOrderMode : byte {
        None = 0,
        ByName,
        ByPlayerCount,
        ByDifficulty,
        ByMode,
        ByPing,
    }
}