namespace EditorAttributes {
    public interface IConditionalAttribute {
        string ConditionName { get; }
        int EnumValue { get; }
    }
}
