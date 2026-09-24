namespace App.Application.Figures.Requests
{
    public readonly struct RotateFigureRequest
    {
        public int SlotIndex { get; }

        public RotateFigureRequest(int slotIndex)
        {
            SlotIndex = slotIndex;
        }
    }

    public readonly struct RotateFigureResult
    {
        public bool Success { get; }
        public static RotateFigureResult Succeeded => new(true);
        public static RotateFigureResult Failed => new(false);

        private RotateFigureResult(bool success)
        {
            Success = success;
        }
    }
}