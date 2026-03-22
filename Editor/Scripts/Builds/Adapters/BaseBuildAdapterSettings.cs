using System;

namespace Raccoons.Builds.Adapters
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class AdapterDisplayNameAttribute : Attribute
    {
        public string Name { get; }
        public AdapterDisplayNameAttribute(string name) => Name = name;
    }

    [System.Serializable]
    public abstract class BaseBuildAdapterSettings
    {
        public virtual string GetAdapterName()
        {
            var attrs = GetType().GetCustomAttributes(typeof(AdapterDisplayNameAttribute), false);
            if (attrs.Length > 0)
                return ((AdapterDisplayNameAttribute)attrs[0]).Name;

            var typeName = GetType().Name;
            typeName = typeName.Replace("Settings", "").Replace("BuildAdapter", "");
            return System.Text.RegularExpressions.Regex.Replace(typeName, "(\\B[A-Z])", " $1").Trim();
        }

        public abstract void SetDefaultDevSettings();
        public abstract void SetDefaultProdSettings();
    }
}