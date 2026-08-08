namespace EditorAttributes {
    public interface IColorAttribute {
        float R { get; }
        float G { get; }
        float B { get; }
        bool UseRGB { get; }
        string HexColor { get; }

        GUIColor Color { get; }
    }
}
