using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Raccoons.Storage.Samples.MultilevelStorage
{
    public class StorablePositionAnimation : MonoBehaviour
    {
        private const string TARGET_POINT_KEY = "TargetPoint";
        private const string POSITION_KEY = "Position";

        [SerializeField]
        private float radius = 5;

        [SerializeField]
        private float speed = 1;

        private IStorage _objectStorage;
        private IStorage _positionStorage;
        private IStorage _targetPointStorage;

        private Vector3 _targetPoint;

        private void Awake()
        {
            PlayerPrefsStorage multilevel = CreateParentStorage();
            _objectStorage = new PlayerPrefsStorage(gameObject.name, multilevel);
            _positionStorage = new PlayerPrefsStorage(POSITION_KEY, _objectStorage);
            _targetPointStorage = new PlayerPrefsStorage(TARGET_POINT_KEY, _objectStorage);
        }

        private static PlayerPrefsStorage CreateParentStorage()
        {
            var raccoons = new PlayerPrefsStorage("Raccoons");
            var storage = new PlayerPrefsStorage("Storage", raccoons);
            var samples = new PlayerPrefsStorage("Samples", storage);
            var multilevel = new PlayerPrefsStorage("MultilevelStorage", samples);
            return multilevel;
        }

        private void Start()
        {
            _targetPoint = LoadVector3(_targetPointStorage);
            transform.position = LoadVector3(_positionStorage);
        }

        private void InitTargetPoint()
        {
            _targetPoint = Random.insideUnitSphere * radius;
            StoreVector3(_targetPointStorage, _targetPoint);
        }

        private void Update()
        {
            Vector3 direction = _targetPoint - transform.position;
            if (direction.sqrMagnitude < 0.1)
            {
                InitTargetPoint();
            }
            transform.Translate(direction.normalized * (speed * Time.deltaTime));
            StoreVector3(_positionStorage, transform.position);
        }

        public void StoreVector3(IStorage storage, Vector3 vector)
        {
            storage.SetFloat("X", vector.x);
            storage.SetFloat("Y", vector.y);
            storage.SetFloat("Z", vector.z);
        }

        public Vector3 LoadVector3(IStorage storage)
        {
            Vector3 vector = new Vector3();
            vector.x = storage.GetFloat("X");
            vector.y = storage.GetFloat("Y");
            vector.z = storage.GetFloat("Z");
            return vector;
        }
    }
}