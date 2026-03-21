namespace Raccoons.Builds.Adapters.SRDebuggerAdapter
{
    [System.Serializable]
    public abstract class BaseBuildAdapterSettings
    {
        public virtual string GetAdapterName()
        {
            var typeName = GetType().Name;
            return typeName.Replace("Settings", "").Replace("BuildAdapter", "");
        }

        public abstract void SetDefaultDevSettings();
        public abstract void SetDefaultProdSettings();
    }
}