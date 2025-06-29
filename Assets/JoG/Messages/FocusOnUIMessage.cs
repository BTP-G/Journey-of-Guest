namespace JoG.Messages {
    public readonly struct FocusOnUIMessage {
        public readonly bool isFocusingOnUI;

        public FocusOnUIMessage(bool isFocusingOnUI) {
            this.isFocusingOnUI = isFocusingOnUI;
        }
    }
}
