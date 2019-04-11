using NReco.VideoConverter;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VideoToMP3
{
    public partial class FrmMain : Form
    {
        String SourcePath;
        String TargetPath;
        Task TaskRunning;
        TimeSpan ElapsedTimeProcessingFiles;
        Int64 TotalFilesProcessed;
        public FrmMain()
        {
            InitializeComponent();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            try
            {
                if (fbdFolderDialog.ShowDialog() == DialogResult.OK)
                {
                    if (string.IsNullOrEmpty(SourcePath))
                    {
                        SourcePath = fbdFolderDialog.SelectedPath;
                        txtSourceDir.Text = SourcePath;
                    }
                    else
                    {
                        TargetPath = fbdFolderDialog.SelectedPath;
                        txtTargetDir.Text = TargetPath;
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void btnConvertFile_Click(object sender, EventArgs e)
        {
            TaskRunning = Task.Factory.StartNew(() =>
            {
                if (string.IsNullOrEmpty(SourcePath) && string.IsNullOrEmpty(TargetPath))
                {
                    MessageBox.Show("Selecione os diretorios dos arquivos!");
                    return;
                }
                var Files = IterateFilesIntoSelectedPath();
                foreach (var File in Files)
                {
                    Task.Factory.StartNew(() => {
                        var watch = new Stopwatch();
                        watch.Start();
                        var ConvertVideo = new FFMpegConverter();
                        ConvertVideo.ConvertMedia(File.FullName, $"{File.Name}.{File.Extension}", File.Extension);
                        TotalFilesProcessed++;
                        watch.Stop();
                        ElapsedTimeProcessingFiles =+ watch.Elapsed;
                    });
                }
                Clear();
            }, CancellationToken.None, TaskCreationOptions.None, PriorityScheduler.Highest);
            txtSourceDir.Clear();
            txtTargetDir.Clear();
        }

        private void Clear()
        {
            SourcePath = "";
            TargetPath = "";
        }

        private List<FileParser> IterateFilesIntoSelectedPath()
        {
            var fileList = new List<FileParser>();
            var validateFileList = new List<FileParser>();
            var sourceDir = new DirectoryInfo(SourcePath);
            var targetDir = new DirectoryInfo(TargetPath);
            foreach (var File in targetDir.GetFiles())
            {
                if (File.Exists)
                {
                    validateFileList.Add(new FileParser(File.FullName, File.Name.Substring(0, File.Name.IndexOf(".")), File.Extension));
                }
            }

            foreach (var File in sourceDir.GetFiles())
            {
                if (File.Exists)
                {
                    bool IsDuplicate = validateFileList.Any(x => x.Name.Equals($"{File.Name.Substring(0, File.Name.IndexOf("."))}"));
                    if (!IsDuplicate)
                    {
                        fileList.Add(new FileParser(File.FullName, $"{TargetPath}\\{File.Name.Substring(0, File.Name.IndexOf("."))}", "mp3"));
                    }
                }
            }
            return fileList;
        }

        private void UpdateUI()
        {
            bool isRunning = TaskRunning.Status == TaskStatus.Running;
            btnBrowse.Enabled = !isRunning;
            btnConvertFile.Enabled = !isRunning;
            txtSourceDir.Enabled = !isRunning;
            txtTargetDir.Enabled = !isRunning;
        }

        private void btnInfo_Click(object sender, EventArgs e)
        {
            MessageBox.Show($"ARQUIVOS CONVERTIDOS - {TotalFilesProcessed} : TEMPO DE EXECUÇÃO - {ElapsedTimeProcessingFiles}");
            TotalFilesProcessed = 0;
            ElapsedTimeProcessingFiles = new TimeSpan();
        }
    }
}
