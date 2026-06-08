using System;
using System.Diagnostics;
using AudioProject.Models;

namespace AudioProject.Algorithms
{

    public class NonlinearQuantization : ICompressionAlgorithm
    {
        public string Name => "Nonlinear Quantization (μ-law)";
        public string Description => "تكميم غير خطي يحافظ على جودة الأصوات الهادئة";

      
        public CompressionResult Compress(short[] samples, CompressionSettings settings)
        {
            var sw = Stopwatch.StartNew();

            byte[] compressed = new byte[samples.Length];

            for (int i = 0; i < samples.Length; i++)
            {
                double normalized = samples[i] / 32768.0;          
                double encoded = ApplyMuLaw(normalized, settings.MuLawParameter);
                compressed[i] = (byte)((encoded + 1.0) / 2.0 * 255);
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

        public short[] Decompress(byte[] compressedData, CompressionSettings settings)
        {
            short[] samples = new short[compressedData.Length];

            for (int i = 0; i < compressedData.Length; i++)
            {
                double encoded = (compressedData[i] / 255.0) * 2.0 - 1.0;
                double decoded = InverseMuLaw(encoded, settings.MuLawParameter);
                samples[i] = (short)(decoded * 32768.0);
            }

            return samples;
        }

       
        private double ApplyMuLaw(double x, int mu)
        {
            return Math.Sign(x) * Math.Log(1 + mu * Math.Abs(x)) / Math.Log(1 + mu);
        }

        private double InverseMuLaw(double y, int mu)
        {
            return Math.Sign(y) * (Math.Pow(1 + mu, Math.Abs(y)) - 1) / mu;
        }
    }
}