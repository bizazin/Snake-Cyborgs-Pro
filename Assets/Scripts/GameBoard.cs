using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameBoard : MonoBehaviour
{
    [SerializeField] private GameObject _cellPrefab;
    [SerializeField] private GameObject _obstaclePrefab;
    [SerializeField] private GameObject _snakeSegmentPrefab;
    [SerializeField] private GameObject _fruitPrefab;
    [SerializeField] private GameDatabase _gameDatabase;
    [SerializeField] private SnakeControlPanel _snakeControlPanel;

    private readonly List<GameObject> _extraObstacles = new();
    private readonly List<SnakeSegment> _snakeSegments = new();

    private GameObject _fruit;
    private Coroutine _moveSnakeCoroutine;
    private Vector2Int _lastMoveDirection;
    private Vector2Int _nextPossibleMoveDirection;
    private Cell[,] _cells;
    private int _gridSize;
    private float _standardBoardItemSize;
    private float _moveInterval;
    private bool _isGameFinished;

    private void Awake()
    {
        _standardBoardItemSize = _gameDatabase.StandardBoardItemSize;
        _gridSize = _gameDatabase.GridSize;
        _cells = new Cell[_gridSize, _gridSize];
    }

    private void Start()
    {
        SpawnBoard();
        SpawnBorder();
        SpawnSnake();
        SpawnExtraObstacles();
        SpawnFruit(GetCellInFrontOfSnake());
    }

    private void OnDestroy()
    {
        if (_moveSnakeCoroutine != null)
            StopCoroutine(_moveSnakeCoroutine);
    }

    public void StartLevel()
    {
        _isGameFinished = false;
        _nextPossibleMoveDirection = _lastMoveDirection = Vector2Int.up;
        _moveInterval = _gameDatabase.MoveIntervalS;
        StartMoveSnakeCoroutine();
    }

    public void Restart()
    {
        DespawnFruit();
        DespawnSnake();
        DespawnExtraObstacles();
        ClearBoardCellTypes();
            
        SpawnSnake();
        SpawnExtraObstacles();
        SpawnFruit(GetCellInFrontOfSnake());
        StartLevel();
    }

    public void RotateSnake(bool isRight) => 
        _nextPossibleMoveDirection = GetRotationDirection(isRight);

    private void SpawnBoard()
    {
        for (var x = 0; x < _gridSize; x++)
            for (var y = 0; y < _gridSize; y++)
            {
                var cell = Instantiate(_cellPrefab, new Vector3(x, 0, y), Quaternion.Euler(90, 0, 0),
                    transform);
                if (cell.transform.localScale != Vector3.one * _standardBoardItemSize)
                    throw new Exception(
                        $"[{nameof(GameBoard)}] {cell} size is not equal to {_gameDatabase.StandardBoardItemSize}");

                _cells[x, y] = new Cell {Type = ECellType.Empty};
            }
    }

    private void SpawnBorder()
    {
        for (var x = 0; x < _gridSize; x++)
            for (var z = 0; z < _gridSize; z++)
                if (x == 0 || x == _gridSize - 1 || z == 0 || z == _gridSize - 1)
                {
                    var obstacle = Instantiate(_obstaclePrefab, new Vector3(x, _standardBoardItemSize / 2, z),
                        Quaternion.identity, transform);
                    if (obstacle.transform.localScale != Vector3.one * _standardBoardItemSize)
                        throw new Exception(
                            $"[{nameof(GameBoard)}] {obstacle} size is not equal to {_gameDatabase.StandardBoardItemSize}");
                    _cells[x, z].Type = ECellType.Obstacle;
                }
    }

    private void SpawnSnake()
    {
        var centerPosition = GetCenterPosition();

        SpawnSnakeSegment(centerPosition);
        SpawnSnakeSegment(centerPosition - Vector2Int.up);
    }

    private void SpawnFruit(Vector2Int forbiddenPosition)
    {
        int x, z;
        do
        {
            x = Random.Range(1, _gameDatabase.GridSize - 1);
            z = Random.Range(1, _gameDatabase.GridSize - 1);
        } while (_cells[x, z].Type != ECellType.Empty || new Vector2Int(x, z) == forbiddenPosition);

        _cells[x, z].Type = ECellType.Fruit;
        _fruit = Instantiate(_fruitPrefab, new Vector3(x, _standardBoardItemSize / 2, z), Quaternion.identity,
            transform);

        var standardBoardItemSize = _gameDatabase.StandardBoardItemSize;
        if (_fruit.transform.localScale != Vector3.one * standardBoardItemSize)
            throw new Exception(
                $"[{nameof(GameBoard)}] {_fruit} size is not equal to {standardBoardItemSize}");
    }

    private void SpawnExtraObstacles()
    {
        var forbiddenPosition = GetCellInFrontOfSnake();
        var obstaclesToSpawn = Random.Range(_gameDatabase.ExtraObstaclesCount.x,
            _gameDatabase.ExtraObstaclesCount.y + 1);

        for (var i = 0; i < obstaclesToSpawn; i++)
        {
            Vector2Int randomPosition;
            do
                randomPosition = new Vector2Int(Random.Range(0, _gridSize), Random.Range(0, _gridSize));
            while (_cells[randomPosition.x, randomPosition.y].Type != ECellType.Empty ||
                   randomPosition == forbiddenPosition);

            _cells[randomPosition.x, randomPosition.y].Type = ECellType.Obstacle;
            var obstacle = Instantiate(_obstaclePrefab,
                new Vector3(randomPosition.x, _standardBoardItemSize / 2, randomPosition.y), Quaternion.identity,
                transform);
            _extraObstacles.Add(obstacle);
        }
    }

    private void DespawnFruit() => Destroy(_fruit.gameObject);

    private void DespawnSnake()
    {
        foreach (var snakeSegment in _snakeSegments)
            Destroy(snakeSegment.GameObject);
        _snakeSegments.Clear();
    }

    private void DespawnExtraObstacles()
    {
        foreach (var obstacle in _extraObstacles)
            Destroy(obstacle.gameObject);
        _extraObstacles.Clear();
    }

    private Vector2Int GetCenterPosition()
    {
        var gridSize = _gameDatabase.GridSize;
        return new Vector2Int(gridSize / 2, gridSize / 2);
    }

    private void StartMoveSnakeCoroutine()
    {
        if (_isGameFinished) return;

        if (_moveSnakeCoroutine != null)
            StopCoroutine(_moveSnakeCoroutine);
        _moveSnakeCoroutine = StartCoroutine(MoveSnakeRoutine());
    }

    private IEnumerator MoveSnakeRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(_moveInterval);
            MoveSnake();
            _lastMoveDirection = _nextPossibleMoveDirection;
        }
    }

    private void MoveSnake()
    {
        var nextHeadPosition = GetCellInFrontOfSnake();
        var nextCellType = _cells[nextHeadPosition.x, nextHeadPosition.y].Type;

        if (CheckCollision(nextCellType))
            return;

        if (nextCellType == ECellType.Fruit)
        {
            DespawnFruit();
            GrowSnake();
        }

        ExecuteMoveAnimation(nextHeadPosition, nextCellType);
    }

    private bool CheckCollision(ECellType cellType)
    {
        if (cellType is ECellType.SnakeSegment or ECellType.Obstacle)
        {
            _snakeControlPanel.SetLevelResult(isWin: false);
            if (_moveSnakeCoroutine != null) StopCoroutine(_moveSnakeCoroutine);
            _isGameFinished = true;
            return true;
        }

        return false;
    }

    private void GrowSnake()
    {
        var lastSegment = _snakeSegments[^1];
        var penultimateSegment = _snakeSegments[^2];
        var tailDirection = lastSegment.Position - penultimateSegment.Position;
        var newTailPosition = lastSegment.Position + tailDirection;

        SpawnSnakeSegment(newTailPosition);
        if (_snakeSegments.Count == _gameDatabase.MaxSize)
        {
            _snakeControlPanel.SetLevelResult(isWin: true);
            if (_moveSnakeCoroutine != null) 
                StopCoroutine(_moveSnakeCoroutine);
            _isGameFinished = true;
            return;
        }

        _moveInterval *= _gameDatabase.FruitSpeedBoostMultiplier;
        StartMoveSnakeCoroutine();
    }

    private void ExecuteMoveAnimation(Vector2Int nextHeadPosition, ECellType nextCellType)
    {
        for (var i = _snakeSegments.Count - 1; i > 0; i--)
            MoveSegmentToPreviousPosition(i);

        MoveSnakeHead(nextHeadPosition, nextCellType);
    }

    private void MoveSegmentToPreviousPosition(int segmentIndex)
    {
        var segment = _snakeSegments[segmentIndex];
        var prevSegmentPos = _snakeSegments[segmentIndex - 1].Position;

        segment.GameObject.transform.position =
            new Vector3(prevSegmentPos.x, _standardBoardItemSize / 2, prevSegmentPos.y);
        UpdateSegmentPosition(segment, segmentIndex, prevSegmentPos);
    }

    private void MoveSnakeHead(Vector2Int position, ECellType nextCellType)
    {
        _snakeSegments[0].GameObject.transform.position =
            new Vector3(position.x, _standardBoardItemSize / 2, position.y);
        UpdateSnakeHeadPosition(position, nextCellType);
    }

    private void UpdateSnakeHeadPosition(Vector2Int position, ECellType nextCellType)
    {
        _snakeSegments[0].Position = position;

        if (nextCellType == ECellType.Fruit)
            SpawnFruit(GetCellInFrontOfSnake());
        _cells[position.x, position.y].Type = ECellType.SnakeSegment;
    }

    private void UpdateSegmentPosition(SnakeSegment segment, int index, Vector2Int position)
    {
        if (index == _snakeSegments.Count - 1)
            _cells[segment.Position.x, segment.Position.y].Type = ECellType.Empty;

        segment.Position = position;
    }

    private void ClearBoardCellTypes()
    {
        foreach (var cell in _cells)
            cell.Type = ECellType.Empty;
        for (var x = 0; x < _gridSize; x++)
            for (var z = 0; z < _gridSize; z++)
                if (x == 0 || x == _gridSize - 1 || z == 0 || z == _gridSize - 1)
                    _cells[x, z].Type = ECellType.Obstacle;
    }

    private Vector2Int GetRotationDirection(bool isRight)
    {
        return isRight
            ? Rotate90(_lastMoveDirection, true)
            : Rotate90(_lastMoveDirection, false);
    }

    private Vector2Int Rotate90(Vector2Int direction, bool clockwise)
    {
        return clockwise
            ? new Vector2Int(direction.y, -direction.x)
            : new Vector2Int(-direction.y, direction.x);
    }

    private void SpawnSnakeSegment(Vector2Int position)
    {
        _cells[position.x, position.y].Type = ECellType.SnakeSegment;
        var segmentObj = Instantiate(_snakeSegmentPrefab,
            new Vector3(position.x, _standardBoardItemSize / 2, position.y), Quaternion.identity, transform);
        if (segmentObj.transform.localScale != Vector3.one * _standardBoardItemSize)
            throw new Exception(
                $"[{nameof(GameBoard)}] {segmentObj} size is not equal to {_gameDatabase.StandardBoardItemSize}");
        _snakeSegments.Add(new SnakeSegment {Position = position, GameObject = segmentObj});
    }

    private Vector2Int GetCellInFrontOfSnake() => _snakeSegments[0].Position + _lastMoveDirection;
}
    
public class SnakeSegment
{
    public GameObject GameObject { get; set; }
    public Vector2Int Position { get; set; }
}

public class Cell
{
    public ECellType Type { get; set; }
}