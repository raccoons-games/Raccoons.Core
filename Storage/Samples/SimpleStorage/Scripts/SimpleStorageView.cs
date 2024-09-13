using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Raccoons.Storage.Samples.SimpleStorage
{
    public class SimpleStorageView : MonoBehaviour
    {
        private const string FLOAT_KEY = "FloatValue";
        private const string STRING_KEY = "StringKey";
        [SerializeField]
        private string storageKey;

        [SerializeField]
        private InputField floatValueInput;
        [SerializeField]
        private InputField stringValueInput;

        private IStorage _storage;

        private void Awake()
        {
            _storage = new PlayerPrefsStorage(storageKey);
            floatValueInput.onValueChanged.AddListener(ChangeFloat);
            stringValueInput.onValueChanged.AddListener(ChangeString);
        }

        private void ChangeString(string arg0)
        {
            _storage.SetString(STRING_KEY, arg0);
        }

        private void ChangeFloat(string arg0)
        {
            _storage.SetFloat(FLOAT_KEY, float.Parse(arg0));
        }

        private void Start()
        {
            floatValueInput.SetTextWithoutNotify(_storage.GetFloat(FLOAT_KEY).ToString());
            stringValueInput.SetTextWithoutNotify(_storage.GetString(STRING_KEY));
        }
    }
}