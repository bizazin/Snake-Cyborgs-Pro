using Core.Abstracts;
using UnityEngine;
using UnityEngine.UI;

namespace Views
{
    public class InputView : View
    {
        [SerializeField] private Button _leftButton;
        [SerializeField] private Button _rightButton;
        [SerializeField] private Button _startButton;

        public Button LeftButton => _leftButton;
        public Button RightButton => _rightButton;
        public Button StartButton => _startButton;
    }
}