using System;
using System.Collections.Generic;
using System.Linq;
using Databases;
using Databases.Impls;
using DG.Tweening;
using Enums;
using ObjectPooling.Objects;
using ObjectPooling.Pools;
using UniRx;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace Services.Impls
{
    public class GameBoardService : IGameBoardService, IInitializable, IDisposable
    {
        private readonly ICellPool _cellPool;
        private readonly IObstaclePool _obstaclePool;
        private readonly IGameBoardSettingsDatabase _gameBoardSettingsDatabase;
        private readonly Transform _gameBoardParent;
        private readonly ISnakeSegmentPool _snakeSegmentPool;
        private readonly IFruitPool _fruitPool;
        private readonly CompositeDisposable _disposable = new();
        private readonly List<SnakeSegment> _snakeSegments = new();
        private readonly Queue<Vector2Int> _moveDirectionsQueue = new();

        private Cell[,] _cells;
        private int _gridSize;
        private Vector2Int _snakeMoveDirection = new(0, 1);
        private Fruit _fruit;
        private IDisposable _moveSnakeSubscription;
        private float _moveInterval = .5f;
        private float _moveAnimationDuration = 0.2f;


        public GameBoardService
        (
            ICellPool cellPool,
            IObstaclePool obstaclePool,
            IGameBoardSettingsDatabase gameBoardSettingsDatabase,
            Transform gameBoardParent,
            ISnakeSegmentPool snakeSegmentPool,
            IFruitPool fruitPool
        )
        {
            _cellPool = cellPool;
            _obstaclePool = obstaclePool;
            _gameBoardSettingsDatabase = gameBoardSettingsDatabase;
            _gameBoardParent = gameBoardParent;
            _snakeSegmentPool = snakeSegmentPool;
            _fruitPool = fruitPool;
        }

        public void Initialize()
        {
            _gridSize = _gameBoardSettingsDatabase.Settings.GridSize;
            _cells = new Cell[_gridSize, _gridSize];
            SpawnBoard();
            SpawnBorder();
            SpawnSnake();
            SpawnExtraObstacles();
            SpawnFruit();
        }

        public void Dispose() => _disposable?.Dispose();

        public void MoveLeft()
        {
            var newDirection = Rotate90(_snakeMoveDirection, false);
            if (!_moveDirectionsQueue.Contains(newDirection))
                _moveDirectionsQueue.Enqueue(newDirection);
        }

        public void MoveRight()
        {
            var newDirection = Rotate90(_snakeMoveDirection, true);
            if (!_moveDirectionsQueue.Contains(newDirection))
                _moveDirectionsQueue.Enqueue(newDirection);
        }


        private Vector2Int Rotate90(Vector2Int direction, bool clockwise)
        {
            if (clockwise)
                return new Vector2Int(direction.y, -direction.x);
            return new Vector2Int(-direction.y, direction.x);
        }


        public void Start()
        {
            _moveSnakeSubscription = Observable.Interval(TimeSpan.FromSeconds(_moveInterval))
                .Subscribe(_ => MoveSnake())
                .AddTo(_disposable);
        }

        private void MoveSnake()
        {
            if (_moveDirectionsQueue.Any())
                _snakeMoveDirection = _moveDirectionsQueue.Dequeue();

            var nextHeadPosition = GetCellInFrontOfSnake();
            var nextCellType = _cells[nextHeadPosition.x, nextHeadPosition.y].Type;

            if (nextCellType is ECellType.SnakeSegment or ECellType.Obstacle)
            {
                Debug.Log("You Lose!");
                _moveSnakeSubscription?.Dispose();
                return;
            }

            if (nextCellType == ECellType.Fruit)
            {
                _fruitPool.Despawn(_fruit);
                GrowSnake();
            }

            var sequence = DOTween.Sequence();

            for (var i = _snakeSegments.Count - 1; i > 0; i--)
            {
                var segment = _snakeSegments[i];
                var prevSegmentPos = _snakeSegments[i - 1].Position;

                var index = i;
                sequence.Join(segment.gameObject.transform.DOMove(
                        new Vector3(prevSegmentPos.x, 0.5f, prevSegmentPos.y), _moveAnimationDuration)
                    .OnComplete(() =>
                    {
                        if (index == _snakeSegments.Count - 1)
                            _cells[segment.Position.x, segment.Position.y].Type = ECellType.Empty;
                        segment.Position = prevSegmentPos;
                    }));
            }

            sequence.Join(_snakeSegments[0].gameObject.transform.DOMove(
                    new Vector3(nextHeadPosition.x, 0.5f, nextHeadPosition.y), _moveAnimationDuration)
                .OnComplete(() =>
                {
                    _cells[_snakeSegments[0].Position.x, _snakeSegments[0].Position.y].Type = ECellType.Empty;
                    _snakeSegments[0].Position = nextHeadPosition;
                    
                    if (nextCellType == ECellType.Fruit) SpawnFruit();

                    _cells[nextHeadPosition.x, nextHeadPosition.y].Type = ECellType.SnakeSegment;
                }));
        }


        private void GrowSnake()
        {
            var lastSegment = _snakeSegments[^1];
            var penultimateSegment = _snakeSegments[^2];

            var tailDirection = lastSegment.Position - penultimateSegment.Position;
            var newTailPosition = lastSegment.Position + tailDirection;


            var newSegment = _snakeSegmentPool.Spawn(_gameBoardParent);
            _snakeSegments.Add(newSegment);
            newSegment.Position = newTailPosition;
            newSegment.gameObject.transform.position = new Vector3(newTailPosition.x, 0.5f, newTailPosition.y);
            _cells[newTailPosition.x, newTailPosition.y].Type = ECellType.SnakeSegment;

            if (_snakeSegments.Count == 10)
            {
                Debug.Log("You Win!");
                _moveSnakeSubscription?.Dispose();
            }
        }

        private void SpawnBoard()
        {
            for (var x = 0; x < _gridSize; x++)
                for (var y = 0; y < _gridSize; y++)
                {
                    var cell = _cellPool.Spawn(_gameBoardParent);
                    var cellObj = cell.gameObject;
                    cellObj.transform.position = new Vector3(x, 0, y);
                    cellObj.transform.rotation = Quaternion.Euler(90, 0, 0);

                    _cells[x, y] = cell;
                    _cells[x, y].Type = ECellType.Empty;
                }
        }

        private void SpawnBorder()
        {
            for (var x = 0; x < _gridSize; x++)
                for (var z = 0; z < _gridSize; z++)
                    if (x == 0 || x == _gridSize - 1 || z == 0 || z == _gridSize - 1)
                    {
                        var obstacle = _obstaclePool.Spawn(_gameBoardParent);
                        obstacle.gameObject.transform.position = new Vector3(x, 0.5f, z);
                        _cells[x, z].Type = ECellType.Obstacle;
                    }
        }

        private void SpawnSnake()
        {
            var centerX = _gridSize / 2;
            var centerZ = _gridSize / 2;

            _cells[centerX, centerZ].Type = ECellType.SnakeSegment;
            var head = _snakeSegmentPool.Spawn(_gameBoardParent);
            _snakeSegments.Add(head);
            head.Position = new Vector2Int(centerX, centerZ);
            head.gameObject.transform.position = new Vector3(centerX, 0.5f, centerZ);

            _cells[centerX, centerZ - 1].Type = ECellType.SnakeSegment;
            var snakeSegment = _snakeSegmentPool.Spawn(_gameBoardParent);
            _snakeSegments.Add(snakeSegment);
            snakeSegment.Position = new Vector2Int(centerX, centerZ - 1);
            snakeSegment.gameObject.transform.position = new Vector3(centerX, 0.5f, centerZ - 1);
        }

        private void SpawnExtraObstacles()
        {
            var forbiddenPosition = GetCellInFrontOfSnake();
            var obstaclesToSpawn = Random.Range(3, 6);

            for (var i = 0; i < obstaclesToSpawn; i++)
            {
                Vector2Int randomPosition;
                do
                    randomPosition = new Vector2Int(Random.Range(0, _gridSize), Random.Range(0, _gridSize));
                while (_cells[randomPosition.x, randomPosition.y].Type != ECellType.Empty ||
                       randomPosition == forbiddenPosition);

                _cells[randomPosition.x, randomPosition.y].Type = ECellType.Obstacle;
                var obstacle = _obstaclePool.Spawn(_gameBoardParent);
                obstacle.gameObject.transform.position = new Vector3(randomPosition.x, 0.5f, randomPosition.y);
            }
        }


        private void SpawnFruit()
        {
            Vector2Int forbiddenPosition = GetCellInFrontOfSnake();

            int x, z;
            do
            {
                x = Random.Range(1, _gridSize - 1);
                z = Random.Range(1, _gridSize - 1);
            } while (_cells[x, z].Type != ECellType.Empty || new Vector2Int(x, z) == forbiddenPosition);

            _cells[x, z].Type = ECellType.Fruit;
            _fruit = _fruitPool.Spawn(_gameBoardParent);
            _fruit.gameObject.transform.position = new Vector3(x, 0.5f, z);
        }

        private Vector2Int GetCellInFrontOfSnake() => _snakeSegments[0].Position + _snakeMoveDirection;
    }
}