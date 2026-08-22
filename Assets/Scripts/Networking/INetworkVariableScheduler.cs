namespace JoG.Networking.P2P {
    /// <summary>JoG 网络对象的变量刷新调度服务。</summary>
    public interface INetworkVariableScheduler {
        void Schedule(JoGNetworkObject networkObject);
    }
}
