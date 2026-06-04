//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Threading.Tasks;
//using System.Drawing;
//using System.Drawing.Drawing2D;
//using System.Windows.Forms;

//// Fix ambiguity between ScottPlot and System.Drawing/WinForms
//using Color = System.Drawing.Color;
//using Font = System.Drawing.Font;
//using Label = System.Windows.Forms.Label;
//using Timer = System.Windows.Forms.Timer;

//using AudioProject.Algorithms;
//using AudioProject.Models;
//using AudioProject.Services;

//// لا تضيف: using ScottPlot; ← هاد هو سبب المشكلة
//namespace AudioProject
//{
//    public partial class Form1 : Form
//    {
//        // ──────────────────────────────────────────
//        //  Services & State
//        // ──────────────────────────────────────────
//        private readonly AudioService _audioService;
//        private readonly CompressionService _compressionService;

//        private string _currentFilePath;
//        private AudioFileInfo _audioInfo;
//        private CompressionResult _lastResult;
//        private System.Windows.Forms.Timer _playbackTimer;

//        private readonly List<double> _ratioHistory = new List<double>();
//        private readonly List<double> _speedHistory = new List<double>();

//        // ──────────────────────────────────────────
//        //  Theme Colors
//        // ──────────────────────────────────────────
//        private static readonly Color C_BG = Color.FromArgb(13, 17, 23);
//        private static readonly Color C_SURFACE = Color.FromArgb(22, 27, 34);
//        private static readonly Color C_CARD = Color.FromArgb(30, 36, 45);
//        private static readonly Color C_BORDER = Color.FromArgb(48, 54, 61);
//        private static readonly Color C_ACCENT = Color.FromArgb(88, 166, 255);
//        private static readonly Color C_GREEN = Color.FromArgb(63, 185, 130);
//        private static readonly Color C_ORANGE = Color.FromArgb(255, 166, 87);
//        private static readonly Color C_RED = Color.FromArgb(248, 81, 73);
//        private static readonly Color C_TEXT = Color.FromArgb(230, 237, 243);
//        private static readonly Color C_SUBTEXT = Color.FromArgb(139, 148, 158);
//        private static readonly Font F_TITLE = new("Segoe UI", 13f, FontStyle.Bold);
//        private static readonly Font F_LABEL = new("Segoe UI", 9f, FontStyle.Regular);
//        private static readonly Font F_BOLD = new("Segoe UI", 9f, FontStyle.Bold);
//        private static readonly Font F_MONO = new("Consolas", 9f, FontStyle.Regular);

//        // ──────────────────────────────────────────
//        //  Control References
//        // ──────────────────────────────────────────
//        private Panel _dropZone;
//        private Label _lblDropHint;
//        private Label _lblFileName, _lblFileSize, _lblDuration;
//        private Label _lblSampleRate, _lblChannels, _lblBitRate, _lblEncoding;
//        private Button _btnBrowse, _btnPlay, _btnPause, _btnStop;
//        private TrackBar _trackPlayback;
//        private ComboBox _cmbAlgorithm;
//        private NumericUpDown _nudSampleRate, _nudQuantLevels, _nudStepSize, _nudMuLaw;
//        private Button _btnCompress, _btnDecompress, _btnCancel, _btnSave, _btnReset;
//        private ProgressBar _progressBar;
//        private Label _lblProgressPct, _lblSpeed, _lblRatio, _lblStatus;
//        private ScottPlot.WinForms.FormsPlot _chartRatio, _chartSpeed;
//        private RichTextBox _rtbReport;
//       // private Panel _panelFileInfo, _panelPlayback, _panelSettings;
//        //private Panel _panelProgress, _panelReport;

//        // ══════════════════════════════════════════
//        //  Constructor
//        // ══════════════════════════════════════════
//        public Form1()
//        {
//            InitializeComponent();
//            _audioService = new AudioService();
//            _compressionService = new CompressionService(_audioService);
//            ConfigureForm();
//            BuildUI();
//            WireServiceEvents();
//            SetupDragDrop();
//            SetupPlaybackTimer();
//            UpdateControlStates();
//        }

//        // ──────────────────────────────────────────
//        //  Form Config
//        // ──────────────────────────────────────────
//        private void ConfigureForm()
//        {
//            Text = "Audio Compressor — Damascus University";
//            Size = new Size(1280, 820);
//            MinimumSize = new Size(1100, 750);
//            BackColor = C_BG;
//            ForeColor = C_TEXT;
//            Font = F_LABEL;
//            StartPosition = FormStartPosition.CenterScreen;
//            FormClosing += (s, e) => { _audioService?.Dispose(); };
//        }

//        // ══════════════════════════════════════════
//        //  UI BUILDER
//        // ══════════════════════════════════════════
//        private void BuildUI()
//        {
//            // ── Title Bar ──
//            var titleBar = MakePanel(C_SURFACE, new Rectangle(0, 0, Width, 56));
//            titleBar.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;

//            var lblTitle = MakeLabel("🎵  Audio Compressor", F_TITLE, C_ACCENT,
//                new Point(20, 14), titleBar);
//            var lblSub = MakeLabel("Multimedia Systems Project — 2026", F_LABEL, C_SUBTEXT,
//                new Point(22, 36), titleBar);
//            lblSub.Font = new Font("Segoe UI", 8f);

//            _lblStatus = MakeLabel("Ready. Drop an audio file or click Browse.", F_LABEL,
//                C_GREEN, new Point(0, 20), titleBar);
//            _lblStatus.AutoSize = false;
//            _lblStatus.TextAlign = ContentAlignment.MiddleRight;
//            _lblStatus.Size = new Size(Width - 30, 20);
//            _lblStatus.Anchor = AnchorStyles.Top | AnchorStyles.Right;

//            Controls.Add(titleBar);

//            // ── Main Layout ──
//            var layout = new TableLayoutPanel
//            {
//                Location = new Point(10, 64),
//                Size = new Size(Width - 20, Height - 80),
//                Anchor = AnchorStyles.Top | AnchorStyles.Bottom |
//                              AnchorStyles.Left | AnchorStyles.Right,
//                BackColor = Color.Transparent,
//                ColumnCount = 3,
//                RowCount = 1,
//                CellBorderStyle = TableLayoutPanelCellBorderStyle.None,
//            };
//            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28f));
//            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42f));
//            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30f));
//            Controls.Add(layout);

//            layout.Controls.Add(BuildLeftPanel(), 0, 0);
//            layout.Controls.Add(BuildCenterPanel(), 1, 0);
//            layout.Controls.Add(BuildRightPanel(), 2, 0);
//        }

//        // ──────────────────────────────────────────
//        //  LEFT PANEL: Drop Zone + File Info + Playback
//        // ──────────────────────────────────────────
//        private Panel BuildLeftPanel()
//        {
//            var root = new Panel
//            {
//                Dock = DockStyle.Fill,
//                BackColor = Color.Transparent,
//                Padding = new Padding(0, 0, 6, 0)
//            };

//            var flow = new FlowLayoutPanel
//            {
//                Dock = DockStyle.Fill,
//                FlowDirection = FlowDirection.TopDown,
//                WrapContents = false,
//                AutoScroll = false,
//                BackColor = Color.Transparent,
//            };
//            root.Controls.Add(flow);

//            // ── Drop Zone ──
//            _dropZone = new Panel
//            {
//                Width = 320,
//                Height = 130,
//                BackColor = C_CARD,
//                Cursor = Cursors.Hand,
//                Margin = new Padding(0, 0, 0, 8),
//            };
//            PaintRoundedBorder(_dropZone, C_ACCENT, 2, 12);

//            _lblDropHint = new Label
//            {
//                Text = "⬇️  Drop audio file here\nor click Browse",
//                TextAlign = ContentAlignment.MiddleCenter,
//                Dock = DockStyle.Fill,
//                ForeColor = C_SUBTEXT,
//                Font = new Font("Segoe UI", 10f),
//            };
//            _dropZone.Controls.Add(_lblDropHint);
//            _dropZone.Click += (s, e) => BrowseFile();
//            _dropZone.MouseEnter += (s, e) => _dropZone.BackColor = Color.FromArgb(38, 44, 55);
//            _dropZone.MouseLeave += (s, e) => _dropZone.BackColor = C_CARD;

//            flow.Controls.Add(_dropZone);

//            // ── Browse Button ──
//            _btnBrowse = MakeStyledButton("📂  Browse File", C_ACCENT, 320, 38);
//            _btnBrowse.Margin = new Padding(0, 0, 0, 10);
//            _btnBrowse.Click += (s, e) => BrowseFile();
//            flow.Controls.Add(_btnBrowse);

//            // ── File Info Card ──
//            var infoCard = MakeCard("File Information", 320, 195);
//            infoCard.Margin = new Padding(0, 0, 0, 8);

//            _lblFileName = AddInfoRow(infoCard, "Name", "—", 50);
//            _lblFileSize = AddInfoRow(infoCard, "Size", "—", 72);
//            _lblDuration = AddInfoRow(infoCard, "Duration", "—", 94);
//            _lblSampleRate = AddInfoRow(infoCard, "Sample Rate", "—", 116);
//            _lblChannels = AddInfoRow(infoCard, "Channels", "—", 138);
//            _lblBitRate = AddInfoRow(infoCard, "Bit Rate", "—", 160);
//            _lblEncoding = AddInfoRow(infoCard, "Encoding", "—", 182);

//            flow.Controls.Add(infoCard);

//            // ── Playback Card ──
//            var playCard = MakeCard("Playback", 320, 125);

