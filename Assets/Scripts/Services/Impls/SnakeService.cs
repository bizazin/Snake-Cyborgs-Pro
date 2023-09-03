using System;
using System.Collections.Generic;
using System.Linq;
using Controllers;
using Databases;
using DG.Tweening;
using Enums;
using ObjectPooling.Objects;
using ObjectPooling.Pools;
using Signals;
using UniRx;
using UnityEngine;
using Zenject;

namespace Services.Impls
{
    public class SnakeService : ISnakeService, IInitializable, IDisposable
    {
        private readonly IGameBoardSettingsDatabase _gameBoardSettingsDatabase;
        private readonly ISnakeSegmentPool _snakeSegmentPool;
        private readonly IFruitService _fruitService;
        private readonly ISnakeSettingsDatabase _snakeSettingsDatabase;
        private readonly SignalBus _signalBus;
        private readonly Queue<Vector2Int> _moveDirectionsQueue = new();
        private readonly List<SnakeSegment> _snakeSegments = new();

        private CompositeDisposable _disposable;
        private Vector2Int _snakeMoveDirection;
        private IDisposable _moveSnakeSubscription;
        private Cell[,] _cells;
        private float _standardBoardItemSize;
        private float _moveInterval;
        private float _moveAnimationDuration;
        private Transform _parent;

        public SnakeService
        (
            IGameBoardSettingsDatabase gameBoardSettingsDatabase,
            ISnakeSegmentPool snakeSegmentPool,
            IFruitService fruitService,
            ISnakeSettingsDatabase snakeSettingsDatabase,
            SignalBus signalBus
        )
        {
            _gameBoardSettingsDatabase = gameBoardSettingsDatabase;
            _snakeSegmentPool = snakeSegmentPool;
            _fruitService = fruitService;
            _snakeSettingsDatabase = snakeSettingsDatabase;
            _signalBus = signalBus;
        }

        public void Initialize()
        {
            _standardBoardItemSize = _gameBoardSettingsDatabase.Settings.StandardBoardItemSize;
        }

        public void Dispose() => _disposable?.Dispose();

        public void Setup(Cell[,] cells, Transform parent)
        {
            _cells = cells;
            _parent = parent;
        }

        public void SpawnFruit() => _fruitService.SpawnFruit(GetCellInFrontOfSnake());

        public void DespawnSnake()
        {
            foreach (var snakeSegment in _snakeSegments) 
                _snakeSegmentPool.Despawn(snakeSegment);
            _snakeSegments.Clear();
        }

        public void Start()
        {
            _snakeMoveDirection = new Vector2Int(0, 1);
            _moveInterval = _snakeSettingsDatabase.Settings.MoveIntervalS;
            _moveAnimationDuration = _snakeSettingsDatabase.Settings.MoveAnimationDurationS;
            _disposable = new CompositeDisposable();
            StartMoveSubscription();
        }

        public void SpawnSnake()
        {
            var centerPosition = GetCenterPosition();

            SpawnSnakeSegment(centerPosition);
            SpawnSnakeSegment(centerPosition - Vector2Int.up);
        }

        public void RotateSnake(ERotationSide rotationSide)
        {
            var newDirection = GetRotationDirection(rotationSide);
            if (!_moveDirectionsQueue.Contains(newDirection))
                _moveDirectionsQueue.Enqueue(newDirection);
        }

        public Vector2Int GetCellInFrontOfSnake() => _snakeSegments[0].Position + _snakeMoveDirection;

        private Vector2Int GetCenterPosition()
        {
            var gridSize = _gameBoardSettingsDatabase.Settings.GridSize;
            return new Vector2Int(gridSize / 2, gridSize / 2);
        }

        private void StartMoveSubscription() =>
            _moveSnakeSubscription = Observable.Interval(TimeSpan.FromSeconds(_moveInterval))
                .Subscribe(_ => MoveSnake())
                .AddTo(_disposable);

        private void SpawnSnakeSegment(Vector2Int position)
        {
            _cells[position.x, position.y].Type = ECellType.SnakeSegment;
            var segment = _snakeSegmentPool.Spawn(_parent);
            if (segment.transform.localScale != Vector3.one * _standardBoardItemSize)
                throw new Exception($"[{nameof(SnakeService)}] {segment} size is not equal to {_gameBoardSettingsDatabase.Settings.StandardBoardItemSize}");
            _snakeSegments.Add(segment);
            segment.Position = position;
            segment.gameObject.transform.position = new Vector3(position.x, _standardBoardItemSize / 2, position.y);
        }
        
