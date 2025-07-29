using System;
using UnityEngine;

namespace Raccoons.Files.Instances
{
    [CreateAssetMenu(fileName = "TextAssetFileAsset", menuName = "Raccoons/Files/TextAssetFileAsset")]
    public class TextAssetFileAsset : BaseFileAsset
    {
        [SerializeField]
        private TextAsset textAsset;

        public override IFileWriter Writer => throw new NotImplementedException("Writer is not supported for text assets file");
        
        protected override IFileReader CreateReader()
        {
            return new TextAssetFileReader(textAsset);
        }

        protected override IFileWriter CreateWriter()
        {
            return null;
        }
    }
}