namespace EditorAttributes {
    public interface IMinMaxAxisValueAttribute {
        float MinValueX { get; }
        float MaxValueX { get; }

        float MinValueY { get; }
        float MaxValueY { get; }

        float MinValueZ { get; }
        float MaxValueZ { get; }

        float MinValueW { get; }
        float MaxValueW { get; }
    }
}
