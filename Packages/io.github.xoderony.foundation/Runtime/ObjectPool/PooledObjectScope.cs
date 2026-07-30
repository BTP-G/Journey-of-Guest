using System;

namespace Xoderony.ObjectPool {

    public struct PooledObjectScope<T> : IDisposable where T : class {

        private readonly IPool<T> _pool;

        private T _buffer;

        public PooledObjectScope(IPool<T> pool, T? buffer) {
            _pool = pool;
            _buffer = buffer;
        }

        public readonly T Buffer => _buffer;

        public void Dispose() {
            if (_buffer is null) {
                return;
            }
            _pool.Return(_buffer);
            _buffer = null;
        }

    }

}
