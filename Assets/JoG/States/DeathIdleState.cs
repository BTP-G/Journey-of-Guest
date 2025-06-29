namespace JoG.States {

    public class DeathIdleState : State {
        private IHealth _health;

        protected override bool CheckTransitionIn() => !_health.IsAlive;

        protected override bool CheckTransitionOut() => _health.IsAlive;

        protected override void Awake() {
            base.Awake();
            _health = GetComponentInParent<IHealth>();
        }
    }
}