using Models;
using UnityEngine;

namespace Databases.Impls
{
    [CreateAssetMenu(menuName = "Databases/SnakeSettingsDatabase", fileName = "SnakeSettingsDatabase")]
    public class SnakeSettingsDatabase : ScriptableObject, ISnakeSettingsDatabase
    {
        [SerializeField] private SnakeSettingsVo _snakeSettings;

        public SnakeSettingsVo Settings => _snakeSettings;
    }
}