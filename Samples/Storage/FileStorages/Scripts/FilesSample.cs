using Raccoons.Storage.Files;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Raccoons.Storage.Samples.FileStorages
{
    public class FilesSample : MonoBehaviour
    {
        [SerializeField]
        private StorageTestPanel persistentTest;
        [SerializeField]
        private StorageTestPanel jsonFileTest;
        [SerializeField]
        private StorageTestPanel folderTest;
        [SerializeField]
        private StorageTestPanel internalTest;
        private void Awake()
        {
            PersistentFolderStorage persistentFolderStorage = new PersistentFolderStorage();
            JsonFileStorage jsonStorage = new JsonFileStorage("TestJson.json", persistentFolderStorage);
            FolderStorage folderStorage = new FolderStorage("TestFolder", jsonStorage);
            JsonInternalStorage internalStorage = new JsonInternalStorage("TestInternalJson", jsonStorage);
            persistentTest.Init(persistentFolderStorage);
            jsonFileTest.Init(jsonStorage);
            folderTest.Init(folderStorage);
            internalTest.Init(internalStorage);
        }
    }
}