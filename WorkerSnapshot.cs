using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace subs2srs
{
    public class WorkerSnapshot
    {
        public bool genSnapshots(WorkerVars workerVars, DialogProgress dialogProgress)
        {
            int totalEpisodes = workerVars.CombinedAll.Count;
            int totalLines = UtilsSubs.getTotalLineCount(workerVars.CombinedAll);
            DateTime lastTime = UtilsSubs.getLastTime(workerVars.CombinedAll);

            UtilsName name = new UtilsName(Settings.Instance.DeckName, totalEpisodes,
              totalLines, lastTime, Settings.Instance.VideoClips.Size.Width, Settings.Instance.VideoClips.Size.Height);

            // Use a shared progress counter
            int progessCount = 0;
            object progressLock = new object();

            // Process episodes in parallel
            Parallel.For(0, totalEpisodes, (episodeIndex, state) =>
            {
                List<InfoCombined> combArray = workerVars.CombinedAll[episodeIndex];
                string videoFileName = Settings.Instance.VideoClips.Files[episodeIndex];
                int localEpisodeCount = episodeIndex + 1;

                for (int i = 0; i < combArray.Count; i++)
                {
                    lock (progressLock)
                    {
                        if (dialogProgress.Cancel)
                        {
                            state.Stop();
                            return;
                        }

                        progessCount++;

                        string progressText = string.Format("Generating snapshot: {0} of {1}",
                                                            progessCount,
                                                            totalLines);

                        int progress = Convert.ToInt32(progessCount * (100.0 / totalLines));

                        DialogProgress.updateProgressInvoke(dialogProgress, progress, progressText);
                    }

                    InfoCombined comb = combArray[i];
                    DateTime startTime = comb.Subs1.StartTime;
                    DateTime endTime = comb.Subs1.EndTime;
                    DateTime midTime = UtilsSubs.getMidpointTime(startTime, endTime);

                    string nameStr = name.createName(ConstantSettings.SnapshotFilenameFormat,
                      localEpisodeCount + Settings.Instance.EpisodeStartNumber - 1,
                      progessCount, startTime, endTime, comb.Subs1.Text, comb.Subs2.Text);

                    string outFile = string.Format("{0}{1}{2}",
                                                    workerVars.MediaDir,
                                                    Path.DirectorySeparatorChar,
                                                    nameStr);

                    // Generate snapshot
                    UtilsSnapshot.takeSnapshotFromVideo(videoFileName, midTime, Settings.Instance.Snapshots.Size,
                      Settings.Instance.Snapshots.Crop, outFile);
                }
            });

            // Check if the operation was canceled
            if (dialogProgress.Cancel)
            {
                return false;
            }

            return true;
        }
    }
}
