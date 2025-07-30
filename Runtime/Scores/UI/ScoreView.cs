using System;
using Raccoons.Identifiers.Guids;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Raccoons.Scores.UI
{
    public class ScoreView : MonoBehaviour
    {
        [SerializeField] private GuidAsset key;
        [SerializeField] private TextMeshProUGUI scoreText;
        
        [Header("Tooltip")] 
        [Tooltip("C# format string for displaying the score.\nExamples:\n  F0   → 123\n  N2   → 1,234.56\n  0.## → 123.45 or 123\n  C    → $123.00\n  P0   → 50%")]
        [SerializeField] private string formatScoreText;

        [Header("Optional")] 
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Image smallIconImage;
        [SerializeField] private Image bigIconImage;
        
        private IScoreBank _scoreBank;
        
        [Inject]
        private void Construct(DiContainer container)
        {
            _scoreBank = container.ResolveId<IScoreBank>(key);
            SetMetadata(_scoreBank.Metadata);
            SetScoreText(_scoreBank.GetScore());
        }

        private void Start()
        {
            _scoreBank.OnScoreChanged += SetScoreText;
        }

        private void OnDestroy()
        {
            _scoreBank.OnScoreChanged -= SetScoreText;
        }

        private void SetScoreText(object sender, ScoreChangeData data) => SetScoreText(data.NewScore);
        
        private void SetScoreText(float score)
        {
            scoreText.text = score.ToString(formatScoreText);
        }

        private void SetMetadata(ScoreMetadata metadata)
        {
            if (metadata == null)
                return;

            if (nameText != null)
            {
                nameText.text = metadata.Name; 
            } 
            
            if (descriptionText != null)
            {
                descriptionText.text = metadata.Description;
            }
            
            if (smallIconImage != null)
            {
                smallIconImage.sprite = metadata.SmallIcon;
            } 
            
            if (bigIconImage != null)
            {
                bigIconImage.sprite = metadata.BigIcon;
            }
        }
    }
}