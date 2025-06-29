namespace JoG {

    public static class DamgeFlag {
        public static readonly ulong physic = 1;
        public static readonly ulong magic = 1 << 1;
        public static readonly ulong fire = 1 << 2;
        public static readonly ulong sharp = 1 << 3;

        public static bool HasFlag(in DamageMessage message, in ulong flag) => (message.flags & flag) is not 0;
    }
}