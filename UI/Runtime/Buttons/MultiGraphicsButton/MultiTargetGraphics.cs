using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Raccoons.UI.Buttons
{
    public class MultiTargetGraphics : MonoBehaviour
    {
        [SerializeField]
        private List<Graphic> _targetGraphics;

        public List<Graphic> TargetGraphics { get => _targetGraphics; }
    }
}
