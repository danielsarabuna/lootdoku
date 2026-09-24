using System;
using System.Collections.Generic;

namespace App.Domain.Repositories
{
    [Serializable]
    public class OccupiedCellSaveData
    {
        public int x;
        public int y;
        public int colorIndex;

        public OccupiedCellSaveData()
        {
        }

        public OccupiedCellSaveData(int x, int y, int colorIndex)
        {
            this.x = x;
            this.y = y;
            this.colorIndex = colorIndex;
        }
    }

    [Serializable]
    public class TraySlotSaveData
    {
        public bool isConsumed;
        public int rotationCount;
        public string figureId;

        public TraySlotSaveData()
        {
        }

        public TraySlotSaveData(bool isConsumed, int rotationCount, string figureId)
        {
            this.isConsumed = isConsumed;
            this.rotationCount = rotationCount;
            this.figureId = figureId;
        }
    }

    [Serializable]
    public class GameSaveData
    {
        public int seed;
        public int dealStep;
        public int currentScore;
        public int bestScore;
        public int boardWidth;
        public int boardHeight;
        public bool isActive;
        public List<OccupiedCellSaveData> occupiedCells = new();
        public List<TraySlotSaveData> traySlots = new();

        public GameSaveData()
        {
        }
    }
}