using System;
using System.Collections.Generic;
using System.Diagnostics;
using AudioProject.Models;

namespace AudioProject.Algorithms
{
    /// <summary>
    /// Adaptive Delta Modulation
    /// نسخة محسّنة من Delta — تُعدّل حجم الخطوة تلقائياً
    /// </summary>
    public class AdaptiveDeltaModulation : ICompressionAlgorithm
    {
        public string Name => "Adaptive Delta Modulation";
        public string Description => "تُعدّل حجم الخطوة ديناميكياً لتحسين الجودة";

        private const double StepGrowth = 1.5;   // معامل النمو
        private const double StepShrink = 0.5;   // معامل الانكماش
        private const int MinStep = 1;
        private const int MaxStep = 32767;

        // ───── Compress ─────
        public CompressionResult Compress(short[] samples, CompressionSettings settings)
        {
            var sw = Stopwatch.StartNew();
            int step = (int)settings.StepSize;
            int approx = 0;
            bool lastBit = false;

            byte[] compressed = new byte[(samples.Length + 7) / 8];

            for (int i = 0; i < samples.Length; i++)
            {
                int byteIndex = i / 8;
                int bitIndex = i % 8;
                bool currentBit;

                if (samples[i] >= approx)
                {
                    currentBit = true;
                    approx += step;
                }
                else
                {
                    currentBit = false;
                    approx -= step;
                }

                // تعديل الخطوة: إذا تكرر نفس الاتجاه → كبّر، وإلا → صغّر
                step = currentBit == lastBit
                    ? (int)Math.Min(step * StepGrowth, MaxStep)
                    : (int)Math.Max(step * StepShrink, MinStep);

                lastBit = currentBit;
                approx = Math.Max(short.MinValue, Math.Min(short.MaxValue, approx));

                if (currentBit)
                    compressed[byteIndex] |= (byte)(1 << bitIndex);
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
            bool lastBit = false;

            for (int i = 0; i < totalSamples; i++)
            {
                int byteIndex = i / 8;
                int bitIndex = i % 8;
                bool currentBit = (compressedData[byteIndex] & (1 << bitIndex)) != 0;

                approx += currentBit ? step : -step;

                step = currentBit == lastBit
                    ? (int)Math.Min(step * StepGrowth, MaxStep)
                    : (int)Math.Max(step * StepShrink, MinStep);

                lastBit = currentBit;
                approx = Math.Max(short.MinValue, Math.Min(short.MaxValue, approx));
                samples[i] = (short)approx;
            }

            return samples;
        }
    }
}