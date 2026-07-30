using Xoderony.Logging;
using System.Collections.Generic;

namespace JoG {

    public interface IPlayerIdentity {
        string PlayerName { get; }
        ulong OwnerClientId { get; }
        bool IsOwner { get; }
    }

    public interface IPlayerRegistry {
        IPlayerIdentity LocalPlayer { get; }
        IReadOnlyCollection<IPlayerIdentity> Players { get; }
        int PlayerCount { get; }
        IPlayerIdentity this[ulong clientId] { get; }

        IPlayerIdentity GetPlayer(ulong clientId);
    }

    internal class PlayerRegistry : IPlayerRegistry {
        private readonly Dictionary<ulong, IPlayerIdentity> _clientIdToPlayer = new();
        public IPlayerIdentity LocalPlayer { get; private set; }

        public IReadOnlyCollection<IPlayerIdentity> Players => _clientIdToPlayer.Values;

        public int PlayerCount => _clientIdToPlayer.Count;

        public IPlayerIdentity this[ulong clientId] {
            get => _clientIdToPlayer[clientId];
        }

        public IPlayerIdentity GetPlayer(ulong clientId) {
            return _clientIdToPlayer[clientId];
        }

        public void Register(IPlayerIdentity player) {
            _clientIdToPlayer.Add(player.OwnerClientId, player);
            this.Log($"Register id: {player.OwnerClientId}, name: {player.PlayerName}");
            if (player.IsOwner) {
                LocalPlayer = player;
            }
        }

        public void Unregister(IPlayerIdentity player) {
            _clientIdToPlayer.Remove(player.OwnerClientId);
            this.Log($"Unregister id: {player.OwnerClientId}, name: {player.PlayerName}");
            if (player.IsOwner) {
                LocalPlayer = null;
            }
        }
    }
}
