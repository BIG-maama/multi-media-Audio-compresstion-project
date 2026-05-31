namespace AudioProject.Algorithms
{
    /// <summary>
    /// نتيجة عملية الضغط الكاملة
    /// </summary>
    public class CompressionResult
    {
        public byte[] CompressedData { get; set; }
        public long OriginalSize { get; set; }
        public long CompressedSize { get; set; }
        public double CompressionRatio { get; set; }
        public double SpaceSavingPercent { get; set; }
        public double ProcessingTime { get; set; }  // بالثواني
        public string AlgorithmUsed { get; set; }

        public void Calculate()
        {
            if (OriginalSize > 0)
            {
                CompressionRatio = (double)OriginalSize / CompressedSize;
                SpaceSavingPercent = (1.0 - (double)CompressedSize / OriginalSize) * 100.0;
            }
        }
    }
}