using System.Collections.Generic;
using System.Diagnostics;

namespace Xoderony.InputChannels {

    public class InputChannelHub {
        private readonly Dictionary<string, InputChannel> _keyToInputChannel = new();

        public InputChannel<T> GetInputChannel<T>(string key) {
            if (_keyToInputChannel.TryGetValue(key, out var inputChannel)) {
                Debug.Assert(
                    inputChannel is InputChannel<T>,
                    $"InputChannel key '{key}' 已注册为 {inputChannel.GetType()}，不能以 InputChannel<{typeof(T)}> 解析；同一 key 的读写方必须约定相同的泛型类型。");
                return (InputChannel<T>)inputChannel;
            }
            var newInputChannel = new InputChannel<T>();
            _keyToInputChannel[key] = newInputChannel;
            return newInputChannel;
        }

        public void ResetAll() {
            foreach (var inputChannel in _keyToInputChannel.Values) {
                inputChannel.Reset();
            }
        }
    }
}
