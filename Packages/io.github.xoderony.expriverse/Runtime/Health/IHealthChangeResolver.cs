namespace Expriverse.Health {

    public interface IHealthChangeResolver {

        HealthChangeReport Resolve(Entity source, ref HealthChangeMessage message);
    }
}
