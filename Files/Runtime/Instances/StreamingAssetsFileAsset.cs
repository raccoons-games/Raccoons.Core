using System;
using UnityEngine;

namespace Raccoons.Files.Instances
{
    [CreateAssetMenu(fileName = "StreamingAssetsFileAsset", menuName = "Raccoons/Files/StreamingAssetsFileAsset")]
    public class StreamingAssetsFileAsset : BaseFileAsset
    {
        [SerializeField]
        private string streamingAssetsPath;

        public override IFileWriter Writer => throw new NotImplementedException("Writer is not supported for streaming assets file");

        protected override IFileReader CreateReader()
        {
            return new StreamingAssetsFileReader(streamingAssetsPath);
        }

        protected override IFileWriter CreateWriter()
        {
            return null;
        }
    }
}