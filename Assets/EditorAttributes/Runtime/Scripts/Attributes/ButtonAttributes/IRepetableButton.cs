namespace EditorAttributes {
    public interface IRepetableButton {
        bool IsRepetable { get; }
        long PressDelay { get; }
        long RepetitionInterval { get; }
    }
}
