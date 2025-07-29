using Raccoons.Storage.Instances;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Raccoons.Storage.Samples
{
    public class StorageTestPanel : MonoBehaviour
    {
        [SerializeField]
        private InputField keyInput;

        [SerializeField]
        private InputField valueInput;

        [SerializeField]
        private Button getButton;

        [SerializeField]
        private Button setButton;

        [SerializeField]
        private Button debugButton;

        [SerializeField]
        private BaseStorageAsset testAsset;

        [SerializeField]
        private Text status;

        [SerializeField]
        private Text title;

        [SerializeField]
        private Text path;

        private IStorage _storage;

        private void Start()
        {
            if (testAsset != null)
            {
                Init(testAsset);
            }
            getButton.onClick.AddListener(GetAction);
            setButton.onClick.AddListener(SetAction);
            debugButton.onClick.AddListener(DebugAction);
        }

        private void DebugAction()
        {
            if (_storage is BaseStorage baseStorage)
            {
                baseStorage.OpenStorageForDebug();
            }
        }

        public void Init(IStorage storage)
        {
            _storage = storage;
            status.text = "Ready";
            title.text = $"{storage.Key} ({storage.GetType().Name})";
            path.text = storage.Path;
        }

        private async void SetAction()
        {
            status.text = "Set action...";
            await _storage.SetStringAsync(keyInput.text, valueInput.text);
            status.text = "Success!";
        }

        private async void GetAction()
        {
            status.text = "Get action...";
            if (await _storage.ExistsAsync(keyInput.text))
            {
                valueInput.text = await _storage.GetStringAsync(keyInput.text);
                status.text = "Success!";
            }
            else
            {
                status.text = "No such key!";
            }
        }
    }
}
