using AudioProject.Algorithms;
using AudioProject.Models;
using System;
using System.Diagnostics;

public class AdaptiveDeltaModulation : ICompressionAlgorithm
{
    public string Name => "Adaptive Delta Modulation";
    public string Description => "تُعدّل حجم الخطوة ديناميكياً لتحسين الجودة";

    private const double StepGrowth = 1.5;
    private const double StepShrink = 0.5;
    private const int MinStep = 16;      // ← لا يجب أن يكون 1، صوت ضعيف جداً
    private const int MaxStep = 8192;    // ← حد أعلى معقول

    public CompressionResult Compress(short[] samples, CompressionSettings settings)
    {
        var sw = Stopwatch.StartNew();
        int step = Math.Max(MinStep, (int)settings.StepSize);
        int approx = 0;          // التقدير الحالي
        bool lastBit = false;

        byte[] compressed = new byte[(samples.Length + 7) / 8];

        for (int i = 0; i < samples.Length; i++)
        {
            int byteIndex = i / 8;
            int bitIndex = i % 8;  // 0..7
            bool currentBit;

            // المقارنة: هل الـ sample الأصلي أعلى من تقديرنا؟
            if (samples[i] >= approx)
            {
                currentBit = true;
                approx += step;     // ارفع التقدير
            }
            else
            {
                currentBit = false;
                approx -= step;     // خفض التقدير
            }

            // تعديل الخطوة adaptively
            if (currentBit == lastBit)
                step = (int)Math.Min(step * StepGrowth, MaxStep);
            else
                step = (int)Math.Max(step * StepShrink, MinStep);

            // تأكد من أن approx في نطاق short
            approx = Math.Max(short.MinValue, Math.Min(short.MaxValue, approx));

            lastBit = currentBit;

            // تخزين البت (LSB first)
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

    public short[] Decompress(byte[] compressedData, CompressionSettings settings)
    {
        int totalSamples = compressedData.Length * 8;
        short[] samples = new short[totalSamples];
        int step = Math.Max(MinStep, (int)settings.StepSize);  // ← نفس القيمة الابتدائية!
        int approx = 0;      // ← يجب أن يبدأ من نفس القيمة كما في Compress!
        bool lastBit = false;

        for (int i = 0; i < totalSamples; i++)
        {
            int byteIndex = i / 8;
            int bitIndex = i % 8;

            // استخراج البت (LSB first - نفس الترتيب)
            bool currentBit = (compressedData[byteIndex] & (1 << bitIndex)) != 0;

            // نفس المنطق تماماً كما في Compress
            if (currentBit)
                approx += step;
            else
                approx -= step;

            // نفس التعديل على الخطوة
            if (currentBit == lastBit)
                step = (int)Math.Min(step * StepGrowth, MaxStep);
            else
                step = (int)Math.Max(step * StepShrink, MinStep);

            approx = Math.Max(short.MinValue, Math.Min(short.MaxValue, approx));
            samples[i] = (short)approx;
            lastBit = currentBit;
        }

        return samples;
    }
}