//            _trackPlayback = new TrackBar
//            {
//                Minimum = 0,
//                Maximum = 100,
//                Location = new Point(10, 44),
//                Width = 296,
//                TickStyle = TickStyle.None,
//                BackColor = C_CARD,
//            };
//            _trackPlayback.Scroll += (s, e) =>
//                _audioService.SeekTo(_trackPlayback.Value / 100.0);
//            playCard.Controls.Add(_trackPlayback);

//            int bx = 14;
//            _btnPlay = MakeIconButton("▶", C_GREEN, new Point(bx, 78), playCard); bx += 58;
//            _btnPause = MakeIconButton("⏸", C_ORANGE, new Point(bx, 78), playCard); bx += 58;
//            _btnStop = MakeIconButton("⏹", C_RED, new Point(bx, 78), playCard);

//            _btnPlay.Click += (s, e) => { _audioService.Play(_currentFilePath); SetStatus("Playing..."); UpdateControlStates(); };
//            _btnPause.Click += (s, e) => { _audioService.Pause(); SetStatus("Paused."); UpdateControlStates(); };
//            _btnStop.Click += (s, e) => { _audioService.Stop(); SetStatus("Stopped."); UpdateControlStates(); };

//            flow.Controls.Add(playCard);

//            return root;
//        }

//        // ──────────────────────────────────────────
//        //  CENTER PANEL: Settings + Progress + Charts
//        // ──────────────────────────────────────────
//        private Panel BuildCenterPanel()
//        {
//            var root = new Panel
//            {
//                Dock = DockStyle.Fill,
//                BackColor = Color.Transparent,
//                Padding = new Padding(3, 0, 3, 0)
//            };

//            var flow = new FlowLayoutPanel
//            {
//                Dock = DockStyle.Fill,
//                FlowDirection = FlowDirection.TopDown,
//                WrapContents = false,
//                BackColor = Color.Transparent,
//                AutoScroll = true,
//            };
//            root.Controls.Add(flow);

//            int panelW = 490;

//            // ── Algorithm Settings Card ──
//            var settingsCard = MakeCard("Compression Settings", panelW, 230);
//            settingsCard.Margin = new Padding(0, 0, 0, 8);

//            MakeLabel("Algorithm:", F_BOLD, C_SUBTEXT, new Point(14, 46), settingsCard);
//            _cmbAlgorithm = new ComboBox
//            {
//                Location = new Point(14, 65),
//                Width = panelW - 28,
//                DropDownStyle = ComboBoxStyle.DropDownList,
//                BackColor = C_SURFACE,
//                ForeColor = C_TEXT,
//                FlatStyle = FlatStyle.Flat,
//            };
//            foreach (AlgorithmType t in Enum.GetValues(typeof(AlgorithmType)))
//                _cmbAlgorithm.Items.Add(t.ToString().Replace("_", " — "));
//            _cmbAlgorithm.SelectedIndex = 1; // DPCM default
//            settingsCard.Controls.Add(_cmbAlgorithm);

//            // Two-column numeric settings
//            _nudSampleRate = AddNumericSetting(settingsCard, "Sample Rate (Hz)",
//                8000, 192000, 44100, new Point(14, 105));
//            _nudQuantLevels = AddNumericSetting(settingsCard, "Quantization Levels",
//                2, 65536, 256, new Point(250, 105));
//            _nudStepSize = AddNumericSetting(settingsCard, "Step Size",
//                1, 32767, 100, new Point(14, 160));
//            _nudMuLaw = AddNumericSetting(settingsCard, "μ-law Parameter",
//                1, 255, 255, new Point(250, 160));

//            flow.Controls.Add(settingsCard);

//            // ── Action Buttons ──
//            var btnCard = MakeCard("Actions", panelW, 74);
//            btnCard.Margin = new Padding(0, 0, 0, 8);

//            int bx2 = 14;
//            _btnCompress = MakeStyledButton("⚙  Compress", C_ACCENT, 108, 34); _btnCompress.Location = new Point(bx2, 30); btnCard.Controls.Add(_btnCompress); bx2 += 116;
//            _btnDecompress = MakeStyledButton("🔓 Decompress", C_GREEN, 108, 34); _btnDecompress.Location = new Point(bx2, 30); btnCard.Controls.Add(_btnDecompress); bx2 += 116;
//            _btnCancel = MakeStyledButton("✖  Cancel", C_RED, 90, 34); _btnCancel.Location = new Point(bx2, 30); btnCard.Controls.Add(_btnCancel); bx2 += 98;
//            _btnReset = MakeStyledButton("↺  Reset", C_SUBTEXT, 80, 34); _btnReset.Location = new Point(bx2, 30); btnCard.Controls.Add(_btnReset);

//            _btnCompress.Click += async (s, e) => await StartCompression();
//            _btnDecompress.Click += async (s, e) => await StartDecompression();
//            _btnCancel.Click += (s, e) => { _compressionService.Cancel(); SetStatus("Cancelled."); };
//            _btnReset.Click += (s, e) => Reset();

//            flow.Controls.Add(btnCard);

//            // ── Progress Card ──
//            var progCard = MakeCard("Progress", panelW, 88);
//            progCard.Margin = new Padding(0, 0, 0, 8);

//            _progressBar = new ProgressBar
//            {
//                Location = new Point(14, 38),
//                Width = panelW - 28,
//                Height = 18,
//                Style = ProgressBarStyle.Continuous,
//                ForeColor = C_ACCENT,
//                BackColor = C_SURFACE,
//            };
//            progCard.Controls.Add(_progressBar);

//            _lblProgressPct = MakeLabel("0%", F_BOLD, C_ACCENT, new Point(14, 62), progCard);
//            _lblSpeed = MakeLabel("Speed: —", F_LABEL, C_SUBTEXT, new Point(100, 62), progCard);
//            _lblRatio = MakeLabel("Ratio: —", F_LABEL, C_SUBTEXT, new Point(270, 62), progCard);

//            flow.Controls.Add(progCard);

//            // ── Charts ──
//            var chartCard = MakeCard("Real-Time Charts", panelW, 280);
//            chartCard.Margin = new Padding(0, 0, 0, 0);

//            _chartRatio = new ScottPlot.WinForms.FormsPlot
//            {
//                Location = new Point(8, 38),
//                Size = new Size((panelW / 2) - 14, 225),
//                BackColor = C_CARD,
//            };
//            StyleChart(_chartRatio, "Compression Ratio", "x");

//            _chartSpeed = new ScottPlot.WinForms.FormsPlot
//            {
//                Location = new Point((panelW / 2) + 4, 38),
//                Size = new Size((panelW / 2) - 14, 225),
//                BackColor = C_CARD,
//            };
//            StyleChart(_chartSpeed, "Processing Speed", "KB/s");

//            chartCard.Controls.Add(_chartRatio);
//            chartCard.Controls.Add(_chartSpeed);
//            flow.Controls.Add(chartCard);

//            return root;
//        }

//        // ──────────────────────────────────────────
//        //  RIGHT PANEL: Report + Save
//        // ──────────────────────────────────────────
//        private Panel BuildRightPanel()
//        {
//            var root = new Panel
//            {
//                Dock = DockStyle.Fill,
//                BackColor = Color.Transparent,
//                Padding = new Padding(6, 0, 0, 0)
//            };

//            var reportCard = MakeCard("Compression Report", 0, 0);
//            reportCard.Dock = DockStyle.Fill;

//            _rtbReport = new RichTextBox
//            {
//                Dock = DockStyle.Fill,
//                BackColor = C_CARD,
//                ForeColor = C_TEXT,
//                Font = F_MONO,
//                BorderStyle = BorderStyle.None,
//                ReadOnly = true,
//                Text = "No report yet.\nRun a compression to see results here.",
//                Margin = new Padding(8),
//            };
//            reportCard.Controls.Add(_rtbReport);

//            _btnSave = MakeStyledButton("💾  Save Compressed File", C_GREEN, 0, 42);
//            _btnSave.Dock = DockStyle.Bottom;
//            _btnSave.Margin = new Padding(0, 4, 0, 0);
//            _btnSave.Click += (s, e) => SaveFile();
//            reportCard.Controls.Add(_btnSave);

//            root.Controls.Add(reportCard);
//            return root;
//        }

//        // ══════════════════════════════════════════
//        //  LOGIC
//        // ══════════════════════════════════════════

//        private void BrowseFile()
//        {
//            using var dlg = new OpenFileDialog
//            {
//                Filter = "Audio Files|*.wav;*.mp3;*.aiff;*.wma|All Files|*.*",
//                Title = "Select Audio File"
//            };
//            if (dlg.ShowDialog() == DialogResult.OK)
//                LoadFile(dlg.FileName);
//        }

//        public void LoadFile(string path)
//        {
//            try
//            {
//                _currentFilePath = path;
//                _audioInfo = _audioService.GetAudioFileInfo(path);
//                _lastResult = null;

//                Invoke((Action)(() =>
//                {
//                    _lblFileName.Text = TruncateText(_audioInfo.FileName, 28);
//                    _lblFileSize.Text = _audioInfo.FileSizeFormatted;
//                    _lblDuration.Text = _audioInfo.DurationFormatted;
//                    _lblSampleRate.Text = _audioInfo.SampleRateFormatted;
//                    _lblChannels.Text = _audioInfo.ChannelsFormatted;
//                    _lblBitRate.Text = _audioInfo.BitRateFormatted;
//                    _lblEncoding.Text = _audioInfo.Encoding;

//                    _lblDropHint.Text = "✅  " + _audioInfo.FileName;
//                    _lblDropHint.ForeColor = C_GREEN;
//                    _dropZone.BackColor = Color.FromArgb(22, 60, 42);

