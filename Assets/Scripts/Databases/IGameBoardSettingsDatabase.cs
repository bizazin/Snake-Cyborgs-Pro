using Models;

namespace Databases
{
    public interface IGameBoardSettingsDatabase
    {
        GameBoardSettingsVo Settings { get; }
    }
}