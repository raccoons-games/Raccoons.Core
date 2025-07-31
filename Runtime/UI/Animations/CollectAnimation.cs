using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Raccoons.UI.Animations.CollectAnimationSettings;

namespace Raccoons.UI.Animations
{
    public class CollectAnimation
    {
        public List<CollectAnimationItem> Items { get; private set; } = new List<CollectAnimationItem>();
        public UniTaskCompletionSource<bool> CompletionSource { get; private set; }
        public CollectAnimationSettingsAsset Settings { get; private set; }
        public Vector3 StartPosition { get; private set; }
        public Vector3 EndPosition { get; private set; }
        public int TotalItems { get; private set; }
        public int CompletedItems { get; private set; }
        
        public bool IsPlaying { get; private set; }
        public UniTask<bool> AnimationTask => CompletionSource?.Task ?? UniTask.FromResult(false);
        
        public void Initialize(int count, Vector3 start, Vector3 end, CollectAnimationSettingsAsset settings)
        {
            StartPosition = start;
            EndPosition = end;
            Settings = settings;
            TotalItems = count;
            CompletedItems = 0;
            IsPlaying = true;
            CompletionSource = new UniTaskCompletionSource<bool>();
        }
        
        public void AddItem(CollectAnimationItem item)
        {
            Items.Add(item);
        }
        
        public void ItemCompleted()
        {
            CompletedItems++;
            
            if (CompletedItems >= TotalItems)
            {
                IsPlaying = false;
                CompletionSource?.TrySetResult(true);
            }
        }
        
        public void Stop()
        {
            IsPlaying = false;
            CompletionSource?.TrySetResult(false);
        }
        
        public void Clear()
        {
            Items.Clear();
        }
    }
} 