namespace Raccoons.Factories
{
    public interface IDependenciesProvider
    {
        T Get<T>();
    }
}