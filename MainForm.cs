using System.Diagnostics;

namespace MovToMp4Converter
{
    public class MainForm : Form
    {
        // ==== Config ====
        // Nếu ffmpeg không có trong PATH, đặt đường dẫn tuyệt đối tại đây, ví dụ:
        // private const string FfmpegPath = @"C:\ffmpeg\bin\ffmpeg.exe";
        // Nếu ffmpeg có trong PATH:
        // private const string FfmpegPath = "ffmpeg";
        // Nếu đã copy ffmpeg.exe vào cùng thư mục với file .exe khi publish
        private static readonly string FfmpegPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg.exe"); // dùng ffmpeg.exe cùng thư mục với file .exe khi publish

        // Mặc định: encode H.264 chất lượng cao, iPhone đọc được chắc chắn
        private const string FfmpegArgsTemplate = "-i \"{IN}\" -c:v libx264 -crf 14 -preset slow -c:a aac -b:a 192k -movflags +faststart \"{OUT}\" -y";

        // === UI controls ===
        private Panel dragDropPanel = new Panel();
        private ComboBox modeCombo = new ComboBox();
        private Button btnConvert = new Button();
        private ProgressBar progressBar = new ProgressBar();
        private TextBox logBox = new TextBox();
        private Label hintLabel = new Label();
        private Label convertingLabel = new Label(); // Thêm label trạng thái chuyển đổi
        private Button btnCancel = new Button();

        // Data
        private readonly List<string> _files = new List<string>();
        private bool _isConverting = false;
        private CancellationTokenSource? _cts = null; // Thêm biến hủy

        public MainForm()
        {
            Text = "MOV → MP4 Converter";
            Width = 820;
            Height = 560;
            StartPosition = FormStartPosition.CenterScreen;
            MinimizeBox = true;
            MaximizeBox = true;

            InitializeComponent();
            WireEvents();
        }

        private void InitializeComponent()
        {
            // Drag-drop panel
            dragDropPanel.BorderStyle = BorderStyle.FixedSingle;
            dragDropPanel.AllowDrop = true;
            dragDropPanel.Left = 16;
            dragDropPanel.Top = 16;
            dragDropPanel.Width = ClientSize.Width - 32;
            dragDropPanel.Height = 180;
            dragDropPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            hintLabel.Text = "Kéo thả file .mov hoặc thư mục chứa .mov vào đây";
            hintLabel.AutoSize = false;
            hintLabel.TextAlign = ContentAlignment.MiddleCenter;
            hintLabel.Dock = DockStyle.Fill;
            hintLabel.Font = new Font("Segoe UI", 11f, FontStyle.Regular);
            dragDropPanel.Controls.Add(hintLabel);

            // Convert button
            btnConvert.Text = "Convert";
            btnConvert.Width = 120;
            btnConvert.Height = 36;
            btnConvert.Left = 16;
            btnConvert.Top = dragDropPanel.Bottom + 12;
            btnConvert.BackColor = Color.FromArgb(0, 120, 215);
            btnConvert.ForeColor = Color.White;
            btnConvert.FlatStyle = FlatStyle.Flat;

            // Mode combo box
            modeCombo.Left = btnConvert.Right + 16;
            modeCombo.Top = btnConvert.Top + 6;
            modeCombo.Width = 260;
            modeCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            modeCombo.Items.AddRange(
            [
                "Giữ nguyên chất lượng (copy stream, rất nhanh)",
                "Chất lượng cao (rất nét, file lớn)",
                "Chất lượng tốt (khuyên dùng)",
                "Nén mạnh (nhỏ gọn, chất lượng giảm)"
            ]);
            modeCombo.SelectedIndex = 2;

            // Converting label
            convertingLabel.Text = "Đang chuyển đổi ...";
            convertingLabel.Font = new Font("Segoe UI", 11f, FontStyle.Bold);
            convertingLabel.TextAlign = ContentAlignment.MiddleCenter;
            convertingLabel.Width = 220;
            convertingLabel.Height = 24;
            convertingLabel.Left = (ClientSize.Width - convertingLabel.Width) / 2;
            convertingLabel.Top = btnConvert.Bottom + 8;
            convertingLabel.Anchor = AnchorStyles.Top;
            convertingLabel.Visible = false;

            // Progress bar
            progressBar.Left = 16;
            progressBar.Top = convertingLabel.Bottom + 4;
            progressBar.Width = ClientSize.Width - 32;
            progressBar.Height = 20;
            progressBar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            progressBar.Visible = false;

            // Cancel button
            btnCancel.Text = "Hủy";
            btnCancel.Width = 80;
            btnCancel.Height = 36;
            btnCancel.Left = (ClientSize.Width - btnCancel.Width) / 2;
            btnCancel.Top = progressBar.Bottom + 8;
            btnCancel.BackColor = Color.FromArgb(220, 53, 69);
            btnCancel.ForeColor = Color.White;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.Visible = false;

            // Log box
            logBox.Multiline = true;
            logBox.ScrollBars = ScrollBars.Vertical;
            logBox.ReadOnly = true;
            logBox.Left = 16;
            logBox.Top = btnConvert.Bottom + 12;
            logBox.Width = ClientSize.Width - 32;
            logBox.Height = ClientSize.Height - logBox.Top - 40;
            logBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            logBox.Font = new Font("Consolas", 10f);

            Controls.Add(dragDropPanel);
            Controls.Add(modeCombo);
            Controls.Add(btnConvert);
            Controls.Add(progressBar);
            Controls.Add(logBox);
            Controls.Add(convertingLabel);
            Controls.Add(btnCancel);
        }

