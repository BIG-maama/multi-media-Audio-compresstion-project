using System;
using System.Diagnostics;
using AudioProject.Models;

namespace AudioProject.Algorithms
{
    /// <summary>
    /// Predictive Differential Coding
    /// يتنبأ بالعينة التالية ويخزن الخطأ فقط
    /// </summary>
    public class PredictiveDifferentialCoding : ICompressionAlgorithm
    {
        public string Name => "Predictive Differential Coding";
        public string Description => "يتنبأ بالعينة القادمة ويخزن فرق التنبؤ فقط";

        // ───── Compress ─────
        public CompressionResult Compress(short[] samples, CompressionSettings settings)
        {
            var sw = Stopwatch.StartNew();

            byte[] compressed = new byte[samples.Length];
            short prev1 = 0;
            short prev2 = 0;

            for (int i = 0; i < samples.Length; i++)
            {
                // التنبؤ بالعينة = 2 × السابقة − قبل السابقة
                int predicted = (2 * prev1) - prev2;
                int error = samples[i] - predicted;

                // نقيّد الخطأ في حدود byte
                int clamped = Math.Max(-128, Math.Min(127, error));
                compressed[i] = (byte)(clamped + 128);

                prev2 = prev1;
                prev1 = samples[i];
            }

            sw.Stop();

            var result = new CompressionResult
            {
                CompressedData = compressed,
                OriginalSize = samples.Length * 2,
                CompressedSize = compressed.Length,
                ProcessingTime = sw.Elapsed.TotalSeconds,
                AlgorithmUsed = Name
            };
            result.Calculate();
            return result;
        }

        // ───── Decompress ─────
        public short[] Decompress(byte[] compressedData, CompressionSettings settings)
        {
            short[] samples = new short[compressedData.Length];
            short prev1 = 0;
            short prev2 = 0;

            for (int i = 0; i < compressedData.Length; i++)
            {
                int error = compressedData[i] - 128;
                int predicted = (2 * prev1) - prev2;
                int sample = predicted + error;

                samples[i] = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, sample));
                prev2 = prev1;
                prev1 = samples[i];
            }

            return samples;
        }
    }
}