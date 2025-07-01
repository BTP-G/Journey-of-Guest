namespace JoG.Messages {
    public readonly struct CharacterInputLockMessage {
        public readonly bool isLocked;

        public CharacterInputLockMessage(bool isLocked) {
            this.isLocked = isLocked;
        }
    }
}
