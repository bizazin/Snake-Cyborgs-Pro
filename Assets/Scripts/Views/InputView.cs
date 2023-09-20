using Core.Abstracts;
using UnityEngine;
using UnityEngine.UI;

namespace Views
{
    public class InputView : View
    {
        [SerializeField] private Button _leftButton;
        [SerializeField] private Button _rightButton;

        public Button LeftButton => _leftButton;
        public Button RightButton => _rightButton;
    }
}