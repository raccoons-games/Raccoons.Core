namespace Raccoons.Builds.Adapters
{
    public interface IBuildSettingsAdapter
    {
        void ApplySettings(AppConfiguration appConfiguration);
        bool i_IsAvailable();
        BaseBuildAdapterSettings CreateDefaultSettings();
    }
}