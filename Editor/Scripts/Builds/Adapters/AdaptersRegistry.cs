#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Raccoons.Builds.Adapters
{
    public static class AdapterRegistry
    {
        private static List<IBuildSettingsAdapter> _adapters;
        
        [InitializeOnLoadMethod]
        public static IReadOnlyList<IBuildSettingsAdapter> GetAllAdapters()
        {
            if (_adapters == null)
            {
                DiscoverAdapters();
            }
            return _adapters;
        }
        
        private static void DiscoverAdapters()
        {
            _adapters = new List<IBuildSettingsAdapter>();
            
            var adapterType = typeof(IBuildSettingsAdapter);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => adapterType.IsAssignableFrom(type) 
                               && !type.IsInterface 
                               && !type.IsAbstract);
            
            foreach (var type in types)
            {
                try
                {
                    var instance = (IBuildSettingsAdapter)Activator.CreateInstance(type);
                    _adapters.Add(instance);
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogWarning($"Failed to create adapter {type.Name}: {e.Message}");
                }
            }
        }
        
        public static IEnumerable<IBuildSettingsAdapter> GetActiveAdapters()
        {
            return GetAllAdapters().Where(a => a.i_IsAvailable());
        }
    }
}
#endif