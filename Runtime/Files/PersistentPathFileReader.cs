using System.IO;
using UnityEngine;

namespace Raccoons.Files
{
    public class PersistentPathFileReader : PathFileReader
    {
        public PersistentPathFileReader(string filePath) : base(filePath)
        {
        }

        public override string FilePath => Path.Combine(Application.persistentDataPath, base.FilePath);
    }
}