using Cysharp.Threading.Tasks;
using Xoderony.Extensions;
using Xoderony.Logging;
using System;
using System.Linq;
using Unity.Services.Authentication;
using Unity.Services.Multiplayer;
using Unity.Services.Qos;
using Unity.Services.Relay;
using VContainer;
using VContainer.Unity;

namespace JoG.Networking {

    internal class SessionService : IDisposable, ISessionService {
        [Inject] internal IAuthenticationService _authenticationService;
        [Inject] internal IMultiplayerService _multiplayerService;
        [Inject] internal IRelayService _relayService;
        [Inject] internal IQosService _qosService;
        private ISession _session;
        public ISession Session => _session;

        public async UniTask CreateSessionAsync(string sessionName, string password = null, int maxPlayers = 4, bool isPrivate = false) {
            await LeaveSessionAsync();
            var region = await GetLowestLatencyRegionAsync();
            var playerNameProperty = new PlayerProperty(_authenticationService.PlayerName, VisibilityPropertyOptions.Public);
            var sessionOptions = new SessionOptions {
                Name = sessionName,
                MaxPlayers = maxPlayers,
                Password = password.IsNullOrWhiteSpace() ? null : password.Trim(),
                IsPrivate = isPrivate,
                PlayerProperties = { ["player_name"] = playerNameProperty },
            }.WithDistributedAuthorityNetwork(region);
            this.Log($"Creating session [region: {region}, name: {sessionName}, maxPlayers: {maxPlayers}, isPrivate: {isPrivate} ] ...");
            _session = await _multiplayerService.CreateSessionAsync(sessionOptions);
            this.Log($"Session created [id: {_session.Id}, code:{_session.Code}].");
        }

        public async UniTask JoinSessionByCodeAsync(string sessionCode, string password = null) {
            await LeaveSessionAsync();
            var playerNameProperty = new PlayerProperty(_authenticationService.PlayerName, VisibilityPropertyOptions.Public);
            var sessionOptions = new JoinSessionOptions {
                Password = password.IsNullOrWhiteSpace() ? null : password.Trim(),
                PlayerProperties = { ["player_name"] = playerNameProperty }
            };
            this.Log($"Joining session [code: {sessionCode}] ...");
            _session = await _multiplayerService.JoinSessionByCodeAsync(sessionCode, sessionOptions);
            this.Log($"Session joined [id: {_session.Id}, code:{_session.Code}, name: {_session.Name}]!");
        }

        public async UniTask JoinSessionByIdAsync(string sessionId, string password) {
            await LeaveSessionAsync();
            var playerNameProperty = new PlayerProperty(_authenticationService.PlayerName, VisibilityPropertyOptions.Public);
            var sessionOptions = new JoinSessionOptions {
                Password = password.IsNullOrWhiteSpace() ? null : password.Trim(),
                PlayerProperties = { ["player_name"] = playerNameProperty }
            };
            this.Log($"Joining session [id: {sessionId}] ...");
            _session = await _multiplayerService.JoinSessionByIdAsync(sessionId, sessionOptions);
            this.Log($"Session joined [id: {_session.Id}, code:{_session.Code}, name: {_session.Name}]!");
        }

        public async UniTask<QuerySessionsResults> QuerySessions() {
            var options = new QuerySessionsOptions() {
            };
            return await _multiplayerService.QuerySessionsAsync(options);
        }

        public async UniTask LeaveSessionAsync() {
            try {
                if (_session is null) return;
                if (_session.State == SessionState.Connected) {
                    this.Log($"Leaving session [id: {_session.Id}, name: {_session.Name}] ...");
                    await _session.LeaveAsync();
                }
            } catch (Exception e) {
                this.LogException(e);
            } finally {
                _session = null;
            }
        }

        void IDisposable.Dispose() {
            LeaveSessionAsync().Forget();
        }

        private async UniTask<string> GetLowestLatencyRegionAsync() {
            var regions = await _relayService.ListRegionsAsync();
            var sortedRegions = await _qosService.GetSortedRelayQosResultsAsync(regions.Select(r => r.Id).ToList());
            return sortedRegions[0].Region;
        }
    }
}