        private void WireEvents()
        {
            dragDropPanel.DragEnter += DragDropPanel_DragEnter;
            dragDropPanel.DragDrop += DragDropPanel_DragDrop;
            btnConvert.Click += BtnConvert_Click;
            btnCancel.Click += BtnCancel_Click;
            Shown += (_, __) => AppendLog("Kéo file .mov hoặc thư mục vào vùng trên, rồi bấm Convert.\r\n");
        }

        private void DragDropPanel_DragEnter(object? sender, DragEventArgs e)
        {
            if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
                e.Effect = DragDropEffects.Copy;
        }

        private void DragDropPanel_DragDrop(object? sender, DragEventArgs e)
        {
            if (e.Data?.GetData(DataFormats.FileDrop) is string[] paths)
            {
                int before = _files.Count;
                foreach (var p in paths)
                {
                    if (Directory.Exists(p))
                    {
                        // lấy tất cả .mov trong thư mục (đệ quy)
                        var movs = Directory.EnumerateFiles(p, "*.mov", SearchOption.AllDirectories);
                        foreach (var f in movs)
                            TryAddMov(f);
                    }
                    else if (File.Exists(p))
                    {
                        TryAddMov(p);
                    }
                }

                int added = _files.Count - before;
                if (added > 0)
                {
                    AppendLog($"Đã thêm {added} file .mov. Tổng: {_files.Count} file.\r\n");
                }
                else
                {
                    AppendLog($"Không có file .mov hợp lệ được thêm.\r\n");
                }
            }
        }

        private void TryAddMov(string path)
        {
            if (string.Equals(Path.GetExtension(path), ".mov", StringComparison.OrdinalIgnoreCase))
            {
                if (!_files.Contains(path))
                    _files.Add(path);
            }
        }

        private async void BtnConvert_Click(object? sender, EventArgs e)
        {
            if (_isConverting)
            {
                MessageBox.Show("Đang convert, vui lòng đợi cho đến khi hoàn tất.");
                return;
            }

            if (_files.Count == 0)
            {
                MessageBox.Show("Chưa có file .mov nào. Hãy kéo & thả file/thư mục vào vùng trên.");
                return;
            }

            // Kiểm tra ffmpeg
            if (!IsFfmpegAvailable())
            {
                MessageBox.Show("Không tìm thấy ffmpeg. Hãy chắc chắn bạn đã cài đặt và thêm vào PATH.");
                return;
            }

            convertingLabel.Text = "Đang chuyển đổi ...";
            _isConverting = true;
            btnConvert.Enabled = false;
            modeCombo.Enabled = false;
            btnCancel.Enabled = true;
            btnCancel.Visible = true;
            convertingLabel.Visible = true;
            progressBar.Visible = true;
            UpdateLogBoxPosition();

            progressBar.Minimum = 0;
            progressBar.Maximum = _files.Count;
            progressBar.Value = 0;

            AppendLog($"Bắt đầu convert {_files.Count} file...\r\n");

            _cts = new CancellationTokenSource();

            foreach (var input in _files.ToList())
            {
                string output = Path.ChangeExtension(input, ".mp4");
                try
                {
                    // Luôn encode H.264 chất lượng cao
                    string args = GetFfmpegArgs(input, output);
                    bool ok = await RunFfmpegAsync(args, _cts.Token);

                    if (ok)
                    {
                        AppendLog($"✅ Đã chuyển: {Path.GetFileName(input)}  →  {Path.GetFileName(output)}\r\n");
                    }
                    else if (_cts.IsCancellationRequested)
                    {
                        AppendLog($"❌ Đã hủy: {Path.GetFileName(input)}\r\n");
                        break;
                    }
                    else
                    {
                        AppendLog($"❌ Không thể chuyển: {Path.GetFileName(input)}\r\n");
                    }
                }
                catch (Exception ex)
                {
                    AppendLog($"❌ Lỗi với {Path.GetFileName(input)}: {ex.Message}\r\n");
                }
                finally
                {
                    if (progressBar.Value < progressBar.Maximum)
                        progressBar.Value += 1;
                }
            }

            if (_cts.IsCancellationRequested)
                AppendLog("Quá trình chuyển đổi đã bị hủy.\r\n");

            _files.Clear();
            _isConverting = false;
            btnConvert.Enabled = true;
            modeCombo.Enabled = true;
            btnCancel.Visible = false;
            convertingLabel.Text = "Đã hoàn tất!";
            UpdateLogBoxPosition();
            _cts.Dispose();
            _cts = null;
        }

