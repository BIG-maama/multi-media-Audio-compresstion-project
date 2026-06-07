using System;
using System.IO;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using AudioProject.Models;
//namespace AudioProject.Services;
namespace AudioProject.Services
{
    /// <summary>
    /// Handles all audio file operations:
    /// reading, property extraction, playback, and saving
    /// </summary>
    public class AudioService : IDisposable
    {
        // ───── Playback state ─────
        private WaveOutEvent _waveOut;
        private AudioFileReader _audioReader;
        private bool _disposed = false;

        public bool IsPlaying => _waveOut?.PlaybackState == PlaybackState.Playing;
        public bool IsPaused => _waveOut?.PlaybackState == PlaybackState.Paused;

        public event EventHandler PlaybackStopped;

        // ───── Read file & extract properties ─────
        /// <summary>
        /// Reads an audio file and returns all its properties
        /// Supports: WAV, MP3, AIFF, WMA
        /// </summary>
        public AudioFileInfo GetAudioFileInfo(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Audio file not found.", filePath);

            using var reader = new AudioFileReader(filePath);
            var fileInfo = new FileInfo(filePath);
            var waveFormat = reader.WaveFormat;

            return new AudioFileInfo
            {
                FilePath = filePath,
                FileName = Path.GetFileName(filePath),
                FileSizeBytes = fileInfo.Length,
                DurationSeconds = reader.TotalTime.TotalSeconds,
                SampleRate = waveFormat.SampleRate,
                Channels = waveFormat.Channels,
                BitsPerSample = waveFormat.BitsPerSample,
                BitRate = (waveFormat.AverageBytesPerSecond * 8) / 1000,
                Encoding = waveFormat.Encoding.ToString()
            };
        }

        // ───── Read raw samples for compression ─────
        /// <summary>
        /// Reads the audio file and returns raw PCM samples as short[]
        /// Used by compression algorithms
        /// </summary>
        public short[] ReadSamples(string filePath)
        {
            using var reader = new AudioFileReader(filePath);

            // Convert to 16-bit PCM mono for processing
            var monoProvider = reader.ToMono();
            var sampleProvider = new SampleToWaveProvider16(monoProvider);

            using var ms = new MemoryStream();
            byte[] buffer = new byte[4096];
            int bytesRead;

            while ((bytesRead = sampleProvider.Read(buffer, 0, buffer.Length)) > 0)
                ms.Write(buffer, 0, bytesRead);

            byte[] rawBytes = ms.ToArray();
            short[] samples = new short[rawBytes.Length / 2];
            Buffer.BlockCopy(rawBytes, 0, samples, 0, rawBytes.Length);

            return samples;
        }

        // ───── Playback controls ─────
        public void Play(string filePath)
        {
            Stop(); // Stop any current playback first

            _audioReader = new AudioFileReader(filePath);
            _waveOut = new WaveOutEvent();

            _waveOut.Init(_audioReader);
            _waveOut.PlaybackStopped += (s, e) => PlaybackStopped?.Invoke(this, EventArgs.Empty);
            _waveOut.Play();
        }

        public void Pause()
        {
            if (IsPlaying) _waveOut?.Pause();
        }

        public void Resume()
        {
            if (IsPaused) _waveOut?.Play();
        }

        public void Stop()
        {
            _waveOut?.Stop();
            _waveOut?.Dispose();
            _audioReader?.Dispose();
            _waveOut = null;
            _audioReader = null;
        }

        /// <summary>
        /// Seek to a specific position (0.0 to 1.0)
        /// </summary>
        public void SeekTo(double positionRatio)
        {
            if (_audioReader == null) return;
            _audioReader.Position = (long)(_audioReader.Length * positionRatio);
        }

        /// <summary>
        /// Returns current playback position as ratio (0.0 to 1.0)
        /// </summary>
        public double GetPlaybackPosition()
        {
            if (_audioReader == null || _audioReader.Length == 0) return 0;
            return (double)_audioReader.Position / _audioReader.Length;
        }

        // ───── Save compressed audio ─────
        /// <summary>
        /// Saves decompressed PCM samples back to WAV file
        /// </summary>
        public void SaveAsWav(short[] samples, string outputPath, int sampleRate, int channels = 1)
        {

            Stop(); // أضف هذا السطر   

            var format = new WaveFormat(sampleRate, 16, channels);

            using var writer = new WaveFileWriter(outputPath, format);
            writer.WriteSamples(samples, 0, samples.Length);
        }

        // ───── Dispose ─────
        public void Dispose()
        {
            if (_disposed) return;
            Stop();
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }

    // Extension helper — converts stereo to mono
    internal static class AudioExtensions
    {
        public static ISampleProvider ToMono(this AudioFileReader reader)
        {
            if (reader.WaveFormat.Channels == 1) return reader;
            return new StereoToMonoSampleProvider(reader);
        }
    }
}