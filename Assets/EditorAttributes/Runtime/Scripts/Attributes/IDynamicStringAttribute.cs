namespace EditorAttributes {
    public enum StringInputMode {
        Constant,
        Dynamic
    }

    public interface IDynamicStringAttribute {
        StringInputMode StringInputMode { get; }
    }
}
