using Raccoons.Identifiers.Guids;
using Raccoons.Scores;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Raccoons.Core.Samples.Scores.ScoreBank
{
    public class SimpleScoreSample : MonoBehaviour
    {
        private const float Amount = 1;

        [Header("Configuration")]
        [SerializeField] private GuidAsset key;
    
        [Header("UI")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private Button acquireButton;
        [SerializeField] private Button spendButton;
    
        private IScoreBank _scoreBank;

        [Inject]
        private void Construct(DiContainer container)
        {
            _scoreBank = container.ResolveId<IScoreBank>(key);
            SetupUI();
            UpdateDisplay();
        }

        private void SetupUI()
        {
            acquireButton.onClick.AddListener(OnAcquireClicked);
            spendButton.onClick.AddListener(OnSpendClicked);
        }

        private void UpdateDisplay()
        {
            scoreText.text = $"Score: {_scoreBank.GetScore()}";
        }

        private void OnAcquireClicked()
        {
            if (_scoreBank.CanAcquire(Amount))
            {
                _scoreBank.Acquire(Amount);
                UpdateDisplay();
            }
        }

        private void OnSpendClicked()
        {
            if (_scoreBank.CanSpend(Amount))
            {
                _scoreBank.Spend(Amount);
                UpdateDisplay();
            }
        }
    }
} 