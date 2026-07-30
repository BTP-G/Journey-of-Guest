namespace JoG.Health {

    public interface IHealthChangeResolver {

        HealthChangeReport Resolve(Entity source, ref HealthChangeMessage message);
    }
}
