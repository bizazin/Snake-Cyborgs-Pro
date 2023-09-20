using System;
using UnityEngine;

namespace Models
{
    [Serializable]
    public class GameBoardSettingsVo
    {
        public int GridSize;
        public float StandardBoardItemSize;
        public Vector2Int ExtraObstaclesCount;
    }
}