//                    _rtbReport.Text = "File loaded successfully.\nConfigure settings and click Compress.";
//                    _progressBar.Value = 0;
//                    _lblProgressPct.Text = "0%";
//                    _ratioHistory.Clear();
//                    _speedHistory.Clear();
//                    RefreshCharts();
//                    UpdateControlStates();
//                    SetStatus($"Loaded: {_audioInfo.FileName}");
//                }));
//            }
//            catch (Exception ex)
//            {
//                SetStatus($"Error loading file: {ex.Message}");
//            }
//        }

//        private async Task StartCompression()
//        {
//            if (_currentFilePath == null) return;

//            _lastResult = null;
//            _ratioHistory.Clear();
//            _speedHistory.Clear();
//            _progressBar.Value = 0;
//            _lblProgressPct.Text = "0%";
//            SetStatus("Compressing...");
//            UpdateControlStates();

//            var settings = BuildSettings();
//            await _compressionService.CompressAsync(_currentFilePath, settings);
//        }

//        private async Task StartDecompression()
//        {
//            if (_lastResult == null) return;

//            SetStatus("Decompressing...");
//            var settings = BuildSettings();
//            short[] samples = await _compressionService.DecompressAsync(
//                _lastResult.CompressedData, settings);

//            if (samples == null) return;

//            string tmp = Path.Combine(Path.GetTempPath(), "preview_decompressed.wav");
//            _audioService.SaveAsWav(samples, tmp, (int)_nudSampleRate.Value);
//            LoadFile(tmp);
//            SetStatus("Decompression complete. Preview loaded.");
//        }

//        private void SaveFile()
//        {
//            if (_lastResult == null) { SetStatus("Nothing to save yet."); return; }

//            using var dlg = new SaveFileDialog
//            {
//                Filter = "Compressed File|*.cmp|WAV File|*.wav",
//                FileName = "compressed_audio"
//            };
//            if (dlg.ShowDialog() != DialogResult.OK) return;

//            try
//            {
//                if (dlg.FilterIndex == 1)
//                    File.WriteAllBytes(dlg.FileName, _lastResult.CompressedData);
//                else
//                {
//                    var algo = AlgorithmFactory.Create(GetSelectedAlgorithm());
//                    short[] pcm = algo.Decompress(_lastResult.CompressedData, BuildSettings());
//                    _audioService.SaveAsWav(pcm, dlg.FileName, (int)_nudSampleRate.Value);
//                }
//                SetStatus($"Saved: {dlg.FileName}");
//            }
//            catch (Exception ex) { SetStatus($"Save error: {ex.Message}"); }
//        }

//        private void Reset()
//        {
//            _audioService.Stop();
//            _currentFilePath = null;
//            _audioInfo = null;
//            _lastResult = null;

//            _lblFileName.Text = _lblFileSize.Text = _lblDuration.Text = "—";
//            _lblSampleRate.Text = _lblChannels.Text = _lblBitRate.Text = "—";
//            _lblEncoding.Text = "—";

//            _lblDropHint.Text = "⬇️  Drop audio file here\nor click Browse";
//            _lblDropHint.ForeColor = C_SUBTEXT;
//            _dropZone.BackColor = C_CARD;

//            _progressBar.Value = 0;
//            _lblProgressPct.Text = "0%";
//            _lblSpeed.Text = "Speed: —";
//            _lblRatio.Text = "Ratio: —";
//            _rtbReport.Text = "Reset. Load a new audio file.";

//            _ratioHistory.Clear();
//            _speedHistory.Clear();
//            RefreshCharts();
//            UpdateControlStates();
//            SetStatus("Reset complete.");
//        }

//        // ══════════════════════════════════════════
//        //  SERVICE EVENTS
//        // ══════════════════════════════════════════
//        private void WireServiceEvents()
//        {
//            _compressionService.ProgressChanged += p =>
//                Invoke(() =>
//                {
//                    _progressBar.Value = Math.Min(100, (int)p);
//                    _lblProgressPct.Text = $"{p:F1}%";
//                });

//            _compressionService.SpeedUpdated += s =>
//                Invoke(() =>
//                {
//                    _lblSpeed.Text = $"Speed: {s / 1000.0:F1} KB/s";
//                    _speedHistory.Add(s / 1000.0);
//                    RefreshCharts();
//                });

//            _compressionService.RatioUpdated += r =>
//                Invoke(() =>
//                {
//                    _lblRatio.Text = $"Ratio: {r:F2}x";
//                    _ratioHistory.Add(r);
//                    RefreshCharts();
//                });

//            _compressionService.CompressionCompleted += result =>
//                Invoke(() =>
//                {
//                    _lastResult = result;
//                    SetStatus("Compression completed ✅");
//                    UpdateControlStates();
//                    ShowReport(result);
//                });

//            _compressionService.CompressionCancelled += msg =>
//                Invoke(() => { SetStatus(msg); UpdateControlStates(); });

//            _compressionService.ErrorOccurred += msg =>
//                Invoke(() => { SetStatus($"Error: {msg}"); UpdateControlStates(); });

//            _audioService.PlaybackStopped += (s, e) =>
//                Invoke(() => UpdateControlStates());
//        }

//        private void ShowReport(CompressionResult r)
//        {
//            _rtbReport.Clear();
//            AppendColored("=== Compression Report ===\n\n", C_ACCENT, true);
//            AppendRow("Algorithm", r.AlgorithmUsed);
//            AppendRow("Original", $"{r.OriginalSize / 1024.0:F2} KB");
//            AppendRow("Compressed", $"{r.CompressedSize / 1024.0:F2} KB");
//            AppendRow("Ratio", $"{r.CompressionRatio:F2}x");
//            AppendRow("Space Saved", $"{r.SpaceSavingPercent:F1}%");
//            AppendRow("Time", $"{r.ProcessingTime:F3} sec");
//            AppendRow("Sample Rate", $"{_nudSampleRate.Value} Hz");
//            AppendRow("Quant Levels", $"{_nudQuantLevels.Value}");
//            AppendRow("Step Size", $"{_nudStepSize.Value}");
//        }

//        private void AppendColored(string text, Color color, bool bold = false)
//        {
//            _rtbReport.SelectionColor = color;
//            _rtbReport.SelectionFont = bold
//                ? new Font("Consolas", 9f, FontStyle.Bold)
//                : F_MONO;
//            _rtbReport.AppendText(text);
//        }

//        private void AppendRow(string key, string val)
//        {
//            AppendColored($"{key,-14}: ", C_SUBTEXT);
//            AppendColored($"{val}\n", C_TEXT);
//        }

//        // ══════════════════════════════════════════
//        //  DRAG & DROP
//        // ══════════════════════════════════════════
//        private void SetupDragDrop()
//        {
//            AllowDrop = true;
//            _dropZone.AllowDrop = true;

//            _dropZone.DragEnter += (s, e) =>
//            {
//                if (e.Data.GetDataPresent(DataFormats.FileDrop))
//                {
//                    e.Effect = DragDropEffects.Copy;
//                    _dropZone.BackColor = Color.FromArgb(22, 60, 90);
//                }
//            };
//            _dropZone.DragLeave += (s, e) =>
//                _dropZone.BackColor = _audioInfo != null
//                    ? Color.FromArgb(22, 60, 42) : C_CARD;

//            _dropZone.DragDrop += (s, e) =>
//            {
//                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
//                if (files.Length > 0) LoadFile(files[0]);
//            };
//        }

//        // ══════════════════════════════════════════
//        //  PLAYBACK TIMER (progress bar update)
//        // ══════════════════════════════════════════
//        private void SetupPlaybackTimer()
//        {
//            _playbackTimer = new System.Windows.Forms.Timer { Interval = 500 };
//            _playbackTimer.Tick += (s, e) =>
//            {
//                if (!_audioService.IsPlaying) return;
//                _trackPlayback.Value = (int)(_audioService.GetPlaybackPosition() * 100);
//            };
//            _playbackTimer.Start();
//        }

//        // ══════════════════════════════════════════
//        //  CHARTS
//        // ══════════════════════════════════════════
//        private void StyleChart(ScottPlot.WinForms.FormsPlot chart, string title, string yLabel)
//        {
//            // Title & labels — no color parameter in this ScottPlot version
//            chart.Plot.Title(title);
//            chart.Plot.YLabel(yLabel);

//            // Background — new API for ScottPlot 5.x
//            chart.Plot.FigureBackground.Color = ScottPlot.Color.FromHex("#1E2430");
//            chart.Plot.DataBackground.Color = ScottPlot.Color.FromHex("#1E2430");

//            // Axes color
//            chart.Plot.Axes.Color(ScottPlot.Color.FromHex("#8B949E"));

//            chart.Refresh();
//        }

//        private void RefreshCharts()
//        {
//            _chartRatio.Plot.Clear();
//            _chartSpeed.Plot.Clear();

//            if (_ratioHistory.Count > 0)
//            {
//                var sig = _chartRatio.Plot.Add.Signal(_ratioHistory.ToArray());
//                sig.Color = ScottPlot.Color.FromHex("#58A6FF");
//                sig.LineWidth = 2;
//            }

//            if (_speedHistory.Count > 0)
//            {
//                var sig = _chartSpeed.Plot.Add.Signal(_speedHistory.ToArray());
//                sig.Color = ScottPlot.Color.FromHex("#3FB982");
//                sig.LineWidth = 2;
//            }

//            StyleChart(_chartRatio, "Compression Ratio", "x");
//            StyleChart(_chartSpeed, "Processing Speed", "KB/s");

//            _chartRatio.Refresh();
//            _chartSpeed.Refresh();
//        }

//        // ══════════════════════════════════════════
//        //  HELPERS
//        // ══════════════════════════════════════════
//        private CompressionSettings BuildSettings() => new CompressionSettings
//        {
//            Algorithm = GetSelectedAlgorithm(),
//            SampleRate = (int)_nudSampleRate.Value,
//            QuantizationLevels = (int)_nudQuantLevels.Value,
//            StepSize = (double)_nudStepSize.Value,
//            MuLawParameter = (int)_nudMuLaw.Value,
//        };

