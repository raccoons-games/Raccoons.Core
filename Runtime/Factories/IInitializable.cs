namespace Raccoons.Factories
{
    public interface IInitializable
    {
        void Initialize(IDependenciesProvider dependenciesProvider);
    }
}