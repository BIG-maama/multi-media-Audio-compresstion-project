
namespace AudioProject.Models
{
 
    public class CompressionSettings
    {
        public int SampleRate { get; set; } = 44100;
        public int QuantizationLevels { get; set; } = 256;
        public double StepSize { get; set; } = 1.0;
        public int MuLawParameter { get; set; } = 255;   
        public AlgorithmType Algorithm { get; set; } = AlgorithmType.DPCM;
    }

    public enum AlgorithmType
    {
        NonlinearQuantization,
        DPCM,
        PredictiveDifferentialCoding,
        DeltaModulation,
        AdaptiveDeltaModulation
    }
}