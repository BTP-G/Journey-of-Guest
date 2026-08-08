namespace Xoderony.ObjectPool {

    /// <summary>对象池租借契约。约定：调用方不得归还 null，归还后不得继续使用对象。</summary>
    public interface IPool<T> where T : class {

        T Rent();

        void Return(T value);
    }
}
