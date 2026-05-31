using System;
using System.Diagnostics;
using AudioProject.Models;

namespace AudioProject.Algorithms
{
    /// <summary>
    /// Delta Modulation
    /// يمثّل كل تغيير بـ bit واحد فقط (صعود = 1 / هبوط = 0)
    /// </summary>
    public class DeltaModulation : ICompressionAlgorithm
    {
        public string Name => "Delta Modulation";
        public string Description => "يمثّل التغيير بـ bit واحد فقط، أعلى نسبة ضغط";

        // ───── Compress ─────
        public CompressionResult Compress(short[] samples, CompressionSettings settings)
        {
            var sw = Stopwatch.StartNew();
            int step = (int)settings.StepSize;
            int approx = 0;

            // كل 8 عينات = byte واحد
            byte[] compressed = new byte[(samples.Length + 7) / 8];

            for (int i = 0; i < samples.Length; i++)
            {
                int byteIndex = i / 8;
                int bitIndex = i % 8;

                if (samples[i] >= approx)
                {
                    compressed[byteIndex] |= (byte)(1 << bitIndex); // bit = 1 → صعود
                    approx += step;
                }
                else
                {
                    approx -= step; // bit = 0 → هبوط
                }

                // نمنع الـ overflow
                approx = Math.Max(short.MinValue, Math.Min(short.MaxValue, approx));
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
            int totalSamples = compressedData.Length * 8;
            short[] samples = new short[totalSamples];
            int step = (int)settings.StepSize;
            int approx = 0;

            for (int i = 0; i < totalSamples; i++)
            {
                int byteIndex = i / 8;
                int bitIndex = i % 8;
                bool isUp = (compressedData[byteIndex] & (1 << bitIndex)) != 0;

                approx += isUp ? step : -step;
                approx = Math.Max(short.MinValue, Math.Min(short.MaxValue, approx));
                samples[i] = (short)approx;
            }

            return samples;
        }
    }
}