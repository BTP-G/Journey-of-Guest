namespace ANU.IngameDebug.Console {
    public interface IConsoleInput {
        bool GetOpen();
        bool GetControl();
        bool GetDot();
        bool GetUp();
        bool GetDown();
        bool GetTab();
        bool GetEscape();
    }
}
