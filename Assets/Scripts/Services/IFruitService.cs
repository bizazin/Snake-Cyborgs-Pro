using ObjectPooling.Objects;
using UnityEngine;

namespace Services
{
    public interface IFruitService
    {
        void SpawnFruit(Vector2Int forbiddenPosition);
        void Setup(Cell[,] cells, Transform gameBoardParent);
        void DespawnFruit();
    }
}