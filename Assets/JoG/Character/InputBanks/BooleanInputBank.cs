namespace JoG.Character.InputBanks {

    public class BooleanInputBank : InputBank {
        private bool value;
        private bool started;
        private bool canceled;

        /// <summary>The action current value.</summary>
        public bool Value => value;

        /// <summary>
        /// Returns true if the value transitioned from false to true (e.g. a button press).
        /// </summary>
        public bool Started => started;

        /// <summary>
        /// Returns true if the value transitioned from true to false (e.g. a button release).
        /// </summary>
        public bool Canceled => canceled;

        public override void Reset() {
            value = false;
            started = false;
            canceled = false;
        }

        /// <summary>Updates the fields based on the current button state.</summary>
        public void UpdateState(bool newState) {
            var previousValue = value;
            value = newState;
            started = value && !previousValue;
            canceled = !value && previousValue;
        }
    }
}