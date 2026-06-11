using System;
using System.Collections.Generic;
using System.Diagnostics;
using AudioProject.Models;

namespace AudioProject.Algorithms
{

    public class DPCM : ICompressionAlgorithm
    {
        public string Name => "Differential PCM (DPCM)";
        public string Description => "يخزن الفروقات بين العينات بدل القيم الكاملة";

     
        public CompressionResult Compress(short[] samples, CompressionSettings settings)
        {
            var sw = Stopwatch.StartNew();

            var differences = new List<byte>();
            short previous = 0;

            foreach (short sample in samples)
            {
                int diff = sample - previous;

                int clamped = Math.Max(-128, Math.Min(127, diff));
                differences.Add((byte)(clamped + 128));

                previous = sample;
            }

            byte[] compressed = differences.ToArray();
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

        public short[] Decompress(byte[] compressedData, CompressionSettings settings)
        {
            short[] samples = new short[compressedData.Length];
            short previous = 0;

            for (int i = 0; i < compressedData.Length; i++)
            {
                int diff = compressedData[i] - 128;   
                int sample = previous + diff;

              
                samples[i] = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, sample));
                previous = samples[i];
            }

            return samples;
        }
    }
}