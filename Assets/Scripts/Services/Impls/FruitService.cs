using System;
using Databases;
using Enums;
using ObjectPooling.Objects;
using ObjectPooling.Pools;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Services.Impls
{
    public class FruitService : IFruitService
    {
        private readonly IGameBoardSettingsDatabase _gameBoardSettingsDatabase;
        private readonly IFruitPool _fruitPool;
        private Cell[,] _cells;
        private Fruit _fruit;
        private Transform _parent;

        public FruitService
        (
            IGameBoardSettingsDatabase gameBoardSettingsDatabase,
            IFruitPool fruitPool
        )
        {
            _gameBoardSettingsDatabase = gameBoardSettingsDatabase;
            _fruitPool = fruitPool;
        }

        public void Setup(Cell[,] cells, Transform parent)
        {
            _cells = cells;
            _parent = parent;
        }

        public void DespawnFruit() => _fruitPool.Despawn(_fruit);

        public void SpawnFruit(Vector2Int forbiddenPosition)
        {
            int x, z;
            do
            {
                x = Random.Range(1, _gameBoardSettingsDatabase.Settings.GridSize - 1);
                z = Random.Range(1, _gameBoardSettingsDatabase.Settings.GridSize - 1);
            } while (_cells[x, z].Type != ECellType.Empty || new Vector2Int(x, z) == forbiddenPosition);

            _cells[x, z].Type = ECellType.Fruit;
            _fruit = _fruitPool.Spawn(_parent);
            
            var standardBoardItemSize = _gameBoardSettingsDatabase.Settings.StandardBoardItemSize;
            if (_fruit.transform.localScale != Vector3.one * standardBoardItemSize)
                throw new Exception(
                    $"[{nameof(GameBoardService)}] {_fruit} size is not equal to {standardBoardItemSize}");

            _fruit.gameObject.transform.position = new Vector3(x, standardBoardItemSize / 2, z);
        }
    }
}