using System;
using AudioProject.Models;

namespace AudioProject.Algorithms
{
    /// <summary>
    /// Factory Pattern — returns the correct algorithm instance
    /// based on user selection
    /// </summary>
    public static class AlgorithmFactory
    {
        public static ICompressionAlgorithm Create(AlgorithmType type)
        {
            return type switch
            {
                AlgorithmType.NonlinearQuantization => new NonlinearQuantization(),
                AlgorithmType.DPCM => new DPCM(),
                AlgorithmType.DeltaModulation => new DeltaModulation(),
                AlgorithmType.AdaptiveDeltaModulation => new AdaptiveDeltaModulation(),
                AlgorithmType.PredictiveDifferentialCoding => new PredictiveDifferentialCoding(),
                _ => throw new ArgumentException($"Unknown algorithm type: {type}")
            };
        }

        public static string GetDescription(AlgorithmType type)
        {
            return AlgorithmFactory.Create(type).Description;
        }
    }
}