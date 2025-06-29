namespace JoG {

    public interface IInfomationProvider {
        string Name => GetString("name");
        string Description => GetString("description");

        string GetString(string token);
    }
}