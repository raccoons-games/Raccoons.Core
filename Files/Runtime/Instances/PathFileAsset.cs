using UnityEngine;

namespace Raccoons.Files.Instances
{
    [CreateAssetMenu(fileName = "PathFileAsset", menuName = "Raccoons/Files/PathFileAsset")]
    public class PathFileAsset : BaseFileAsset
    {
        [SerializeField]
        private string path;

        protected override IFileReader CreateReader()
        {
            return new PathFileReader(path);
        }

        protected override IFileWriter CreateWriter()
        {
            return new PathFileWriter(path);
        }
    }
}