using JoG.Character;

namespace JoG.Messages {

    public struct CharacterBodyChangedMessage {
        public CharacterBody previous;
        public CharacterBody next;
    }
}