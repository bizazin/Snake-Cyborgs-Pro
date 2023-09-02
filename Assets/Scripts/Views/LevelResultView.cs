using Core.Abstracts;
using UnityEngine;
using UnityEngine.UI;

namespace Views
{
    public class LevelResultView : View
    {
        [SerializeField] private Text _headerText;

        public Text HeaderText => _headerText;
    }
}