//        private AlgorithmType GetSelectedAlgorithm() =>
//            (AlgorithmType)_cmbAlgorithm.SelectedIndex;

//        private void UpdateControlStates()
//        {
//            bool hasFile = _currentFilePath != null;
//            bool hasResult = _lastResult != null;

//            _btnPlay.Enabled = hasFile;
//            _btnPause.Enabled = _audioService.IsPlaying;
//            _btnStop.Enabled = _audioService.IsPlaying || _audioService.IsPaused;
//            _btnCompress.Enabled = hasFile;
//            _btnDecompress.Enabled = hasResult;
//            _btnSave.Enabled = hasResult;
//        }

//        private void SetStatus(string msg)
//        {
//            if (InvokeRequired) Invoke(() => _lblStatus.Text = msg);
//            else _lblStatus.Text = msg;
//        }

//        private string TruncateText(string s, int max) =>
//      s.Length > max ? s.Substring(0, max) + "…" : s;

//        // ──────────────────────────────────────────
//        //  UI FACTORY METHODS
//        // ──────────────────────────────────────────
//        private Panel MakePanel(Color bg, Rectangle bounds)
//        {
//            var p = new Panel { BackColor = bg, Bounds = bounds };
//            return p;
//        }

//        private Panel MakeCard(string title, int width, int height)
//        {
//            var card = new Panel
//            {
//                Width = width,
//                Height = height,
//                BackColor = C_CARD,
//            };
//            PaintRoundedBorder(card, C_BORDER, 1, 10);

//            if (!string.IsNullOrEmpty(title))
//            {
//                var lbl = new Label
//                {
//                    Text = title,
//                    ForeColor = C_SUBTEXT,
//                    Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
//                    Location = new Point(12, 10),
//                    AutoSize = true,
//                };
//                card.Controls.Add(lbl);
//            }

//            return card;
//        }

//        private Label MakeLabel(string text, Font font, Color color,
//            Point location, Control parent = null)
//        {
//            var lbl = new Label
//            {
//                Text = text,
//                Font = font,
//                ForeColor = color,
//                Location = location,
//                AutoSize = true,
//                BackColor = Color.Transparent,
//            };
//            parent?.Controls.Add(lbl);
//            return lbl;
//        }

//        private Label AddInfoRow(Panel parent, string key, string value, int y)
//        {
//            MakeLabel(key + ":", F_LABEL, C_SUBTEXT, new Point(14, y), parent);
//            var valLabel = MakeLabel(value, F_BOLD, C_TEXT, new Point(115, y), parent);
//            return valLabel;
//        }

//        private Button MakeStyledButton(string text, Color color, int width, int height)
//        {
//            var btn = new Button
//            {
//                Text = text,
//                Width = width,
//                Height = height,
//                FlatStyle = FlatStyle.Flat,
//                BackColor = Color.FromArgb(30, color.R, color.G, color.B),
//                ForeColor = color,
//                Font = F_BOLD,
//                Cursor = Cursors.Hand,
//            };
//            btn.FlatAppearance.BorderColor = color;
//            btn.FlatAppearance.BorderSize = 1;
//            btn.MouseEnter += (s, e) => btn.BackColor = Color.FromArgb(60, color.R, color.G, color.B);
//            btn.MouseLeave += (s, e) => btn.BackColor = Color.FromArgb(30, color.R, color.G, color.B);
//            return btn;
//        }

//        private Button MakeIconButton(string icon, Color color, Point loc, Panel parent)
//        {
//            var btn = new Button
//            {
//                Text = icon,
//                Size = new Size(50, 34),
//                Location = loc,
//                FlatStyle = FlatStyle.Flat,
//                BackColor = Color.FromArgb(30, color.R, color.G, color.B),
//                ForeColor = color,
//                Font = new Font("Segoe UI", 11f),
//                Cursor = Cursors.Hand,
//            };
//            btn.FlatAppearance.BorderColor = color;
//            btn.FlatAppearance.BorderSize = 1;
//            btn.MouseEnter += (s, e) => btn.BackColor = Color.FromArgb(70, color.R, color.G, color.B);
//            btn.MouseLeave += (s, e) => btn.BackColor = Color.FromArgb(30, color.R, color.G, color.B);
//            parent.Controls.Add(btn);
//            return btn;
//        }

//        private NumericUpDown AddNumericSetting(Panel parent, string label,
//            int min, int max, int val, Point loc)
//        {
//            var lbl = new Label
//            {
//                Text = label,
//                ForeColor = C_SUBTEXT,
//                Font = new Font("Segoe UI", 8f),
//                Location = new Point(loc.X, loc.Y),
//                AutoSize = true,
//                BackColor = Color.Transparent,
//            };

//            var nud = new NumericUpDown
//            {
//                Minimum = min,
//                Maximum = max,
//                Value = val,
//                Location = new Point(loc.X, loc.Y + 17),
//                Width = 210,
//                BackColor = C_SURFACE,
//                ForeColor = C_TEXT,
//                BorderStyle = BorderStyle.FixedSingle,
//                Font = F_LABEL,
//            };

//            parent.Controls.Add(lbl);
//            parent.Controls.Add(nud);
//            return nud;
//        }

//        private void PaintRoundedBorder(Panel panel, Color borderColor, int thickness, int radius)
//        {
//            panel.Paint += (s, e) =>
//            {
//                var g = e.Graphics;
//                g.SmoothingMode = SmoothingMode.AntiAlias;
//                using var pen = new Pen(borderColor, thickness);
//                var rect = new Rectangle(1, 1, panel.Width - 2, panel.Height - 2);
//                using var path = RoundedRect(rect, radius);
//                g.DrawPath(pen, path);
//            };
//        }

//        private static GraphicsPath RoundedRect(Rectangle r, int rad)
//        {
//            int d = rad * 2;
//            var path = new GraphicsPath();
//            path.AddArc(r.X, r.Y, d, d, 180, 90);
//            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
//            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
//            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
//            path.CloseFigure();
//            return path;
//        }
//    }
//}
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

using Color = System.Drawing.Color;
using Font = System.Drawing.Font;
using Label = System.Windows.Forms.Label;
using Timer = System.Windows.Forms.Timer;

using AudioProject.Algorithms;
using AudioProject.Models;
using AudioProject.Services;

namespace AudioProject
{
    public partial class Form1 : Form
    {
        // ══════════════════════════════════════════
        //  SERVICES & STATE
        // ══════════════════════════════════════════
        private readonly AudioService _audioService;
        private readonly CompressionService _compressionService;

        private string _currentFilePath;
        private AudioFileInfo _audioInfo;
        private CompressionResult _lastResult;
        private Timer _playbackTimer;
        private Timer _toastTimer;

        private readonly List<double> _ratioHistory = new List<double>();
        private readonly List<double> _speedHistory = new List<double>();

        // ══════════════════════════════════════════
        //  THEME
        // ══════════════════════════════════════════
        private static readonly Color C_BG = Color.FromArgb(10, 14, 20);
        private static readonly Color C_SURFACE = Color.FromArgb(16, 21, 28);
        private static readonly Color C_CARD = Color.FromArgb(22, 28, 38);
        private static readonly Color C_CARD2 = Color.FromArgb(28, 35, 46);
        private static readonly Color C_BORDER = Color.FromArgb(44, 52, 64);
        private static readonly Color C_ACCENT = Color.FromArgb(82, 162, 255);
        private static readonly Color C_GREEN = Color.FromArgb(56, 193, 114);
        private static readonly Color C_ORANGE = Color.FromArgb(255, 158, 68);
        private static readonly Color C_RED = Color.FromArgb(240, 72, 72);
        private static readonly Color C_PURPLE = Color.FromArgb(147, 112, 219);
        private static readonly Color C_TEXT = Color.FromArgb(220, 228, 240);
        private static readonly Color C_SUBTEXT = Color.FromArgb(120, 132, 150);
        private static readonly Color C_DIVIDER = Color.FromArgb(36, 44, 56);

        private static readonly Font F_TITLE = new Font("Segoe UI Semibold", 14f, FontStyle.Bold);
        private static readonly Font F_SECTION = new Font("Segoe UI", 8.5f, FontStyle.Bold);
        private static readonly Font F_LABEL = new Font("Segoe UI", 8.5f, FontStyle.Regular);
        private static readonly Font F_BOLD = new Font("Segoe UI", 8.5f, FontStyle.Bold);
        private static readonly Font F_MONO = new Font("Consolas", 9f, FontStyle.Regular);
        private static readonly Font F_MONO_B = new Font("Consolas", 9f, FontStyle.Bold);
        private static readonly Font F_SMALL = new Font("Segoe UI", 7.5f, FontStyle.Regular);

        // ══════════════════════════════════════════
        //  CONTROL REFS
        // ══════════════════════════════════════════
        private Panel _dropZone;
        private Label _lblDropHint;
        private Label _lblFileName, _lblFileSize, _lblDuration;
        private Label _lblSampleRate, _lblChannels, _lblBitRate, _lblEncoding;
        private Button _btnBrowse, _btnPlay, _btnPause, _btnStop;
        private TrackBar _trackPlayback;
        private Label _lblPlayTime;
        private ComboBox _cmbAlgorithm;
        private NumericUpDown _nudSampleRate, _nudQuantLevels, _nudStepSize, _nudMuLaw;
        private Button _btnCompress, _btnDecompress, _btnCancel, _btnSave, _btnReset;
        private ProgressBar _progressBar;
        private Label _lblProgressPct, _lblSpeed, _lblRatio, _lblStatus;
        private ScottPlot.WinForms.FormsPlot _chartRatio, _chartSpeed;
        private RichTextBox _rtbReport;
        private Panel _toastPanel;
        private Label _lblToast;
        private Label _lblAlgoDesc;

