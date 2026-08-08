namespace ANU.IngameDebug.Console {
    public interface IInjectDebugConsoleContext {
        IReadOnlyDebugConsoleProcessor Context { get; set; }
    }
}