        private void BtnCancel_Click(object? sender, EventArgs e)
        {
            if (_isConverting && _cts != null)
            {
                _cts.Cancel();
                btnCancel.Enabled = false;
                convertingLabel.Text = "Đang hủy ...";
            }
        }

        private void AppendLog(string message)
        {
            if (logBox.InvokeRequired)
            {
                logBox.Invoke(new Action<string>(AppendLog), message);
                return;
            }
            logBox.AppendText(message);
        }

        private bool IsFfmpegAvailable()
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = FfmpegPath,
                    Arguments = "-version",
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using var p = Process.Start(psi)!;
                p.WaitForExit(3000);
                return p.ExitCode == 0 || p.ExitCode == 1; // -version trả 0, 1 đều coi như chạy được
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> RunFfmpegAsync(string arguments, CancellationToken ct)
        {
            return await Task.Run(() =>
            {
                var psi = new ProcessStartInfo
                {
                    FileName = FfmpegPath,
                    Arguments = arguments,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var proc = new Process { StartInfo = psi };
                try
                {
                    proc.Start();

                    proc.ErrorDataReceived += (_, e) => { /* ... */ };
                    proc.BeginErrorReadLine();

                    while (!proc.HasExited)
                    {
                        if (ct.IsCancellationRequested)
                        {
                            try { proc.Kill(); } catch { }
                            return false;
                        }
                        Thread.Sleep(100);
                    }

                    // ffmpeg trả 0 = OK
                    return proc.ExitCode == 0;
                }
                catch (Exception ex)
                {
                    AppendLog($"[ffmpeg] {ex.Message}\r\n");
                    return false;
                }
            }, ct);
        }

        private string GetFfmpegArgs(string input, string output)
        {
            switch (modeCombo.SelectedIndex)
            {
                case 0:
                    return $"-i \"{input}\" -c:v copy -c:a aac -b:a 192k -movflags +faststart \"{output}\" -y";
                case 1:
                    return $"-i \"{input}\" -c:v libx264 -crf 14 -preset slow -c:a aac -b:a 192k -movflags +faststart \"{output}\" -y";
                case 2:
                    return $"-i \"{input}\" -c:v libx264 -crf 18 -preset slow -c:a aac -b:a 192k -movflags +faststart \"{output}\" -y";
                case 3:
                    return $"-i \"{input}\" -c:v libx264 -crf 23 -preset medium -c:a aac -b:a 128k -movflags +faststart \"{output}\" -y";
                default:
                    return FfmpegArgsTemplate.Replace("{IN}", input).Replace("{OUT}", output);
            }
        }
        
        private void UpdateLogBoxPosition()
        {
            // Nếu đang chuyển đổi, logBox nằm dưới btnCancel
            if (btnCancel.Visible)
            {
                logBox.Top = btnCancel.Bottom + 12;
            }
            else if (convertingLabel.Visible && progressBar.Visible)
            {
                logBox.Top = progressBar.Bottom + 12;
            }
            else
            {
                // Nếu không chuyển đổi, logBox nằm dưới btnConvert
                logBox.Top = btnConvert.Bottom + 12;
            }
        }
    }
}
