namespace JoG.Player {

    /// <summary>玩家名称验证失败的具体原因类型</summary>
    public enum NicknameExceptionType {

        /// <summary>名称为空或仅包含空白字符</summary>
        EmptyOrWhitespace,

        /// <summary>名称长度小于最小允许值（如 2）</summary>
        TooShort,

        /// <summary>名称长度超过最大允许值（如 16）</summary>
        TooLong,

        /// <summary> 包含非法字符（如 @#$%<> 等） </summary>
        InvalidCharacters,

        /// <summary>不包含任何有效文字（如全为数字或下划线）</summary>
        NoValidLetters,

        /// <summary>其他未分类错误</summary>
        Unknown
    }
}
