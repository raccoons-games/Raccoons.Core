using System;
using Raccoons.Factories.Pools;
using UnityEngine;
using UnityEngine.UI;

namespace Raccoons.UI.Animations
{
    public class CollectAnimationItem : PoolObject
    {
        [Header("Transform")]
        [SerializeField] private Transform itemTransform;
        
        [Header("All Optional")]
        [Header("Rendering")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Image itemImage;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Renderer meshRenderer;
        
        [Header("Particles")]
        [SerializeField] private new ParticleSystem particleSystem;
        
        [Header("Sprites")]
        [SerializeField] private Sprite defaultSprite;

        private void OnValidate()
        {
            itemTransform = GetComponent<Transform>();
        }

        public Transform GetTransform()
        {
            return itemTransform;
        }
        
        // Transform methods
        public void SetPosition(Vector3 position)
        {
            if (itemTransform != null)
                itemTransform.position = position;
        }
        
        public void SetLocalScale(Vector3 scale)
        {
            if (itemTransform != null)
                itemTransform.localScale = scale;
        }
        
        public void SetRotation(Quaternion rotation)
        {
            if (itemTransform != null)
                itemTransform.rotation = rotation;
        }
        
        // Sprite methods
        public void SetSprite(Sprite sprite)
        {
            if (itemImage != null)
                itemImage.sprite = sprite;
            
            if (spriteRenderer != null)
                spriteRenderer.sprite = sprite;
        }
        
        public void SetDefaultSprite()
        {
            if (defaultSprite != null)
                SetSprite(defaultSprite);
        }
        
        // Alpha/Transparency methods
        public void SetAlpha(float alpha)
        {
            if (itemImage != null)
            {
                Color color = itemImage.color;
                color.a = alpha;
                itemImage.color = color;
            }
            
            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = alpha;
                spriteRenderer.color = color;
            }
            
            if (canvasGroup != null)
            {
                canvasGroup.alpha = alpha;
            }
            
            if (meshRenderer != null)
            {
                Color color = meshRenderer.material.color;
                color.a = alpha;
                meshRenderer.material.color = color;
            }
        }
        
        // Color methods
        public void SetColor(Color color)
        {
            if (itemImage != null)
                itemImage.color = color;
            
            if (spriteRenderer != null)
                spriteRenderer.color = color;
            
            if (meshRenderer != null)
                meshRenderer.material.color = color;
        }
        
        // Particle methods
        public void PlayParticle()
        {
            if (particleSystem != null)
                particleSystem.Play();
        }
        
        public void StopParticle()
        {
            if (particleSystem != null)
                particleSystem.Stop();
        }
        
        public void SetParticleColor(Color color)
        {
            if (particleSystem != null)
            {
                var main = particleSystem.main;
                main.startColor = color;
            }
        }
        
        // Visibility methods
        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }
        
        public void SetVisible(bool visible)
        {
            if (itemImage != null)
                itemImage.enabled = visible;
            
            if (spriteRenderer != null)
                spriteRenderer.enabled = visible;
            
            if (meshRenderer != null)
                meshRenderer.enabled = visible;
        }
        
        // Utility methods
        
        public void ResetToDefaults()
        {
            SetDefaultSprite();
            SetAlpha(1f);
            SetColor(Color.white);
            SetLocalScale(Vector3.one);
            SetRotation(Quaternion.identity);
            StopParticle();
        }
    }
} 