namespace Raccoons.Builds.Adapters
{
    public abstract class BaseBuildSettingsAdapter: IBuildSettingsAdapter
    {
        private static BaseBuildSettingsAdapter _instance;
        public BaseBuildSettingsAdapter Instance => _instance;

        public BaseBuildSettingsAdapter()
        {
            _instance = this;
        }
        public abstract void ApplySettings(AppConfiguration appConfiguration);
        public abstract bool i_IsAvailable();

        public static bool IsAvailable() => _instance.i_IsAvailable();
    }
}