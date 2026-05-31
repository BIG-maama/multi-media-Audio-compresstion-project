using AudioProject.Models;

namespace AudioProject.Algorithms
{
    /// <summary>
    /// الواجهة الأساسية لجميع خوارزميات الضغط
    /// كل خوارزمية يجب أن تنفذ هذه الواجهة
    /// </summary>
    public interface ICompressionAlgorithm
    {
        string Name { get; }
        string Description { get; }

        /// <summary>ضغط البيانات الصوتية</summary>
        CompressionResult Compress(short[] audioSamples, CompressionSettings settings);

        /// <summary>فك ضغط البيانات</summary>
        short[] Decompress(byte[] compressedData, CompressionSettings settings);
    }
}