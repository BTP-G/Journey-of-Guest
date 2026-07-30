namespace JoG.Character {

    public delegate void CharacterSpawnHandler(CharacterEntity entity);

    public delegate void CharacterDespawnHandler(CharacterEntity entity);

    public delegate void CharacterLifeStartHandler(CharacterEntity entity);

    public delegate void CharacterLifeStopHandler(CharacterEntity entity);
}
