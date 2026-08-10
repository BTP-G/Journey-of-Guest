using System.Collections.Generic;
using UnityEngine;
using Xoderony.InputChannels;

namespace JoG.Character {

    public interface ICharacterInputDriver {

        void Bind(CharacterEntity body);

        void Unbind();
    }

    internal sealed class CharacterInputBinding {

        private readonly CharacterSpawner _spawner;

        private readonly List<ICharacterInputDriver> _drivers = new();

        private CharacterEntity _body;

        private bool _isEnabled;

        public CharacterInputBinding(CharacterSpawner spawner) {
            _spawner = spawner;
            RefreshDrivers();
        }

        public void SetBody(CharacterEntity body) {
            if (ReferenceEquals(_body, body)) {
                return;
            }

            UnbindDrivers();
            _body?.InputChannelHub.ResetAll();
            _body = body;
            RefreshDrivers();
        }

        public void SetEnabled(bool enabled) {
            if (_isEnabled == enabled) {
                return;
            }

            if (enabled) {
                foreach (var driver in _drivers) {
                    driver.Bind(_body);
                }
            } else {
                UnbindDrivers();
                _body?.InputChannelHub.ResetAll();
            }
            _isEnabled = enabled;
        }

        private void RefreshDrivers() {
            _drivers.Clear();
            AddDrivers(_spawner.GetComponents<MonoBehaviour>());
            if (_drivers.Count == 0 && _body != null) {
                AddDrivers(_body.GetComponentsInChildren<MonoBehaviour>(true));
            }
            UnbindDrivers();
        }

        private void AddDrivers(MonoBehaviour[] behaviours) {
            foreach (var behaviour in behaviours) {
                if (behaviour is ICharacterInputDriver driver) {
                    _drivers.Add(driver);
                }
            }
        }

        private void UnbindDrivers() {
            foreach (var driver in _drivers) {
                driver.Unbind();
            }
            _isEnabled = false;
        }
    }
}
