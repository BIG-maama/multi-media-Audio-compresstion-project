namespace AudioProject.Models
{

    public class AudioFileInfo
    {
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public long FileSizeBytes { get; set; }
        public double DurationSeconds { get; set; }
        public int SampleRate { get; set; }
        public int Channels { get; set; }
        public int BitRate { get; set; }
        public string Encoding { get; set; }
        public int BitsPerSample { get; set; }

 
        public string FileSizeFormatted
        {
            get
            {
                if (FileSizeBytes >= 1_048_576)
                    return $"{FileSizeBytes / 1_048_576.0:F2} MB";
                return $"{FileSizeBytes / 1024.0:F2} KB";
            }
        }

        public string DurationFormatted
        {
            get
            {
                var ts = System.TimeSpan.FromSeconds(DurationSeconds);
                return ts.Hours > 0
                    ? $"{ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}"
                    : $"{ts.Minutes:D2}:{ts.Seconds:D2}";
            }
        }

        public string ChannelsFormatted =>
            Channels == 1 ? "Mono" : Channels == 2 ? "Stereo" : $"{Channels} Channels";

        public string BitRateFormatted => $"{BitRate} kbps";
        public string SampleRateFormatted => $"{SampleRate} Hz";
    }
}