        private void MoveSnake()
        {
            if (_moveDirectionsQueue.Any())
                _snakeMoveDirection = _moveDirectionsQueue.Dequeue();

            var nextHeadPosition = GetCellInFrontOfSnake();
            var nextCellType = _cells[nextHeadPosition.x, nextHeadPosition.y].Type;

            if (CheckCollision(nextCellType))
                return;

            if (nextCellType == ECellType.Fruit)
            {
                _fruitService.DespawnFruit();
                GrowSnake();
            }

            ExecuteMoveAnimation(nextHeadPosition, nextCellType);
        }

        private bool CheckCollision(ECellType cellType)
        {
            if (cellType is ECellType.SnakeSegment or ECellType.Obstacle)
            {
                _signalBus.Fire(new SignalLevelResult(ELevelResultType.Lose));
                Dispose();
                return true;
            }

            return false;
        }

        private void ExecuteMoveAnimation(Vector2Int nextHeadPosition, ECellType nextCellType)
        {
            var sequence = DOTween.Sequence();

            for (var i = _snakeSegments.Count - 1; i > 0; i--)
                MoveSegmentToPreviousPosition(sequence, i);

            MoveSnakeHead(sequence, nextHeadPosition, nextCellType);
        }

        private void MoveSegmentToPreviousPosition(Sequence sequence, int segmentIndex)
        {
            var segment = _snakeSegments[segmentIndex];
            var prevSegmentPos = _snakeSegments[segmentIndex - 1].Position;

            sequence.Join(segment.gameObject.transform.DOMove(
                    new Vector3(prevSegmentPos.x, _standardBoardItemSize / 2, prevSegmentPos.y),
                    _moveAnimationDuration)
                .OnComplete(() => UpdateSegmentPosition(segment, segmentIndex, prevSegmentPos)));
        }

        private void UpdateSegmentPosition(SnakeSegment segment, int index, Vector2Int position)
        {
            if (index == _snakeSegments.Count - 1)
                _cells[segment.Position.x, segment.Position.y].Type = ECellType.Empty;

            segment.Position = position;
        }

        private void MoveSnakeHead(Sequence sequence, Vector2Int position, ECellType nextCellType) =>
            sequence.Join(_snakeSegments[0].gameObject.transform.DOMove(
                    new Vector3(position.x, _standardBoardItemSize / 2, position.y),
                    _moveAnimationDuration)
                .OnComplete(() => UpdateSnakeHeadPosition(position, nextCellType)));

        private void UpdateSnakeHeadPosition(Vector2Int position, ECellType nextCellType)
        {
            _snakeSegments[0].Position = position;

            if (nextCellType == ECellType.Fruit)
                SpawnFruit();
            _cells[position.x, position.y].Type = ECellType.SnakeSegment;
        }

        private Vector2Int GetRotationDirection(ERotationSide rotationSide)
        {
            return rotationSide == ERotationSide.Right
                ? Rotate90(_snakeMoveDirection, true)
                : Rotate90(_snakeMoveDirection, false);
        }

        private Vector2Int Rotate90(Vector2Int direction, bool clockwise)
        {
            return clockwise
                ? new Vector2Int(direction.y, -direction.x)
                : new Vector2Int(-direction.y, direction.x);
        }

        private void GrowSnake()
        {
            var lastSegment = _snakeSegments[^1];
            var penultimateSegment = _snakeSegments[^2];
            var tailDirection = lastSegment.Position - penultimateSegment.Position;
            var newTailPosition = lastSegment.Position + tailDirection;

            SpawnSnakeSegment(newTailPosition);
            if (_snakeSegments.Count == _snakeSettingsDatabase.Settings.MaxSize)
            {
                _signalBus.Fire(new SignalLevelResult(ELevelResultType.Win));
                Dispose();
            }

            UpdateMoveSpeed();
            StartMoveSubscription();
        }

        private void UpdateMoveSpeed()
        {
            var speedMultiplier = _snakeSettingsDatabase.Settings.FruitSpeedBoostMultiplier;

            _moveInterval *= speedMultiplier;
            _moveAnimationDuration *= speedMultiplier;

            _moveSnakeSubscription?.Dispose();
        }
    }
}
