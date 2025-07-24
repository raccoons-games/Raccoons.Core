using UnityEngine;

namespace Raccoons.Storage.Files
{
    /// <summary>
    /// Class that contains configuration of the file-based storages
    /// </summary>
    [System.Serializable]
    public class FileStorageSettings
    {
        [Tooltip("Automatically loads and saves the data, no need to call load/save manually")]
        [SerializeField]
        private bool autoStore = true;

        [Tooltip("When there are async methods performing and you call sync method, this parameter forces them to finish with blocking thread. False value doesn't make it wait, but the result value can be changed after all the tasks performed")]
        [SerializeField]
        private bool asyncBlocksSync = true;

        [Tooltip("Creates *.tmp copy of the file before overwriting it")]
        [SerializeField]
        private bool safeSave = true;

        /// <summary>
        /// Automatically loads and saves the data, no need to call load/save manually
        /// </summary>
        public bool AutoStore { get => autoStore; set => autoStore = value; }

        /// <summary>
        /// When there are async methods performing and you call sync method, this parameter forces them to finish with blocking thread. 
        /// False value doesn't make it wait, but the result value can be changed after all the tasks performed
        /// </summary>
        public bool AsyncBlocksSync { get => asyncBlocksSync; set => asyncBlocksSync = value; }

        /// <summary>
        /// Creates *.tmp copy of the file before overwriting it
        /// </summary>
        public bool SafeSave { get => safeSave; set => safeSave = value; }
    }
}