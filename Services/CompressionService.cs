using System;
using System.Threading;
using System.Threading.Tasks;
using AudioProject.Algorithms;
using AudioProject.Models;

namespace AudioProject.Services
{
    /// <summary>
    /// Manages the full compression/decompression pipeline.
    /// Supports cancellation, real-time progress reporting,
    /// and chunk-based processing for large files.
    /// </summary>
    public class CompressionService
    {
        private readonly AudioService _audioService;
        private CancellationTokenSource _cts;

        // ───── Events ─────
        public event Action<double> ProgressChanged;   // 0.0 → 100.0
        public event Action<double> SpeedUpdated;      // samples/sec
        public event Action<double> RatioUpdated;      // compression ratio so far
        public event Action<CompressionResult> CompressionCompleted;
        public event Action<string> CompressionCancelled;
        public event Action<string> ErrorOccurred;

        public CompressionService(AudioService audioService)
        {
            _audioService = audioService;
        }

        // ───── Compress ─────
        public async Task CompressAsync(string inputPath,
                                        CompressionSettings settings)
        {
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            try
            {
                // 1. Read samples
                short[] samples = await Task.Run(() =>
                    _audioService.ReadSamples(inputPath), token);

                if (token.IsCancellationRequested)
                {
                    CompressionCancelled?.Invoke("Compression cancelled by user.");
                    return;
                }

                // 2. Get algorithm
                var algorithm = AlgorithmFactory.Create(settings.Algorithm);

                // 3. Compress in chunks with progress reporting
                var result = await Task.Run(() =>
                    CompressWithProgress(samples, algorithm, settings, token), token);

                if (result != null)
                    CompressionCompleted?.Invoke(result);
            }
            catch (OperationCanceledException)
            {
                CompressionCancelled?.Invoke("Compression cancelled by user.");
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke($"Compression error: {ex.Message}");
            }
        }

        // ───── Decompress ─────
        public async Task<short[]> DecompressAsync(byte[] compressedData,
                                                    CompressionSettings settings)
        {
            try
            {
                var algorithm = AlgorithmFactory.Create(settings.Algorithm);
                return await Task.Run(() =>
                    algorithm.Decompress(compressedData, settings));
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke($"Decompression error: {ex.Message}");
                return null;
            }
        }

        // ───── Cancel ─────
        public void Cancel() => _cts?.Cancel();

        // ───── Chunk-based processing with progress ─────
        private CompressionResult CompressWithProgress(short[] samples,
                                                       ICompressionAlgorithm algorithm,
                                                       CompressionSettings settings,
                                                       CancellationToken token)
        {
            const int ChunkSize = 4096;
            int totalChunks = (int)Math.Ceiling(samples.Length / (double)ChunkSize);
            var allBytes = new System.Collections.Generic.List<byte>();
            long startTime = System.Diagnostics.Stopwatch.GetTimestamp();
            long originalSize = samples.Length * 2;

            for (int chunk = 0; chunk < totalChunks; chunk++)
            {
                if (token.IsCancellationRequested)
                    throw new OperationCanceledException();

                // Extract chunk
                int start = chunk * ChunkSize;
                int length = Math.Min(ChunkSize, samples.Length - start);
                var chunkArr = new short[length];
                Array.Copy(samples, start, chunkArr, 0, length);

                // Compress chunk
                var chunkResult = algorithm.Compress(chunkArr, settings);
                allBytes.AddRange(chunkResult.CompressedData);

                // Report progress
                double progress = (chunk + 1.0) / totalChunks * 100.0;
                ProgressChanged?.Invoke(progress);

                // Report speed (samples per second)
                double elapsed = (System.Diagnostics.Stopwatch.GetTimestamp() - startTime)
                                 / (double)System.Diagnostics.Stopwatch.Frequency;
                double speed = elapsed > 0 ? ((chunk + 1) * ChunkSize) / elapsed : 0;
                SpeedUpdated?.Invoke(speed);

                // Report live ratio
                double currentRatio = originalSize > 0
                    ? (double)originalSize / allBytes.Count
                    : 1.0;
                RatioUpdated?.Invoke(currentRatio);
            }

            byte[] compressed = allBytes.ToArray();
            double totalElapsed = (System.Diagnostics.Stopwatch.GetTimestamp() - startTime)
                                  / (double)System.Diagnostics.Stopwatch.Frequency;

            var result = new CompressionResult
            {
                CompressedData = compressed,
                OriginalSize = originalSize,
                CompressedSize = compressed.Length,
                ProcessingTime = totalElapsed,
                AlgorithmUsed = algorithm.Name
            };
            result.Calculate();
            return result;
        }
    }
}