        // ══════════════════════════════════════════
        //  CONSTRUCTOR
        // ══════════════════════════════════════════
        public Form1()
        {
            InitializeComponent();
            _audioService = new AudioService();
            _compressionService = new CompressionService(_audioService);
            ConfigureForm();
            BuildUI();
            WireServiceEvents();
            SetupDragDrop();
            SetupTimers();
            UpdateControlStates();
        }

        // ──────────────────────────────────────────
        private void ConfigureForm()
        {
            Text = "Audio Compressor  |  Damascus University — 2026";
            Size = new Size(1340, 840);
            MinimumSize = new Size(1180, 780);
            BackColor = C_BG;
            ForeColor = C_TEXT;
            Font = F_LABEL;
            StartPosition = FormStartPosition.CenterScreen;
            DoubleBuffered = true;
            FormClosing += (s, e) => _audioService?.Dispose();
        }

        // ══════════════════════════════════════════
        //  BUILD UI — TOP LEVEL
        // ══════════════════════════════════════════
        private void BuildUI()
        {
            BuildHeader();
            BuildToast();

            // 3-column splitter
            var outer = new TableLayoutPanel
            {
                Left = 0,
                Top = 58,
                Width = Width,
                Height = Height - 58,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.Transparent,
                ColumnCount = 3,
                RowCount = 1,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None,
                Padding = new Padding(10, 8, 10, 10),
            };
            outer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 300f));
            outer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            outer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 310f));
            Controls.Add(outer);

            outer.Controls.Add(BuildLeftPanel(), 0, 0);
            outer.Controls.Add(BuildCenterPanel(), 1, 0);
            outer.Controls.Add(BuildRightPanel(), 2, 0);
        }

        // ──────────────────────────────────────────
        //  HEADER
        // ──────────────────────────────────────────
        private void BuildHeader()
        {
            var bar = new Panel
            {
                Left = 0,
                Top = 0,
                Width = Width,
                Height = 58,
                BackColor = C_SURFACE,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            };

            // accent left strip
            var strip = new Panel { Left = 0, Top = 0, Width = 4, Height = 58, BackColor = C_ACCENT };
            bar.Controls.Add(strip);

            var icon = new Label
            {
                Text = "◈",
                Font = new Font("Segoe UI", 18f, FontStyle.Bold),
                ForeColor = C_ACCENT,
                Location = new Point(16, 10),
                AutoSize = true,
                BackColor = Color.Transparent,
            };
            bar.Controls.Add(icon);

            var title = new Label
            {
                Text = "Audio Compressor",
                Font = F_TITLE,
                ForeColor = C_TEXT,
                Location = new Point(46, 8),
                AutoSize = true,
                BackColor = Color.Transparent,
            };
            bar.Controls.Add(title);

            var sub = new Label
            {
                Text = "Multimedia Systems · Damascus University · 2026",
                Font = F_SMALL,
                ForeColor = C_SUBTEXT,
                Location = new Point(48, 32),
                AutoSize = true,
                BackColor = Color.Transparent,
            };
            bar.Controls.Add(sub);

            _lblStatus = new Label
            {
                Text = "●  Ready — drop an audio file or click Browse",
                Font = F_SMALL,
                ForeColor = C_GREEN,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleRight,
                Size = new Size(560, 58),
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
            };
            _lblStatus.Left = bar.Width - _lblStatus.Width - 14;
            bar.Controls.Add(_lblStatus);

            Controls.Add(bar);
            bar.BringToFront();

            // divider line
            var line = new Panel { Left = 0, Top = 57, Width = Width, Height = 1, BackColor = C_BORDER, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            Controls.Add(line);
        }

        // ──────────────────────────────────────────
        //  TOAST NOTIFICATION
        // ──────────────────────────────────────────
        private void BuildToast()
        {
            _toastPanel = new Panel
            {
                Size = new Size(360, 48),
                BackColor = C_CARD2,
                Visible = false,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
            };
            PositionToast();
            DrawBorder(_toastPanel, C_BORDER, 1, 8);

            _lblToast = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = F_BOLD,
                ForeColor = C_TEXT,
                BackColor = Color.Transparent,
                Padding = new Padding(14, 0, 14, 0),
            };
            _toastPanel.Controls.Add(_lblToast);
            Controls.Add(_toastPanel);
            _toastPanel.BringToFront();

            _toastTimer = new Timer { Interval = 3200 };
            _toastTimer.Tick += (s, e) =>
            {
                _toastTimer.Stop();
                _toastPanel.Visible = false;
            };

            Resize += (s, e) => PositionToast();
        }

        private void PositionToast()
        {
            if (_toastPanel == null) return;
            _toastPanel.Location = new Point(Width - 380, Height - 100);
        }

        public void ShowToast(string msg, ToastType type = ToastType.Info)
        {
            if (InvokeRequired) { Invoke((Action)(() => ShowToast(msg, type))); return; }
            Color col;
            string icon;
            switch (type)
            {
                case ToastType.Success: col = C_GREEN; icon = "✔  "; break;
                case ToastType.Error: col = C_RED; icon = "✖  "; break;
                case ToastType.Warning: col = C_ORANGE; icon = "⚠  "; break;
                default: col = C_ACCENT; icon = "ℹ  "; break;
            }
            _lblToast.ForeColor = col;
            _lblToast.Text = icon + msg;
            DrawBorder(_toastPanel, col, 1, 8);
            _toastPanel.Visible = true;
            _toastPanel.BringToFront();
            _toastTimer.Stop();
            _toastTimer.Start();
        }

        public enum ToastType { Info, Success, Error, Warning }

        // ══════════════════════════════════════════
        //  LEFT PANEL
        // ══════════════════════════════════════════
        private Panel BuildLeftPanel()
        {
            var root = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Padding = new Padding(0, 0, 6, 0) };

            // Drop Zone
            _dropZone = new Panel
            {
                Left = 0,
                Top = 0,
                Width = 285,
                Height = 118,
                BackColor = C_CARD,
                Cursor = Cursors.Hand,
            };
            DrawBorder(_dropZone, C_ACCENT, 2, 10);
            _lblDropHint = new Label
            {
                Text = "⬇  Drop audio file here\nor click Browse",
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                ForeColor = C_SUBTEXT,
                Font = new Font("Segoe UI", 9.5f),
                BackColor = Color.Transparent,
            };
            _dropZone.Controls.Add(_lblDropHint);
            _dropZone.Click += (s, e) => BrowseFile();
            _dropZone.MouseEnter += (s, e) => _dropZone.BackColor = Color.FromArgb(32, 40, 52);
            _dropZone.MouseLeave += (s, e) => _dropZone.BackColor = C_CARD;
            root.Controls.Add(_dropZone);

            // Browse
            _btnBrowse = MakeBtn("📂  Browse File", C_ACCENT, 285, 34);
            _btnBrowse.Left = 0; _btnBrowse.Top = 126;
            _btnBrowse.Click += (s, e) => BrowseFile();
            root.Controls.Add(_btnBrowse);

            // File Info
            var infoCard = SectionCard("File Information", 0, 170, 285, 200);
            root.Controls.Add(infoCard);
            _lblFileName = InfoRow(infoCard, "Name", "—", 34);
            _lblFileSize = InfoRow(infoCard, "Size", "—", 54);
            _lblDuration = InfoRow(infoCard, "Duration", "—", 74);
            _lblSampleRate = InfoRow(infoCard, "Sample Rate", "—", 94);
            _lblChannels = InfoRow(infoCard, "Channels", "—", 114);
            _lblBitRate = InfoRow(infoCard, "Bit Rate", "—", 134);
            _lblEncoding = InfoRow(infoCard, "Encoding", "—", 154);

            // Playback
            var playCard = SectionCard("Playback", 0, 380, 285, 122);
            root.Controls.Add(playCard);

            _trackPlayback = new TrackBar
            {
                Left = 8,
                Top = 32,
                Width = 266,
                Minimum = 0,
                Maximum = 100,
                TickStyle = TickStyle.None,
                BackColor = C_CARD,
            };
            _trackPlayback.Scroll += (s, e) => _audioService.SeekTo(_trackPlayback.Value / 100.0);
            playCard.Controls.Add(_trackPlayback);

            _lblPlayTime = new Label
            {
                Text = "00:00 / 00:00",
                Font = F_SMALL,
                ForeColor = C_SUBTEXT,
                Location = new Point(10, 62),
                AutoSize = true,
                BackColor = Color.Transparent,
            };
            playCard.Controls.Add(_lblPlayTime);

            int bx = 8;
            _btnPlay = MakeIconBtn("▶", C_GREEN, new Point(bx, 82), playCard); bx += 56;
            _btnPause = MakeIconBtn("⏸", C_ORANGE, new Point(bx, 82), playCard); bx += 56;
            _btnStop = MakeIconBtn("⏹", C_RED, new Point(bx, 82), playCard);

            _btnPlay.Click += (s, e) => { _audioService.Play(_currentFilePath); SetStatus("▶  Playing..."); UpdateControlStates(); };
            _btnPause.Click += (s, e) =>
            {
                if (_audioService.IsPlaying)
                {
                    _audioService.Pause();
                    _btnPause.Text = "▶️▶️";   // يتغير الأيقونة ليشير للـ Resume
                    _btnPause.ForeColor = C_GREEN;
                    SetStatus("⏸️  Paused.");
                }
                else if (_audioService.IsPaused)
                {
                    _audioService.Resume();
                    _btnPause.Text = "⏸️";
                    _btnPause.ForeColor = C_ORANGE;
                    SetStatus("▶️  Resumed.");
                }
                UpdateControlStates();
            };

            _btnStop.Click += (s, e) =>
            {
                _audioService.Stop();
                _btnPause.Text = "⏸️";       // ← reset الأيقونة
                _btnPause.ForeColor = C_ORANGE;
                SetStatus("⏹️  Stopped.");
                UpdateControlStates();
            };


            return root;
        }

        // ══════════════════════════════════════════
        //  CENTER PANEL
        // ══════════════════════════════════════════
        private Panel BuildCenterPanel()
        {
            var root = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Padding = new Padding(4, 0, 4, 0) };

            // ── Settings ──
            var settCard = SectionCard("Compression Settings", 0, 0, 0, 236);
            settCard.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            settCard.Left = 0; settCard.Width = root.Width;
            root.Controls.Add(settCard);
            root.Resize += (s, e) => settCard.Width = root.Width;

            var algLabel = new Label { Text = "Algorithm", Font = F_SMALL, ForeColor = C_SUBTEXT, Location = new Point(12, 30), AutoSize = true, BackColor = Color.Transparent };
            settCard.Controls.Add(algLabel);

            _cmbAlgorithm = new ComboBox
            {
                Location = new Point(12, 46),
                Width = settCard.Width - 24,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = C_SURFACE,
                ForeColor = C_TEXT,
                FlatStyle = FlatStyle.Flat,
                Font = F_BOLD,
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top,
            };
            foreach (AlgorithmType t in Enum.GetValues(typeof(AlgorithmType)))
                _cmbAlgorithm.Items.Add(AlgoDisplayName(t));
            _cmbAlgorithm.SelectedIndex = 1;
            _cmbAlgorithm.SelectedIndexChanged += (s, e) => UpdateAlgoDesc();
            settCard.Controls.Add(_cmbAlgorithm);

            _lblAlgoDesc = new Label
            {
                Location = new Point(12, 74),
                Size = new Size(settCard.Width - 24, 28),
                Font = F_SMALL,
                ForeColor = C_SUBTEXT,
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top,
            };
            settCard.Controls.Add(_lblAlgoDesc);
            UpdateAlgoDesc();

            // 2×2 numeric grid
            _nudSampleRate = NudSetting(settCard, "Sample Rate (Hz)", 8000, 192000, 44100, new Point(12, 112));
            _nudQuantLevels = NudSetting(settCard, "Quantization Levels", 2, 65536, 256, new Point(245, 112));
            _nudStepSize = NudSetting(settCard, "Step Size", 1, 32767, 100, new Point(12, 166));
            _nudMuLaw = NudSetting(settCard, "μ-law Parameter", 1, 255, 255, new Point(245, 166));

            // ── Action Buttons ──
            var actCard = SectionCard("Actions", 0, 244, 0, 68);
            actCard.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            actCard.Width = root.Width;
            root.Controls.Add(actCard);
            root.Resize += (s, e) => actCard.Width = root.Width;

            _btnCompress = MakeBtn("⚙  Compress", C_ACCENT, 116, 32); _btnCompress.Location = new Point(12, 26); actCard.Controls.Add(_btnCompress);
            _btnDecompress = MakeBtn("🔓 Decompress", C_GREEN, 116, 32); _btnDecompress.Location = new Point(136, 26); actCard.Controls.Add(_btnDecompress);
            _btnCancel = MakeBtn("✖  Cancel", C_RED, 90, 32); _btnCancel.Location = new Point(260, 26); actCard.Controls.Add(_btnCancel);
            _btnReset = MakeBtn("↺  Reset", C_SUBTEXT, 80, 32); _btnReset.Location = new Point(358, 26); actCard.Controls.Add(_btnReset);

            _btnCompress.Click += async (s, e) => await StartCompression();
            _btnDecompress.Click += async (s, e) => await StartDecompression();
            _btnCancel.Click += (s, e) => { _compressionService.Cancel(); SetStatus("✖  Cancelled."); ShowToast("Compression cancelled.", ToastType.Warning); };
            _btnReset.Click += (s, e) => Reset();

            // ── Progress ──
            var progCard = SectionCard("Progress", 0, 320, 0, 80);
            progCard.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            progCard.Width = root.Width;
            root.Controls.Add(progCard);
            root.Resize += (s, e) => progCard.Width = root.Width;

            _progressBar = new ProgressBar
            {
                Location = new Point(12, 30),
                Width = progCard.Width - 24,
                Height = 16,
                Style = ProgressBarStyle.Continuous,
                BackColor = C_SURFACE,
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top,
            };
            progCard.Controls.Add(_progressBar);
            progCard.Resize += (s, e) => _progressBar.Width = progCard.Width - 24;

            _lblProgressPct = StatLabel(progCard, "0%", C_ACCENT, new Point(12, 52));
            _lblSpeed = StatLabel(progCard, "Speed: —", C_SUBTEXT, new Point(72, 52));
            _lblRatio = StatLabel(progCard, "Ratio: —", C_SUBTEXT, new Point(220, 52));

            // ── Charts ──
            var chartCard = SectionCard("Real-Time Monitor", 0, 408, 0, 0);
            chartCard.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            chartCard.Width = root.Width;
            chartCard.Height = root.Height - 416;
            root.Controls.Add(chartCard);
            root.Resize += (s, e) =>
            {
                chartCard.Width = root.Width;
                chartCard.Height = root.Height - 416;
                LayoutCharts(chartCard);
            };

            _chartRatio = new ScottPlot.WinForms.FormsPlot { BackColor = C_CARD };
            _chartSpeed = new ScottPlot.WinForms.FormsPlot { BackColor = C_CARD };
            chartCard.Controls.Add(_chartRatio);
            chartCard.Controls.Add(_chartSpeed);
            LayoutCharts(chartCard);
            StyleChart(_chartRatio, "Compression Ratio", "x");
            StyleChart(_chartSpeed, "Processing Speed", "KB/s");

            return root;
        }

        private void LayoutCharts(Panel chartCard)
        {
            if (_chartRatio == null || _chartSpeed == null) return;
            int w = (chartCard.Width - 30) / 2;
            int h = chartCard.Height - 32;
            if (h < 60) h = 60;
            _chartRatio.Location = new Point(8, 26);
            _chartRatio.Size = new Size(w, h);
            _chartSpeed.Location = new Point(w + 18, 26);
            _chartSpeed.Size = new Size(w, h);
        }

        // ══════════════════════════════════════════
        //  RIGHT PANEL
        // ══════════════════════════════════════════
        private Panel BuildRightPanel()
        {
            var root = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Padding = new Padding(6, 0, 0, 0) };

            var card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = C_CARD,
            };
            DrawBorder(card, C_BORDER, 1, 8);
            root.Controls.Add(card);

            // header strip
            var hdr = new Panel { Dock = DockStyle.Top, Height = 34, BackColor = C_SURFACE };
            DrawBorderBottom(hdr, C_BORDER);
            var hdrLbl = new Label
            {
                Text = "Compression Report",
                Font = F_SECTION,
                ForeColor = C_SUBTEXT,
                Location = new Point(12, 9),
                AutoSize = true,
                BackColor = Color.Transparent,
            };
            hdr.Controls.Add(hdrLbl);
            card.Controls.Add(hdr);

            // save button
            _btnSave = MakeBtn("💾  Save Compressed File", C_GREEN, 20, 150);
            _btnSave.Dock = DockStyle.Bottom;
            _btnSave.Click += (s, e) => SaveFile();
            card.Controls.Add(_btnSave);

            // report box
            _rtbReport = new RichTextBox
            {
                Dock = DockStyle.Fill,
                BackColor = C_CARD,
                ForeColor = C_TEXT,
                Font = F_MONO,
                BorderStyle = BorderStyle.None,
                ReadOnly = true,
                Padding = new Padding(10),
                ScrollBars = RichTextBoxScrollBars.Vertical,
            };
            card.Controls.Add(_rtbReport);
            WriteDefaultReport();

            return root;
        }

        private void WriteDefaultReport()
        {
            _rtbReport.Clear();
            AppendRtb("AUDIO COMPRESSION TOOL\n", C_ACCENT, true);
            AppendRtb("═══════════════════════════\n\n", C_BORDER, false);
            AppendRtb("No report yet.\n", C_SUBTEXT, false);
            AppendRtb("Load a file, configure settings,\nthen click Compress.\n", C_SUBTEXT, false);
        }

        // ══════════════════════════════════════════
        //  LOGIC
        // ══════════════════════════════════════════
        private void BrowseFile()
        {
            using (var dlg = new OpenFileDialog
            {
                Filter = "Audio Files|*.wav;*.mp3;*.aiff;*.wma|All Files|*.*",
                Title = "Select Audio File",
            })
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                    LoadFile(dlg.FileName);
            }
        }

        public void LoadFile(string path)
        {
            try
            {
                _currentFilePath = path;
                _audioInfo = _audioService.GetAudioFileInfo(path);
                _lastResult = null;

                Invoke((Action)(() =>
                {
                    _lblFileName.Text = Truncate(_audioInfo.FileName, 24);
                    _lblFileSize.Text = _audioInfo.FileSizeFormatted;
                    _lblDuration.Text = _audioInfo.DurationFormatted;
                    _lblSampleRate.Text = _audioInfo.SampleRateFormatted;
                    _lblChannels.Text = _audioInfo.ChannelsFormatted;
                    _lblBitRate.Text = _audioInfo.BitRateFormatted;
                    _lblEncoding.Text = _audioInfo.Encoding;

                    _lblDropHint.Text = "✔  " + Truncate(_audioInfo.FileName, 26);
                    _lblDropHint.ForeColor = C_GREEN;
                    _dropZone.BackColor = Color.FromArgb(18, 50, 34);
                    DrawBorder(_dropZone, C_GREEN, 2, 10);

                    _progressBar.Value = 0;
                    _lblProgressPct.Text = "0%";
                    _ratioHistory.Clear();
                    _speedHistory.Clear();
                    RefreshCharts();

                    WriteDefaultReport();
                    AppendRtb("\nFile loaded:\n", C_SUBTEXT, false);
                    AppendRtb("  " + _audioInfo.FileName + "\n", C_GREEN, true);
                    AppendRtb("\nConfigure settings and click Compress.\n", C_SUBTEXT, false);

                    UpdateControlStates();
                    SetStatus("●  Loaded: " + _audioInfo.FileName);
                    ShowToast("File loaded: " + Truncate(_audioInfo.FileName, 22), ToastType.Success);
                }));
            }
            catch (Exception ex)
            {
                ShowToast("Load error: " + ex.Message, ToastType.Error);
            }
        }

        private async Task StartCompression()
        {
            if (_currentFilePath == null) return;
            _lastResult = null;
            _ratioHistory.Clear();
            _speedHistory.Clear();
            _progressBar.Value = 0;
            _lblProgressPct.Text = "0%";
            SetStatus("⚙  Compressing...");
            UpdateControlStates();
            ShowToast("Compression started...", ToastType.Info);
            await _compressionService.CompressAsync(_currentFilePath, BuildSettings());
        }

        private async Task StartDecompression()
        {
            if (_lastResult == null) return;
            SetStatus("🔓  Decompressing...");
            ShowToast("Decompression started...", ToastType.Info);
            var settings = BuildSettings();
            short[] samples = await _compressionService.DecompressAsync(_lastResult.CompressedData, settings);
            if (samples == null) return;
            string tmp = Path.Combine(Path.GetTempPath(), "decompressed_preview.wav");
            _audioService.SaveAsWav(samples, tmp, (int)_nudSampleRate.Value);
            LoadFile(tmp);
            SetStatus("🔓  Decompression complete.");
            ShowToast("Decompression complete — preview loaded.", ToastType.Success);
        }

        private void SaveFile()
        {
            if (_lastResult == null) { ShowToast("Nothing to save yet.", ToastType.Warning); return; }
            using (var dlg = new SaveFileDialog
            {
                Filter = "Compressed Binary|*.cmp|WAV Audio|*.wav",
                FileName = "compressed_audio",
            })
            {
                if (dlg.ShowDialog() != DialogResult.OK) return;
                try
                {
                    if (dlg.FilterIndex == 1)
                        File.WriteAllBytes(dlg.FileName, _lastResult.CompressedData);
                    else
                    {
                        var algo = AlgorithmFactory.Create(GetAlgo());
                        short[] pcm = algo.Decompress(_lastResult.CompressedData, BuildSettings());
                        _audioService.SaveAsWav(pcm, dlg.FileName, (int)_nudSampleRate.Value);
                    }
                    SetStatus("💾  Saved: " + Path.GetFileName(dlg.FileName));
                    ShowToast("File saved successfully.", ToastType.Success);
                }
                catch (Exception ex) { ShowToast("Save error: " + ex.Message, ToastType.Error); }
            }
        }

        private void Reset()
        {
            _audioService.Stop();
            _currentFilePath = null;
            _audioInfo = null;
            _lastResult = null;

            _lblFileName.Text = _lblFileSize.Text = _lblDuration.Text = "—";
            _lblSampleRate.Text = _lblChannels.Text = _lblBitRate.Text = "—";
            _lblEncoding.Text = "—";

            _lblDropHint.Text = "⬇  Drop audio file here\nor click Browse";
            _lblDropHint.ForeColor = C_SUBTEXT;
            _dropZone.BackColor = C_CARD;
            DrawBorder(_dropZone, C_ACCENT, 2, 10);

            _progressBar.Value = 0;
            _lblProgressPct.Text = "0%";
            _lblSpeed.Text = "Speed: —";
            _lblRatio.Text = "Ratio: —";
            _ratioHistory.Clear();
            _speedHistory.Clear();
            RefreshCharts();
            WriteDefaultReport();
            UpdateControlStates();
            SetStatus("●  Ready — drop an audio file or click Browse");
            ShowToast("Reset complete.", ToastType.Info);
        }

        // ══════════════════════════════════════════
        //  SERVICE EVENTS
        // ══════════════════════════════════════════
        private void WireServiceEvents()
        {
            _compressionService.ProgressChanged += p =>
                Invoke((Action)(() =>
                {
                    _progressBar.Value = Math.Min(100, (int)p);
                    _lblProgressPct.Text = string.Format("{0:F1}%", p);
                }));

            _compressionService.SpeedUpdated += sp =>
                Invoke((Action)(() =>
                {
                    _lblSpeed.Text = string.Format("Speed: {0:F1} KB/s", sp / 1000.0);
                    _speedHistory.Add(sp / 1000.0);
                    RefreshCharts();
                }));

            _compressionService.RatioUpdated += r =>
                Invoke((Action)(() =>
                {
                    _lblRatio.Text = string.Format("Ratio: {0:F2}x", r);
                    _ratioHistory.Add(r);
                    RefreshCharts();
                }));

            _compressionService.CompressionCompleted += result =>
                Invoke((Action)(() =>
                {
                    _lastResult = result;
                    SetStatus("✔  Compression completed");
                    ShowToast("Compression complete!", ToastType.Success);
                    UpdateControlStates();
                    ShowReport(result);
                }));

            _compressionService.CompressionCancelled += msg =>
                Invoke((Action)(() =>
                {
                    SetStatus("✖  " + msg);
                    UpdateControlStates();
                }));

            _compressionService.ErrorOccurred += msg =>
                Invoke((Action)(() =>
                {
                    SetStatus("⚠  Error: " + msg);
                    ShowToast("Error: " + msg, ToastType.Error);
                    UpdateControlStates();
                }));

            _audioService.PlaybackStopped += (s, e) =>
                
            Invoke((Action)(() => UpdateControlStates()));
        }

        // ══════════════════════════════════════════
        //  REPORT
        // ══════════════════════════════════════════
        private void ShowReport(CompressionResult r)
        {
            _rtbReport.Clear();

            AppendRtb("COMPRESSION REPORT\n", C_ACCENT, true);
            AppendRtb("═══════════════════════════\n\n", C_DIVIDER, false);

            ReportRow("Algorithm", r.AlgorithmUsed);
            AppendRtb("\n", C_TEXT, false);

            ReportSection("FILE SIZE");
            ReportRow("Original", FormatKB(r.OriginalSize));
            ReportRow("Compressed", FormatKB(r.CompressedSize));
            ReportRow("Ratio", string.Format("{0:F2}x", r.CompressionRatio));
            ReportRow("Space Saved", string.Format("{0:F1}%", r.SpaceSavingPercent));
            AppendRtb("\n", C_TEXT, false);

            ReportSection("PERFORMANCE");
            ReportRow("Time", string.Format("{0:F3} sec", r.ProcessingTime));
            ReportRow("Throughput", string.Format("{0:F1} KB/s", r.OriginalSize / 1024.0 / r.ProcessingTime));
            AppendRtb("\n", C_TEXT, false);

            ReportSection("SETTINGS");
            ReportRow("Sample Rate", string.Format("{0} Hz", _nudSampleRate.Value));
            ReportRow("Quant Levels", string.Format("{0}", _nudQuantLevels.Value));
            ReportRow("Step Size", string.Format("{0}", _nudStepSize.Value));
            ReportRow("μ-law Param", string.Format("{0}", _nudMuLaw.Value));

            AppendRtb("\n═══════════════════════════\n", C_DIVIDER, false);
            AppendRtb(r.SpaceSavingPercent > 0
                ? string.Format("✔  Saved {0:F1}% of original size\n", r.SpaceSavingPercent)
                : "⚠  No size reduction achieved\n",
                r.SpaceSavingPercent > 0 ? C_GREEN : C_ORANGE, true);
        }

        private void AppendRtb(string text, Color col, bool bold)
        {
            _rtbReport.SelectionColor = col;
            _rtbReport.SelectionFont = bold ? F_MONO_B : F_MONO;
            _rtbReport.AppendText(text);
        }

        private void ReportRow(string key, string val)
        {
            AppendRtb(string.Format("  {0,-14}: ", key), C_SUBTEXT, false);
            AppendRtb(val + "\n", C_TEXT, true);
        }

        private void ReportSection(string title)
        {
            AppendRtb("▸ " + title + "\n", C_ACCENT, true);
        }

        private string FormatKB(long bytes)
        {
            return bytes >= 1048576
                ? string.Format("{0:F2} MB", bytes / 1048576.0)
                : string.Format("{0:F2} KB", bytes / 1024.0);
        }

        // ══════════════════════════════════════════
        //  DRAG & DROP
        // ══════════════════════════════════════════
        private void SetupDragDrop()
        {
            AllowDrop = true;
            _dropZone.AllowDrop = true;

            _dropZone.DragEnter += (s, e) =>
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    e.Effect = DragDropEffects.Copy;
                    _dropZone.BackColor = Color.FromArgb(20, 60, 90);
                    DrawBorder(_dropZone, C_ACCENT, 2, 10);
                }
            };
            _dropZone.DragLeave += (s, e) =>
            {
                _dropZone.BackColor = _audioInfo != null ? Color.FromArgb(18, 50, 34) : C_CARD;
                DrawBorder(_dropZone, _audioInfo != null ? C_GREEN : C_ACCENT, 2, 10);
            };
            _dropZone.DragDrop += (s, e) =>
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Length > 0) LoadFile(files[0]);
            };
        }

        // ══════════════════════════════════════════
        //  TIMERS
        // ══════════════════════════════════════════
        private void SetupTimers()
        {
            _playbackTimer = new Timer { Interval = 400 };
            _playbackTimer.Tick += (s, e) =>
            {
                if (!_audioService.IsPlaying || _audioInfo == null) return;
                double pos = _audioService.GetPlaybackPosition();
                _trackPlayback.Value = Math.Min(100, (int)(pos * 100));

                double elapsed = pos * _audioInfo.DurationSeconds;
                double total = _audioInfo.DurationSeconds;
                _lblPlayTime.Text = string.Format("{0}:{1:D2} / {2}:{3:D2}",
                    (int)(elapsed / 60), (int)(elapsed % 60),
                    (int)(total / 60), (int)(total % 60));
            };
            _playbackTimer.Start();
        }

        // ══════════════════════════════════════════
        //  CHARTS
        // ══════════════════════════════════════════
        private void StyleChart(ScottPlot.WinForms.FormsPlot chart, string title, string yLabel)
        {
            chart.Plot.Title(title);
            chart.Plot.YLabel(yLabel);
            chart.Plot.FigureBackground.Color = ScottPlot.Color.FromHex("#16171C");
            chart.Plot.DataBackground.Color = ScottPlot.Color.FromHex("#1A1D24");
            chart.Plot.Axes.Color(ScottPlot.Color.FromHex("#78849E"));
            // ✅ تثبيت نطاق المحور Y من -500 إلى 500
            chart.Plot.Axes.SetLimitsY(-2000, 2000);
            chart.Plot.Axes.SetLimitsX(-3000, 2000);
            chart.Refresh();
        }

        private void RefreshCharts()
        {
            _chartRatio.Plot.Clear();
            _chartSpeed.Plot.Clear();

            if (_ratioHistory.Count > 1)
            {
                var sig = _chartRatio.Plot.Add.Signal(_ratioHistory.ToArray());
                sig.Color = ScottPlot.Color.FromHex("#52A2FF");
                sig.LineWidth = 2;
            }
            if (_speedHistory.Count > 1)
            {
                var sig = _chartSpeed.Plot.Add.Signal(_speedHistory.ToArray());
                sig.Color = ScottPlot.Color.FromHex("#38C172");
                sig.LineWidth = 2;
            }

            StyleChart(_chartRatio, "Compression Ratio", "x");
            StyleChart(_chartSpeed, "Processing Speed", "KB/s");
            _chartRatio.Refresh();
            _chartSpeed.Refresh();
        }

        // ══════════════════════════════════════════
        //  HELPERS
        // ══════════════════════════════════════════
        private CompressionSettings BuildSettings()
        {
            return new CompressionSettings
            {
                Algorithm = GetAlgo(),
                SampleRate = (int)_nudSampleRate.Value,
                QuantizationLevels = (int)_nudQuantLevels.Value,
                StepSize = (double)_nudStepSize.Value,
                MuLawParameter = (int)_nudMuLaw.Value,
            };
        }

        private AlgorithmType GetAlgo() => (AlgorithmType)_cmbAlgorithm.SelectedIndex;

        private void UpdateControlStates()
        {
            bool hasFile = _currentFilePath != null;
            bool hasResult = _lastResult != null;
            bool playing = _audioService.IsPlaying;
            bool paused = _audioService.IsPaused;


            _btnPlay.Enabled = hasFile && !playing;         // ← مو متغير
            _btnPause.Enabled = playing || paused;           // ← هون الإصلاح: paused كمان
            //_btnPlay.Enabled = hasFile && !playing;
            //_btnPause.Enabled = playing;
            _btnStop.Enabled = playing || paused;
            _btnCompress.Enabled = hasFile;
            _btnDecompress.Enabled = hasResult;
            _btnSave.Enabled = hasResult;
        }

        private void SetStatus(string msg)
        {
            if (InvokeRequired) Invoke((Action)(() => _lblStatus.Text = msg));
            else _lblStatus.Text = msg;
        }

        private void UpdateAlgoDesc()
        {
            if (_lblAlgoDesc == null) return;
            string[] descs = new string[]
            {
                "Maps audio amplitudes logarithmically (μ-law) — best for speech",
                "Stores differences between consecutive samples — balanced quality",
                "Predicts next sample from previous ones — high efficiency",
                "1-bit per sample encoding — maximum compression rate",
                "Self-adjusting Delta step — improved quality over basic Delta",
            };
            int idx = _cmbAlgorithm.SelectedIndex;
            _lblAlgoDesc.Text = (idx >= 0 && idx < descs.Length) ? descs[idx] : "";
        }

        private string AlgoDisplayName(AlgorithmType t)
        {
            switch (t)
            {
                case AlgorithmType.NonlinearQuantization: return "Nonlinear Quantization (μ-law)";
                case AlgorithmType.DPCM: return "Differential PCM (DPCM)";
                case AlgorithmType.PredictiveDifferentialCoding: return "Predictive Differential Coding";
                case AlgorithmType.DeltaModulation: return "Delta Modulation";
                case AlgorithmType.AdaptiveDeltaModulation: return "Adaptive Delta Modulation";
                default: return t.ToString();
            }
        }

        private string Truncate(string s, int max)
        {
            return s != null && s.Length > max ? s.Substring(0, max) + "…" : s;
        }

        // ══════════════════════════════════════════
        //  UI FACTORY
        // ══════════════════════════════════════════
        private Panel SectionCard(string title, int x, int y, int w, int h)
        {
            var card = new Panel { Left = x, Top = y, Width = w, Height = h, BackColor = C_CARD };
            DrawBorder(card, C_BORDER, 1, 8);

            if (!string.IsNullOrEmpty(title))
            {
                var strip = new Panel { Left = 0, Top = 0, Height = 24, Width = card.Width, BackColor = C_SURFACE, Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top };
                DrawBorderBottom(strip, C_BORDER);
                var lbl = new Label { Text = title, Font = F_SECTION, ForeColor = C_SUBTEXT, Location = new Point(10, 4), AutoSize = true, BackColor = Color.Transparent };
                strip.Controls.Add(lbl);
                card.Controls.Add(strip);
                card.Resize += (s, e) => strip.Width = card.Width;
            }
            return card;
        }

        private Button MakeBtn(string text, Color col, int w, int h)
        {
            var btn = new Button
            {
                Text = text,
                Width = w,
                Height = h,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(28, col.R, col.G, col.B),
                ForeColor = col,
                Font = F_BOLD,
                Cursor = Cursors.Hand,
            };
            btn.FlatAppearance.BorderColor = col;
            btn.FlatAppearance.BorderSize = 1;
            btn.MouseEnter += (s, e) => btn.BackColor = Color.FromArgb(58, col.R, col.G, col.B);
            btn.MouseLeave += (s, e) => btn.BackColor = Color.FromArgb(28, col.R, col.G, col.B);
            return btn;
        }

        private Button MakeIconBtn(string icon, Color col, Point loc, Panel parent)
        {
            var btn = MakeBtn(icon, col, 50, 32);
            btn.Location = loc;
            btn.Font = new Font("Segoe UI", 11f);
            parent.Controls.Add(btn);
            return btn;
        }

        private Label InfoRow(Panel parent, string key, string val, int y)
        {
            var k = new Label { Text = key + ":", Font = F_SMALL, ForeColor = C_SUBTEXT, Location = new Point(12, y + 26), AutoSize = true, BackColor = Color.Transparent };
            var v = new Label { Text = val, Font = F_BOLD, ForeColor = C_TEXT, Location = new Point(104, y + 26), AutoSize = true, BackColor = Color.Transparent };
            parent.Controls.Add(k);
            parent.Controls.Add(v);
            return v;
        }

        private Label StatLabel(Panel parent, string text, Color col, Point loc)
        {
            var lbl = new Label { Text = text, Font = F_BOLD, ForeColor = col, Location = loc, AutoSize = true, BackColor = Color.Transparent };
            parent.Controls.Add(lbl);
            return lbl;
        }

        private NumericUpDown NudSetting(Panel parent, string label, int min, int max, int val, Point loc)
        {
            var lbl = new Label { Text = label, Font = F_SMALL, ForeColor = C_SUBTEXT, Location = new Point(loc.X, loc.Y), AutoSize = true, BackColor = Color.Transparent };
            var nud = new NumericUpDown
            {
                Minimum = min,
                Maximum = max,
                Value = val,
                Location = new Point(loc.X, loc.Y + 16),
                Width = 218,
                BackColor = C_SURFACE,
                ForeColor = C_TEXT,
                BorderStyle = BorderStyle.FixedSingle,
                Font = F_LABEL,
            };
            parent.Controls.Add(lbl);
            parent.Controls.Add(nud);
            return nud;
        }

        // ──────────────────────────────────────────
        //  BORDER DRAWING
        // ──────────────────────────────────────────
        private static void DrawBorder(Panel p, Color col, int thick, int radius)
        {
            // Remove old paint handlers by creating new panel paint
            p.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var pen = new Pen(col, thick))
                {
                    var rect = new Rectangle(1, 1, p.Width - 2, p.Height - 2);
                    using (var path = RoundRect(rect, radius))
                        g.DrawPath(pen, path);
                }
            };
        }

        private static void DrawBorderBottom(Panel p, Color col)
        {
            p.Paint += (s, e) =>
            {
                using (var pen = new Pen(col, 1))
                    e.Graphics.DrawLine(pen, 0, p.Height - 1, p.Width, p.Height - 1);
            };
        }

        private static GraphicsPath RoundRect(Rectangle r, int rad)
        {
            int d = rad * 2;
            var path = new GraphicsPath();
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}