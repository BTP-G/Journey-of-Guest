namespace Xoderony.ObjectPool {

    public interface IPool<T> where T : class {

        T Rent();

        void Return(T value);
    }
}