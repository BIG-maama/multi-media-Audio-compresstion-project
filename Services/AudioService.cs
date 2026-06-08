using System;
using System.IO;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using AudioProject.Models;
namespace AudioProject.Services
{
 
    public class AudioService : IDisposable
    {
   
        private WaveOutEvent _waveOut;
        private AudioFileReader _audioReader;
        private bool _disposed = false;

        public bool IsPlaying => _waveOut?.PlaybackState == PlaybackState.Playing;
        public bool IsPaused => _waveOut?.PlaybackState == PlaybackState.Paused;

        public event EventHandler PlaybackStopped;

   
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

        public short[] ReadSamples(string filePath)
        {
            using var reader = new AudioFileReader(filePath);

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

        
        public void Play(string filePath)
        {
            Stop(); 

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

       
        public void SeekTo(double positionRatio)
        {
            if (_audioReader == null) return;
            _audioReader.Position = (long)(_audioReader.Length * positionRatio);
        }

        
        public double GetPlaybackPosition()
        {
            if (_audioReader == null || _audioReader.Length == 0) return 0;
            return (double)_audioReader.Position / _audioReader.Length;
        }

        public void SaveAsWav(short[] samples, string outputPath, int sampleRate, int channels = 1)
        {

            Stop(); 

            var format = new WaveFormat(sampleRate, 16, channels);

            using var writer = new WaveFileWriter(outputPath, format);
            writer.WriteSamples(samples, 0, samples.Length);
        }

        
        public void Dispose()
        {
            if (_disposed) return;
            Stop();
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }

 
    internal static class AudioExtensions
    {
        public static ISampleProvider ToMono(this AudioFileReader reader)
        {
            if (reader.WaveFormat.Channels == 1) return reader;
            return new StereoToMonoSampleProvider(reader);
        }
    }
}