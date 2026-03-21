namespace Raccoons.Builds.Adapters
{
    public interface IBuildSettingsAdapter
    {
        public void ApplySettings(AppConfiguration appConfiguration);
        public bool i_IsAvailable();
    }
}