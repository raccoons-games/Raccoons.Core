using UnityEngine;

namespace Raccoons.Files.Instances
{
    [CreateAssetMenu(fileName = "PersistentPathFileAsset", menuName = "Raccoons/Files/PersistentPathFileAsset")]
    public class PersistentPathFileAsset : BaseFileAsset
    {
        [SerializeField]
        private string path;

        protected override IFileReader CreateReader()
        {
            return new PersistentPathFileReader(path);
        }

        protected override IFileWriter CreateWriter()
        {
            return new PersistentPathFileWriter(path);
        }
    }
}