using System;
using System.Collections.Generic;

namespace App.Domain.Figures
{
    public static class PredictableRandom
    {
        public static IReadOnlyList<Figure> GenerateStepFigures(int masterSeed, int stepIndex,
            IReadOnlyList<Figure> pool, int count = 3)
        {
            if (pool is null || pool.Count == 0)
                throw new InvalidOperationException("Cannot generate figures from an empty pool.");

            var stepSeed = CombineSeed(masterSeed, stepIndex);
            var rng = new Random(stepSeed);
            var result = new List<Figure>(count);

            for (var i = 0; i < count; i++)
            {
                var index = rng.Next(pool.Count);
                var template = pool[index];
                result.Add(new Figure(template.Id, template.Name, template.Shape, RotationAngle.Deg0,
                    template.ColorIndex));
            }

            return result;
        }

        private static int CombineSeed(int masterSeed, int stepIndex) =>
            unchecked((17 * 31 + masterSeed) * 31 + stepIndex);
    }
}