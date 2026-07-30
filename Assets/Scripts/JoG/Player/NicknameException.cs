using System;

namespace JoG.Player {

    public class NicknameException : Exception {
        public NicknameExceptionType ExceptionType { get; }

        public NicknameException(NicknameExceptionType exceptionType) : base(exceptionType.ToString()) {
            ExceptionType = exceptionType;
        }
    }
}
