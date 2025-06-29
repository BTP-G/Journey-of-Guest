using Cysharp.Threading.Tasks;
using GuestUnion;
using System;
using System.Linq;
using Unity.Netcode;
using Unity.Services.Multiplayer;
using Unity.Services.Qos;
using Unity.Services.Relay;
using UnityEngine;
using VContainer.Unity;

namespace JoG.Networking {

    public class SessionManager : IStartable, IDisposable, ISessionService {
        private ISession _session;
        private NetworkManager _networkManager;
        private IMultiplayerService _multiplayerService;
        private IRelayService _relayService;
        private IQosService _qosService;
        public string SessionName => _session?.Name;
        public string SessionCode => _session?.Code;

        public SessionManager(NetworkManager networkManager,
            IMultiplayerService multiplayerService,
            IRelayService relayService,
            IQosService qosService) {
            _networkManager = networkManager;
            _multiplayerService = multiplayerService;
            _relayService = relayService;
            _qosService = qosService;
        }

        void IStartable.Start() {
            _networkManager.OnConnectionEvent += OnConnectionEvent;
            _networkManager.OnClientStopped += OnLocalClientStopped;
            _networkManager.OnSessionOwnerPromoted += OnSessionOwnerPromoted;
        }

        public async UniTask<string> CreateSessionAsync(string sessionName, string password = null, byte maxPlayers = 4, bool isPrivate = false) {
            if (sessionName.IsNullOrWhiteSpace()) return "session name is empty!";
            if (maxPlayers < 1) return "max players must be greater than 0!";
            try {
                await LeaveSessionAsync();
                var region = await GetLowestLatencyRegionAsync();
                Debug.LogFormat("region: {0}", region);
                Debug.LogFormat("Creating session [name: {0}, maxPlayers: {1}, isPrivate: {2} ] ...", sessionName, maxPlayers, isPrivate);
                _session = await _multiplayerService.CreateSessionAsync(new SessionOptions() {
                    Name = sessionName,
                    MaxPlayers = maxPlayers,
                    Password = password.IsNullOrWhiteSpace() ? null : password,
                    IsPrivate = isPrivate,
                }.WithDistributedAuthorityNetwork(region));
                Debug.LogFormat("Session created [id: {0}, code:{1}, name: {2}]!", _session.Id, _session.Code, _session.Name);
                return "success";
            } catch (Exception e) {
                Debug.LogException(e);
                return e.Message;
            }
        }

        public async UniTask<string> JoinSessionAsync(string sessionCode, string password = null) {
            if (sessionCode.IsNullOrWhiteSpace()) return "session code is empty!";
            try {
                await LeaveSessionAsync();
                Debug.LogFormat("Joining session [code: {0}] ...", sessionCode);
                _session = await _multiplayerService.JoinSessionByCodeAsync(sessionCode, new JoinSessionOptions() {
                    Password = password.IsNullOrWhiteSpace() ? null : password,
                });
                Debug.LogFormat("Session joined [id: {0}, code:{1}, name: {2}]!", _session.Id, _session.Code, _session.Name);
                return "success";
            } catch (Exception e) {
                Debug.LogException(e);
                return e.Message;
            }
        }

        public async UniTask<string> JoinSessionByIdAsync(string sessionId, string password = null) {
            if (sessionId.IsNullOrWhiteSpace()) return "session id is empty!";
            try {
                await LeaveSessionAsync();
                Debug.LogFormat("Joining session [id: {0}] ...", sessionId);
                _session = await _multiplayerService.JoinSessionByIdAsync(sessionId, new JoinSessionOptions() {
                    Password = password.IsNullOrWhiteSpace() ? null : password,
                });
                Debug.LogFormat("Session joined [id: {0}, code:{1}, name: {2}]!", _session.Id, _session.Code, _session.Name);
                return "success";
            } catch (Exception e) {
                Debug.LogException(e);
                return e.Message;
            }
        }

        public async UniTask<QuerySessionsResults> QuerySessions() {
            var options = new QuerySessionsOptions();
            return await _multiplayerService.QuerySessionsAsync(options);
        }

        public async UniTask LeaveSessionAsync() {
            try {
                if (_session is null) return;
                var sessionToLeave = _session;
                _session = null;
                Debug.LogFormat("Leaving session [id: {0}, name: {1}] ...", sessionToLeave.Id, sessionToLeave.Name);
                await sessionToLeave.LeaveAsync();
                Debug.Log("Session left!");
            } catch (Exception e) {
                Debug.LogException(e);
            }
        }

        async void IDisposable.Dispose() {
            await LeaveSessionAsync();
            if (_networkManager) {
                _networkManager.OnConnectionEvent -= OnConnectionEvent;
                _networkManager.OnClientStopped -= OnLocalClientStopped;
                _networkManager.OnSessionOwnerPromoted -= OnSessionOwnerPromoted;
            }
        }

        private async UniTask<string> GetLowestLatencyRegionAsync() {
            var regions = await _relayService.ListRegionsAsync();
            var sortedRegions = await _qosService.GetSortedRelayQosResultsAsync(regions.Select(r => r.Id).ToList());
            return sortedRegions[0].Region;
        }

        private void OnSessionOwnerPromoted(ulong sessionOwnerPromoted) {
            Debug.LogFormat("[Client: {0}] is the session owner!", sessionOwnerPromoted);
        }

        private async void OnLocalClientStopped(bool isHost) {
            await LeaveSessionAsync();
        }

        private void OnConnectionEvent(NetworkManager networkManager, ConnectionEventData data) {
            if (data.EventType is ConnectionEvent.ClientConnected) {
                Debug.LogFormat("[Client: {0}] is connected.", data.ClientId);
            } else if (data.EventType is ConnectionEvent.ClientDisconnected) {
                Debug.LogFormat("[Client: {0}] is disconnected.", data.ClientId);
            }
        }
    }
}