using System.IO;
using UnityEngine;

namespace Raccoons.Files
{
    public class PersistentPathFileWriter : PathFileWriter
    {
        public PersistentPathFileWriter(string path) : base(path)
        {
        }

        public override string FilePath => Path.Combine(Application.persistentDataPath, base.FilePath);
    }
}