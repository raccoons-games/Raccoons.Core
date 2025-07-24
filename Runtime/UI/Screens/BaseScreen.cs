using Raccoons.UI.Guids;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Raccoons.UI.Screens
{
    public class BaseScreen : MonoBehaviour, IScreen
    {
        [SerializeField]
        private string _key;
        [SerializeField]
        private UIGuidAsset _guid;

        private TaskCompletionSource<bool> _taskCompletionSource;

        public event EventHandler<IScreen> OnOpened;
        public event EventHandler<IScreen> OnClosed;

        public string Key { get => _key; }
        public IUIGuid Guid { get => _guid; }
        protected TaskCompletionSource<bool> TaskCompletionSource { get => _taskCompletionSource; set => _taskCompletionSource = value; }

        public virtual async Task Open(CancellationToken cancellationToken = default)
        {
            InitTaskCompletionSource();
            gameObject.SetActive(true);
            SetResultTaskCompletionSource(true);
            await _taskCompletionSource.Task;
        }

        public virtual async Task Close(CancellationToken cancellationToken = default)
        {
            InitTaskCompletionSource();
            gameObject.SetActive(false);
            SetResultTaskCompletionSource(true);
            await _taskCompletionSource.Task;
        }

        protected virtual void InitTaskCompletionSource()
        {
            if (_taskCompletionSource != null && _taskCompletionSource.Task.IsCompleted == false)
            {
                _taskCompletionSource.SetResult(false);
            }
            _taskCompletionSource = new TaskCompletionSource<bool>();
        }
        protected virtual void SetResultTaskCompletionSource(bool value)
        {
            _taskCompletionSource?.SetResult(value);
        }

        public virtual bool IsOpen()
        {
            return gameObject.activeSelf == true; ;
        }
    }
}
