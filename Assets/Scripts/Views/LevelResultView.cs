using Core.Abstracts;
using UnityEngine;
using UnityEngine.UI;

namespace Views
{
    public class LevelResultView : View
    {
        [SerializeField] private Text _headerText;
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _restartButton;

        public Text HeaderText => _headerText;
        public Button StartButton => _startButton;
        public Button RestartButton => _restartButton;

        public void SetRestartActive(bool isActive)
        {
            _startButton.gameObject.SetActive(!isActive);
            _restartButton.gameObject.SetActive(isActive);
        }
    }
}