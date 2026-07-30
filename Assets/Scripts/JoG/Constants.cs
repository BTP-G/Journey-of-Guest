namespace JoG {

    public static class Constants {

        public static class Stats {
            public const string Regen = nameof(Regen);
            public const string MaxHealth = nameof(MaxHealth);
            public const string MaxMoveSpeed = nameof(MaxMoveSpeed);
            public const string MoveAcceleration = nameof(MoveAcceleration);
            public const string AttackPower = nameof(AttackPower);
            public const string Defense = nameof(Defense);
            public const string TreatmentAmplification = nameof(TreatmentAmplification);
        }

        public static class Tags {
            public const string Character = nameof(Character);
            public const string Player = nameof(Player);
            public const string Enemy = nameof(Enemy);
        }

        public static class Factions {
            public const int Neutral = 0;
            public const int Player = 1;
            public const int Enemy = 2;
        }

        public static class Layers {
            public const string Character = nameof(Character);
        }

        public static class Camera {
            public const string MainCamera = nameof(MainCamera);
            public const string UICamera = nameof(UICamera);
        }

        public static class InputActionMap {
            public const string Gameplay = nameof(Gameplay);

            public const string Overlay = nameof(Overlay);

            public const string Menu = nameof(Menu);
        }

        public static class InputAction {
            public const string Move = nameof(Move);
            public const string Look = nameof(Look);
            public const string Sprint = nameof(Sprint);
            public const string Crouch = nameof(Crouch);
            public const string Jump = nameof(Jump);
            public const string PrimaryAction = nameof(PrimaryAction);
            public const string SecondaryAction = nameof(SecondaryAction);
            public const string Skill = nameof(Skill);
            public const string Equipment = nameof(Equipment);
            public const string Interact = nameof(Interact);
            public const string Scroll = nameof(Scroll);
            public const string Reload = nameof(Reload);
            public const string Drop = nameof(Drop);
            public const string Number = nameof(Number);

            public const string Chat = nameof(Chat);
            public const string Inventory = nameof(Inventory);

            public const string IngameMenu = nameof(IngameMenu);
            public const string Lobby = nameof(Lobby);
        }
    }
}
