using Enums;

namespace Services
{
    public interface IGameBoardService
    {
        void RotateSnake(ERotationSide rotationSide);
        void Start();
    }
}