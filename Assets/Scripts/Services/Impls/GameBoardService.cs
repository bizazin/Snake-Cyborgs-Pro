using System;
using System.Collections.Generic;
using Databases;
using Enums;
using ObjectPooling.Objects;
using ObjectPooling.Pools;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace Services.Impls
{
    public class GameBoardService : IGameBoardService, IInitializable
    {
        private readonly ICellPool _cellPool;
        private readonly IObstaclePool _obstaclePool;
        private readonly IGameBoardSettingsDatabase _gameBoardSettingsDatabase;
        private readonly Transform _gameBoardParent;
        private readonly ISnakeService _snakeService;
        private readonly IFruitService _fruitService;
        private readonly List<Obstacle> _extraObstacles = new();

        private Cell[,] _cells;
        private int _gridSize;
        private float _standardBoardItemSize;

        public GameBoardService
        (
            ICellPool cellPool,
            IObstaclePool obstaclePool,
            IGameBoardSettingsDatabase gameBoardSettingsDatabase,
            Transform gameBoardParent,
            ISnakeService snakeService,
            IFruitService fruitService
        )
        {
            _cellPool = cellPool;
            _obstaclePool = obstaclePool;
            _gameBoardSettingsDatabase = gameBoardSettingsDatabase;
            _gameBoardParent = gameBoardParent;
            _snakeService = snakeService;
            _fruitService = fruitService;
        }

        public void Initialize()
        {
            _standardBoardItemSize = _gameBoardSettingsDatabase.Settings.StandardBoardItemSize;
            _gridSize = _gameBoardSettingsDatabase.Settings.GridSize;
            _cells = new Cell[_gridSize, _gridSize];
            _snakeService.Setup(_cells, _gameBoardParent);
            _fruitService.Setup(_cells, _gameBoardParent);

            SpawnBoard();
            SpawnBorder();
            _snakeService.SpawnSnake();
            SpawnExtraObstacles();
            _snakeService.SpawnFruit();
        }

        public void Start() => _snakeService.Start();

        public void Restart()
        {
            _fruitService.DespawnFruit();
            _snakeService.DespawnSnake();
            DespawnExtraObstacles();
            ClearBoardCellTypes();
            _snakeService.SpawnSnake();
            SpawnExtraObstacles();
            _snakeService.SpawnFruit();
            
            Start();
        }

        private void SpawnBoard()
        {
            for (var x = 0; x < _gridSize; x++)
                for (var y = 0; y < _gridSize; y++)
                {
                    var cell = _cellPool.Spawn(_gameBoardParent);
                    if (cell.transform.localScale != Vector3.one * _standardBoardItemSize)
                        throw new Exception(
                            $"[{nameof(GameBoardService)}] {cell} size is not equal to {_gameBoardSettingsDatabase.Settings.StandardBoardItemSize}");
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
                        if (obstacle.transform.localScale != Vector3.one * _standardBoardItemSize)
                            throw new Exception(
                                $"[{nameof(GameBoardService)}] {obstacle} size is not equal to {_gameBoardSettingsDatabase.Settings.StandardBoardItemSize}");
                        obstacle.gameObject.transform.position = new Vector3(x, _standardBoardItemSize / 2, z);
                        _cells[x, z].Type = ECellType.Obstacle;
                    }
        }

        private void SpawnExtraObstacles()
        {
            var forbiddenPosition = _snakeService.GetCellInFrontOfSnake();
            var obstaclesToSpawn = Random.Range(_gameBoardSettingsDatabase.Settings.ExtraObstaclesCount.x,
                _gameBoardSettingsDatabase.Settings.ExtraObstaclesCount.y + 1);

            for (var i = 0; i < obstaclesToSpawn; i++)
            {
                Vector2Int randomPosition;
                do
                    randomPosition = new Vector2Int(Random.Range(0, _gridSize), Random.Range(0, _gridSize));
                while (_cells[randomPosition.x, randomPosition.y].Type != ECellType.Empty ||
                       randomPosition == forbiddenPosition);

                _cells[randomPosition.x, randomPosition.y].Type = ECellType.Obstacle;
                var obstacle = _obstaclePool.Spawn(_gameBoardParent);
                _extraObstacles.Add(obstacle);
                obstacle.gameObject.transform.position =
                    new Vector3(randomPosition.x, _standardBoardItemSize / 2, randomPosition.y);
            }
        }

        private void DespawnExtraObstacles()
        {
            foreach (var obstacle in _extraObstacles) 
                _obstaclePool.Despawn(obstacle);
            _extraObstacles.Clear();
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

    }
}