namespace Raccoons.Builds.Adapters
{
    public abstract class BaseBuildSettingsAdapter : IBuildSettingsAdapter
    {
        public abstract void ApplySettings(AppConfiguration appConfiguration);
        public abstract bool i_IsAvailable();
        public abstract BaseBuildAdapterSettings CreateDefaultSettings();
    }
}