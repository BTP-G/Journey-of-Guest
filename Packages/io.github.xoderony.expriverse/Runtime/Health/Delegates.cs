namespace Expriverse.Health {

    public delegate void OutgoingDamageMessageModifier(ref HealthChangeMessage message, in Entity victim);

    public delegate void OutgoingDamageReportHandler(in HealthChangeReport report);

    public delegate void IncomingDamageMessageModifier(ref HealthChangeMessage message, in Entity attacker);

    public delegate void IncomingDamageReportHandler(in HealthChangeReport report);

    public delegate void OutgoingHealMessageModifier(ref HealthChangeMessage message, Entity target);

    public delegate void OutgoingHealReportHandler(in HealthChangeReport report);

    public delegate void IncomingHealMessageModifier(ref HealthChangeMessage message, Entity healer);

    public delegate void IncomingHealReportHandler(in HealthChangeReport report);

    public delegate void OutgoingHitMessageHandler(in HitMessage message, Entity target);

    public delegate void IncomingHitMessageHandler(in HitMessage message, Entity source);

    public delegate void HealthChangedHandler(int prev, int next);
}
