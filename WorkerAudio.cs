using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace subs2srs
{
    public class WorkerAudio
    {
        private int _processedCount = 0;
        private readonly object _lock = new object();
        private ConcurrentDictionary<string, int> _fileNameCounter = new ConcurrentDictionary<string, int>();

        public bool genAudioClip(WorkerVars workerVars, DialogProgress dialogProgress)
        {
            _processedCount = 0;
            _fileNameCounter.Clear();
            int totalEpisodes = workerVars.CombinedAll.Count;
            int totalLines = UtilsSubs.getTotalLineCount(workerVars.CombinedAll);
            DateTime lastTime = UtilsSubs.getLastTime(workerVars.CombinedAll);

            UtilsName name = new UtilsName(Settings.Instance.DeckName, totalEpisodes,
                totalLines, lastTime, Settings.Instance.VideoClips.Size.Width, Settings.Instance.VideoClips.Size.Height);

            DialogProgress.updateProgressInvoke(dialogProgress, 0, "Initializing parallel processing...");

 
            try
            {
                // 第一级并行：按剧集处理
                Parallel.ForEach(workerVars.CombinedAll, new ParallelOptions
                {
                    MaxDegreeOfParallelism = Environment.ProcessorCount
                }, (combArray, state, episodeIndex) =>
                {
                    int episodeCount = (int)episodeIndex + 1;
                    int episodeNumber = episodeCount + Settings.Instance.EpisodeStartNumber - 1;

                    if (combArray.Count == 0) return;

                    string tempMp3Filename = Path.Combine(
                        Path.GetTempPath(),
                        $"{ConstantSettings.TempAudioFilename}_EP{episodeNumber}.opus"
                    );

                    if (!ProcessEpisodeAudio(episodeCount, combArray, tempMp3Filename, dialogProgress))
                    {
                        state.Break();
                        return;
                    }

                    // 预先生成所有文件名
                    var fileNames = new ConcurrentDictionary<int, string>();
                    for (int i = 0; i < combArray.Count; i++)
                    {
                        var comb = combArray[i];
                        var lineIndex = i + 1;
                        string nameStr = GenerateFileName(name, combArray, comb, episodeCount, lineIndex);
                        fileNames[lineIndex] = nameStr;
                    }

                    // 第二级并行：处理单句台词
                    Parallel.ForEach(Partitioner.Create(0, combArray.Count), (range, loopState) =>
                    {
                        for (int i = range.Item1; i < range.Item2; i++)
                        {
                            var comb = combArray[i];
                            var lineIndex = i + 1;
                            ProcessSingleLine(combArray, comb, episodeCount, lineIndex,
                                tempMp3Filename, fileNames[lineIndex], workerVars.MediaDir,
                                dialogProgress, totalLines);
                        }
                    });

                    SafeDeleteTempFile(tempMp3Filename);
                });
            }
            catch (AggregateException ae)
            {
                foreach (var ex in ae.InnerExceptions)
                {
                    UtilsMsg.showErrMsg($"Critical error: {ex.Message}");
                }
                return false;
            }

            if (Settings.Instance.AudioClips.Normalize)
            {
                DialogProgress.updateProgressInvoke(dialogProgress, -1, "Normalizing audio...");
                UtilsAudio.normalizeAudio(workerVars.MediaDir);
            }

            return true;
        }

        private string GenerateFileName(UtilsName name, List<InfoCombined> combArray,
            InfoCombined comb, int episodeCount, int lineIndex)
        {
            DateTime filenameStartTime = comb.Subs1.StartTime;
            DateTime filenameEndTime = comb.Subs1.EndTime;

            if (Settings.Instance.AudioClips.PadEnabled)
            {
                filenameStartTime = UtilsSubs.applyTimePad(comb.Subs1.StartTime,
                    -Settings.Instance.AudioClips.PadStart);
                filenameEndTime = UtilsSubs.applyTimePad(comb.Subs1.EndTime,
                    Settings.Instance.AudioClips.PadEnd);
            }

            return name.createName(
                ConstantSettings.AudioFilenameFormat,
                episodeCount + Settings.Instance.EpisodeStartNumber - 1,
                lineIndex,
                filenameStartTime,
                filenameEndTime,
                comb.Subs1.Text.Trim(),
                Settings.Instance.Subs[1].Files.Length != 0 ? comb.Subs2.Text.Trim() : ""
            );
        }

        private bool ProcessEpisodeAudio(int episodeCount, List<InfoCombined> combArray,
            string tempMp3Filename, DialogProgress dialogProgress)
        {
            try
            {
                bool inputFileIsMp3 = (Settings.Instance.AudioClips.Files.Length > 0)
                    && Path.GetExtension(Settings.Instance.AudioClips.Files[episodeCount - 1])
                        .Equals(".opus", StringComparison.OrdinalIgnoreCase);

                DateTime entireClipStartTime = combArray[0].Subs1.StartTime;
                DateTime entireClipEndTime = combArray[combArray.Count - 1].Subs1.EndTime;

                if (Settings.Instance.AudioClips.PadEnabled)
                {
                    entireClipStartTime = UtilsSubs.applyTimePad(entireClipStartTime,
                        -Settings.Instance.AudioClips.PadStart);
                    entireClipEndTime = UtilsSubs.applyTimePad(entireClipEndTime,
                        Settings.Instance.AudioClips.PadEnd);
                }

                // 音频处理逻辑
                if (Settings.Instance.AudioClips.UseAudioFromVideo)
                {
                    string progressText = $"Extracting audio (Episode {episodeCount})";
                    return ConvertToMp3(
                        Settings.Instance.VideoClips.Files[episodeCount - 1],
                        Settings.Instance.VideoClips.AudioStream.Num.ToString(),
                        progressText,
                        dialogProgress,
                        entireClipStartTime,
                        entireClipEndTime,
                        tempMp3Filename);
                }
                else if (ConstantSettings.ReencodeBeforeSplittingAudio || !inputFileIsMp3)
                {
                    string progressText = $"Reencoding audio (Episode {episodeCount})";
                    return ConvertToMp3(
                        Settings.Instance.AudioClips.Files[episodeCount - 1],
                        "0",
                        progressText,
                        dialogProgress,
                        entireClipStartTime,
                        entireClipEndTime,
                        tempMp3Filename);
                }

                return true;
            }
            catch (Exception ex)
            {
                UtilsMsg.showErrMsg($"Episode processing failed: {ex.Message}");
                return false;
            }
        }

        private void ProcessSingleLine(List<InfoCombined> combArray, InfoCombined comb,
         int episodeCount, int lineIndex, string tempMp3Filename, string nameStr,
         string mediaDir, DialogProgress dialogProgress, int totalLines)
        {
            try
            {
                DateTime startTime = comb.Subs1.StartTime;
                DateTime endTime = comb.Subs1.EndTime;

                if (Settings.Instance.AudioClips.UseAudioFromVideo ||
                    ConstantSettings.ReencodeBeforeSplittingAudio ||
                    !Path.GetExtension(tempMp3Filename).Equals(".opus", StringComparison.OrdinalIgnoreCase))
                {
                    var entireClipStartTime = combArray[0].Subs1.StartTime;
                    startTime = UtilsSubs.shiftTiming(startTime,
                        -((int)entireClipStartTime.TimeOfDay.TotalMilliseconds));
                    endTime = UtilsSubs.shiftTiming(endTime,
                        -((int)entireClipStartTime.TimeOfDay.TotalMilliseconds));
                }

                if (Settings.Instance.AudioClips.PadEnabled)
                {
                    startTime = UtilsSubs.applyTimePad(startTime, -Settings.Instance.AudioClips.PadStart);
                    endTime = UtilsSubs.applyTimePad(endTime, Settings.Instance.AudioClips.PadEnd);
                }

                string outName = Path.Combine(mediaDir, nameStr);

                // 确保目录存在
                Directory.CreateDirectory(Path.GetDirectoryName(outName));

                // 执行音频切割
                UtilsAudio.cutAudio(tempMp3Filename, startTime, endTime, outName);


                // 更新进度（线程安全）
                int currentCount = Interlocked.Increment(ref _processedCount);
                int progress = (int)((double)currentCount / totalLines * 100);
                DialogProgress.updateProgressInvoke(dialogProgress, progress,
                    $"Processed: {currentCount}/{totalLines} clips");
            }
            catch (Exception ex)
            {
                UtilsMsg.showErrMsg($"Error processing line {lineIndex} in episode {episodeCount}: {ex}");
            }
        }

        private bool ConvertToMp3(string inputFile, string stream, string progressText,
            DialogProgress dialogProgress, DateTime startTime, DateTime endTime, string outputFile)
        {
            try
            {
                DialogProgress.updateProgressInvoke(dialogProgress, -1, progressText);
                DateTime duration = UtilsSubs.getDurationTime(startTime, endTime);

                DialogProgress.enableDetailInvoke(dialogProgress, true);
                DialogProgress.setDuration(dialogProgress, duration);

                UtilsAudio.ripAudioFromVideo(inputFile, stream, startTime, endTime,
                    Settings.Instance.AudioClips.Bitrate, outputFile, dialogProgress);

                return File.Exists(outputFile) && new FileInfo(outputFile).Length > 0;
            }
            finally
            {
                DialogProgress.enableDetailInvoke(dialogProgress, false);
            }
        }


        private static void SafeDeleteTempFile(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch (Exception ex)
            {
                UtilsMsg.showErrMsg($"Temp file deletion failed: {ex.Message}");
            }
        }
    }
}