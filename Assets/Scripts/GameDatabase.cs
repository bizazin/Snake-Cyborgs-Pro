using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Databases/GameDatabase", fileName = "GameDatabase")]
public class GameDatabase : ScriptableObject
{
    [Header("Game Board")] 
    [SerializeField] private int _gridSize;
    [SerializeField] private float _standardBoardItemSize;
    [SerializeField] private Vector2Int _extraObstaclesCount;

    [Header("Snake")] 
    [SerializeField] private float _moveIntervalS;
    [SerializeField] private int _maxSize;
    [SerializeField] private float _fruitSpeedBoostMultiplier;
    [SerializeField] private LevelResultTypeVo[] _levelResults;

    private Dictionary<bool, string> _gameResultsDictionary;

    public int GridSize => _gridSize;
    public float StandardBoardItemSize => _standardBoardItemSize;
    public Vector2Int ExtraObstaclesCount => _extraObstaclesCount;
    public float MoveIntervalS => _moveIntervalS;
    public int MaxSize => _maxSize;
    public float FruitSpeedBoostMultiplier => _fruitSpeedBoostMultiplier;

    private void OnEnable()
    {
        _gameResultsDictionary = new Dictionary<bool, string>();

        foreach (var gameResult in _levelResults)
            _gameResultsDictionary.Add(gameResult.IsWin, gameResult.Text);
    }

    public string GetLevelResult(bool isWin)
    {
        try
        {
            return _gameResultsDictionary[isWin];
        }
        catch (Exception e)
        {
            throw new Exception(
                $"[{nameof(GameDatabase)}] Game result" +
                $" with type {isWin.ToString()} was not present in the dictionary. {e.StackTrace}");
        }
    }
}

[Serializable]
public class LevelResultTypeVo
{
    public string Text;
    public bool IsWin;
}


public enum ECellType
{
    Empty = 0,
    SnakeSegment = 1,
    Fruit = 2,
    Obstacle = 3
}
