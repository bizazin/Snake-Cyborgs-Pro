using Models;
using UnityEngine;

namespace Databases.Impls
{
    [CreateAssetMenu(menuName = "Databases/GameBoardSettingsDatabase", fileName = "GameBoardSettingsDatabase")]
    public class GameBoardSettingsDatabase : ScriptableObject, IGameBoardSettingsDatabase
    {
        [SerializeField] private GameBoardSettingsVo _gameBoardSettings;

        public GameBoardSettingsVo Settings => _gameBoardSettings;
    }
}