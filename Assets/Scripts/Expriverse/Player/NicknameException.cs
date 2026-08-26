using System;

namespace Expriverse.Player {

    public class NicknameException : Exception {
        public NicknameExceptionType ExceptionType { get; }

        public NicknameException(NicknameExceptionType exceptionType) : base(exceptionType.ToString()) {
            ExceptionType = exceptionType;
        }
    }
}
