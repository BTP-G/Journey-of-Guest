namespace JoG.Character.InputBanks {

    public class TriggerInputBank : InputBank {
        private bool triggered;

        public bool Triggered => triggered;

        public void UpdateState(bool newState) {
            triggered = newState;
        }

        public override void Reset() {
            triggered = false;
        }
    }
}