using System;
using System.Collections.Generic;
using Enums;
using Models;
using UnityEngine;

namespace Databases.Impls
{
    [CreateAssetMenu(menuName = "Databases/LevelResultDatabase", fileName = "LevelResultDatabase")]
    public class LevelResultDatabase : ScriptableObject, ILevelResultDatabase
    {
        [SerializeField] private LevelResultTypeVo[] _gameResults;
        private Dictionary<ELevelResultType, string> _gameResultsDictionary;
        
        private void OnEnable()
        {
            _gameResultsDictionary = new Dictionary<ELevelResultType, string>();

            foreach (var gameResult in _gameResults) 
                _gameResultsDictionary.Add(gameResult.Type, gameResult.Text);
        }
        
        public string GetLevelResult(ELevelResultType levelResultTypeType)
        {
            try
            {
                return _gameResultsDictionary[levelResultTypeType];
            }
            catch (Exception e)
            {
                throw new Exception(
                    $"[{nameof(LevelResultDatabase)}] Game result" +
                    $" with type {levelResultTypeType.ToString()} was not present in the dictionary. {e.StackTrace}");
            }
        }

    }
}