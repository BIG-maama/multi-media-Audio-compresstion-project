using AudioProject.Models;

namespace AudioProject.Algorithms
{

    public interface ICompressionAlgorithm
    {
        string Name { get; }
        string Description { get; }

        CompressionResult Compress(short[] audioSamples, CompressionSettings settings);

       
        short[] Decompress(byte[] compressedData, CompressionSettings settings);
    }
}