using System;

namespace Models
{
    [Serializable]
    public class SnakeSettingsVo
    {
        public float MoveIntervalS;
        public float MoveAnimationDurationS;
        public int MaxSize;
        public float FruitSpeedBoostMultiplier;
    }
}