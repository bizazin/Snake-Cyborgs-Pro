using Enums;
using ObjectPooling.Objects;
using UnityEngine;

namespace Services
{
    public interface ISnakeService
    {
        void RotateSnake(ERotationSide left);
        void SpawnSnake();
        void Setup(Cell[,] cells, Transform parent);
        void SpawnFruit();
        void DespawnSnake();
        void Start();
        Vector2Int GetCellInFrontOfSnake();
    }
}