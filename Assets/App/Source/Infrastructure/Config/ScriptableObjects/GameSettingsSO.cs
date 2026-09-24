using UnityEngine;

namespace App.Infrastructure.Config.ScriptableObjects
{
    [CreateAssetMenu(fileName = "GameSettings", menuName = "Lootdoku/Config/Game Settings")]
    public sealed class GameSettingsSO : ScriptableObject
    {
        [Header("Board Dimensions")] [SerializeField, Range(4, 16)]
        private int _width = 8;

        [SerializeField, Range(4, 8)] private int _height = 8;

        [Header("Score Rules")] [SerializeField, Tooltip("Points awarded for each cleared cell in a completed line.")]
        private int _pointsPerCell = 10;

        public int Width => _width;
        public int Height => _height;
        public int PointsPerCell => _pointsPerCell;
    }
}