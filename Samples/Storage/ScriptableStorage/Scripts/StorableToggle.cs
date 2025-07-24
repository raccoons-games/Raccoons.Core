using Raccoons.Storage.Instances;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Raccoons.Storage.Samples.ScriptableStorage
{
    public class StorableToggle : MonoBehaviour
    {
        private const string TOGGLE_KEY = "Toggle";

        [SerializeField]
        private Toggle toggle;

        [SerializeField]
        private BaseStorageAsset storageAsset;

        private void Awake()
        {
            toggle.onValueChanged.AddListener(ToggleChange);    
        }

        private void Start()
        {
            toggle.SetIsOnWithoutNotify(storageAsset.GetInt(TOGGLE_KEY) == 1);    
        }

        private void ToggleChange(bool arg0)
        {
            storageAsset.SetInt(TOGGLE_KEY, arg0 ? 1 : 0);
        }
    }
}