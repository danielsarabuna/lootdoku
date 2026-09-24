namespace App.Domain.Figures
{
    public enum RotationAngle
    {
        Deg0 = 0,
        Deg90 = 90,
        Deg180 = 180,
        Deg270 = 270
    }

    public static class RotationAngleExtensions
    {
        public static RotationAngle NextClockwise(this RotationAngle current) => current switch
        {
            RotationAngle.Deg0 => RotationAngle.Deg90,
            RotationAngle.Deg90 => RotationAngle.Deg180,
            RotationAngle.Deg180 => RotationAngle.Deg270,
            _ => RotationAngle.Deg0
        };
    }
}
