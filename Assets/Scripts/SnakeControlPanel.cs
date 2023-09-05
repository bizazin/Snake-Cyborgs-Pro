using UnityEngine;
using UnityEngine.UI;

public class SnakeControlPanel : MonoBehaviour
{
    [SerializeField] private Text _headerText;
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _restartButton;
    [SerializeField] private GameBoard _gameBoard;
    [SerializeField] private GameDatabase _gameDatabase;
    [SerializeField] private Button _leftButton;
    [SerializeField] private Button _rightButton;
    [SerializeField] private GameObject _levelResultContainer;

    private void Start()
    {
        SetRestartActive(false);
    }

    private void OnEnable()
    {
        _startButton.onClick.AddListener(OnStartButtonClick);
        _restartButton.onClick.AddListener(OnRestartButtonClick);
        _leftButton.onClick.AddListener(OnLeftButtonClick);
        _rightButton.onClick.AddListener(OnRightButtonClick);
    }

    private void OnDisable()
    {
        _startButton.onClick.RemoveListener(OnStartButtonClick);
        _restartButton.onClick.RemoveListener(OnRestartButtonClick);
        _leftButton.onClick.RemoveListener(OnLeftButtonClick);
        _rightButton.onClick.RemoveListener(OnRightButtonClick);
    }

    public void SetLevelResult(bool isWin)
    {
        _levelResultContainer.SetActive(true);
        _headerText.text = _gameDatabase.GetLevelResult(isWin);
    }

    private void OnStartButtonClick()
    {
        _gameBoard.StartLevel();
        SetRestartActive(true);
        _levelResultContainer.SetActive(false);
    }

    private void OnRestartButtonClick()
    {
        _levelResultContainer.SetActive(false);
        _gameBoard.Restart();
    }

    private void OnLeftButtonClick() => _gameBoard.RotateSnake(isRight: false);

    private void OnRightButtonClick() => _gameBoard.RotateSnake(isRight: true);

    private void SetRestartActive(bool isActive)
    {
        _startButton.gameObject.SetActive(!isActive);
        _restartButton.gameObject.SetActive(isActive);
    }
}