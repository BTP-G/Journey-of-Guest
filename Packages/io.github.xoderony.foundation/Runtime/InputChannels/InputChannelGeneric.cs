namespace Xoderony.InputChannels {

    public sealed class InputChannel<T> : InputChannel {
        public T value;

        public override void Reset() {
            value = default;
        }
    }
}
