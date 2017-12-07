using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BoZPreparation_Tool
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        CameraSettingPage cameraSettingPage = new CameraSettingPage();
        PictureSettingPage pictureSettingPage = new PictureSettingPage();
        AdvancedSettingPage advancedSettingPage = new AdvancedSettingPage();
        MapWindowPage mapWindowPage = new MapWindowPage();
        Processing processingPage = new Processing();
        BackgroundWorker worker = null;

        private string strTempDir;
        private string strSaveRecogResultDir;
        private string strLRUResultsCsvFilename;
        private string UnstableAreaDetectionDir;
        private string UnstableSoftName;
        private string CaptureRawDataDir;

        public List<BozTask> bozTasks = new List<BozTask>();

        public MainWindow()
        {
            InitializeComponent();
            pageTransitionControl.TransitionType = WpfPageTransitions.PageTransitionType.SlideAndFade;
            pageTransitionControl.ShowPage(cameraSettingPage);
            cameraSettingPage.CameraSettingPageNextBtnClickEvent += new CameraSettingPageNextBtnClickDelegate(ShowPictureSettingPage);
            cameraSettingPage.CameraSettingPageShowMapBtnClickEvent += new CameraSettingPageShowMapBtnClickDelegate(ShowMap);
            pictureSettingPage.PictureSettingPageAdvancedBtnClickEvent += new PictureSettingPageAdvancedBtnClickDelegate(ShowAdvancedSettingPage);
            pictureSettingPage.PictureSettingPageBackBtnClickEvent += new PictureSettingPageBackBtnClickDelegate(ShowCameraSettingPage);
            pictureSettingPage.PictureSettingPageOkBtnClickEvent += new PictureSettingPageOkBtnClickDelegate(ShowProcessingPage);
            advancedSettingPage.AdvancedSettingPageBackBtnClickEvent += new AdvancedSettingPageBackBtnClickDelegate(ShowPictureSettingPage);
            advancedSettingPage.AdvancedSettingPageOkBtnClickEvent += new AdvancedSettingPageOkBtnClickDelegatte(ShowProcessingPage);
            mapWindowPage.MapWindowPageCancelBtnClickEvent += new MapWindowPageCancelBtnClickDelegate(ShowCameraSettingPage);
            mapWindowPage.MapWindowPageOkBtnClickEvent += new MapWindowPageOkBtnClickDelegate(ShowCameraSettingPage);
            worker = new BackgroundWorker();
            worker.DoWork += RunBoZTasks;
            worker.RunWorkerCompleted += CallFinished;
            worker.WorkerReportsProgress = true;
            worker.ProgressChanged += ProgressStatus;
        }

        public void ShowCameraSettingPage(object sender, EventArgs e)
        {
            pageTransitionControl.ShowPage(cameraSettingPage);
        }

        public void ShowPictureSettingPage(object sender, EventArgs e)
        {
            pageTransitionControl.ShowPage(pictureSettingPage);
        }

        public void ShowAdvancedSettingPage(object sender, EventArgs e)
        {
            pageTransitionControl.ShowPage(advancedSettingPage);
        }

        public void ShowProcessingPage(object sender,EventArgs e)
        {
            //UnstableAreaDetectionDir = @".\DCD";
            UnstableAreaDetectionDir = @".";
            DirectoryInfo UnstableAreaDetectionDirInfo = new DirectoryInfo(UnstableAreaDetectionDir);
            UnstableAreaDetectionDir = UnstableAreaDetectionDirInfo.FullName;
            UnstableSoftName = UnstableAreaDetectionDir+@"\DCD.exe";
            CaptureRawDataDir = pictureSettingPage.CaptureRawDataDir;
            if (string.IsNullOrWhiteSpace(CaptureRawDataDir))
            {
                System.Windows.Forms.MessageBox.Show("Please Input Correct Directory Path");
                return;
            }
            DirectoryInfo directoryInfo = new DirectoryInfo(pictureSettingPage.CaptureRawDataDir);
            strTempDir = System.IO.Path.Combine(directoryInfo.Parent.FullName, directoryInfo.Name + @"_Results");
            strSaveRecogResultDir = System.IO.Path.Combine(strTempDir, @"Recognition");
            strLRUResultsCsvFilename = System.IO.Path.Combine(strSaveRecogResultDir, @"LRUResult_"+BoZConstant.nStartFrameNo+@"_"+BoZConstant.ProcessingFrameNum+@".csv");

            DirectoryInfo strTempDirInfo = new DirectoryInfo(strTempDir);
            if (!strTempDirInfo.Exists)
            {
                strTempDirInfo.Create();
            }

            BozTask initInIWriterTask = new BozTask("IniWriter.exe",   "BirdsView.ini -CameraHeight " + cameraSettingPage.CameraHeight + @" -CameraPitch " + cameraSettingPage.CameraAngle + " -ProcessingFrameNum " + BoZConstant.ProcessingFrameNum + " -StartFrameNo " + BoZConstant.nStartFrameNo);
            BozTask secondIniWriterTask = new BozTask("IniWriter.exe", "Param_3DPos.ini -CameraHeight " + cameraSettingPage.CameraHeight + @" -CameraPitch " + cameraSettingPage.CameraAngle + " -ProcessingFrameNum " + BoZConstant.ProcessingFrameNum + " -StartFrameNo " + BoZConstant.nStartFrameNo);
            bozTasks.Add(initInIWriterTask);
            bozTasks.Add(secondIniWriterTask);

            if (advancedSettingPage.IsBirdsViewCreation == true)
            {
                AddBirdsViewTask();
            }

            if (advancedSettingPage.IsDetectionConfirmation == true)
            {
                //AddDCDTask();
            }

            if (advancedSettingPage.IsObjectDetection == true)
            {
                //AddLRU3DTask();
            }

            if(advancedSettingPage.IsHeapMapCreation == true)
            {
                //AddHeatmapTask();
            }
            processingPage.ProcessingPageOKBtn.IsEnabled = false;
            worker.RunWorkerAsync();
            pageTransitionControl.ShowPage(processingPage);
        }

        public void ShowMap(object sender, RoutedEventArgs e)
        {
            mapWindowPage.SetCoordinate((sender as CameraSettingPage).currentLongitude, (sender as CameraSettingPage).currentLatitude);
            //mapWindowPage.SetCoordinate(e.Longitude,e.Latitude);
            pageTransitionControl.ShowPage(mapWindowPage);
        }


        private void AddBirdsViewTask()
        {
            // iniwriter.exe
            BozTask iniWriterTask = new BozTask("IniWriter.exe", @"BirdsView.ini -ObjInfo 1 -SaveDataPath " + strTempDir + " -FrameSkipNum 1");
            bozTasks.Add(iniWriterTask);
            //birdsview.exe
            BozTask birdsViewTask = new BozTask("BirdsView.exe", CaptureRawDataDir);
            bozTasks.Add(birdsViewTask);
            //del file
            BozTask deleteFiles = new BozTask("DelFiles", UnstableAreaDetectionDir+@"\data\matched\train\*.png");
            //bozTasks.Add(deleteFiles);
            //copyresult.exe
            BozTask copyResultTask = new BozTask("CopyResults.exe", @"-org " + strTempDir + @"\umap -dst "+UnstableAreaDetectionDir+@"\data\matched\train -mode 1 -nStartFrameNo "+BoZConstant.nStartFrameNo+" -nFrameNum " + BoZConstant.ProcessingFrameNum);
            //bozTasks.Add(copyResultTask);
        }

        private void AddDCDTask()
        {
            BozTask dcdTask = new BozTask(UnstableSoftName, " "+ UnstableAreaDetectionDir+ @"\inputparam.csv data 1 1");
            bozTasks.Add(dcdTask);

            BozTask createDirTask = new BozTask("MakeDir", strTempDir + @"\DCDResults");
            bozTasks.Add(createDirTask);

            BozTask moveFiles = new BozTask("moveFiles", UnstableAreaDetectionDir + @"\data\output\train\*.csv@"+strTempDir+ @"\DCDResults");
            bozTasks.Add(moveFiles);
        }

        private void AddLRU3DTask()
        {
            BozTask delFile = new BozTask("DelFiles", strLRUResultsCsvFilename);
            bozTasks.Add(delFile);

            BozTask createDirTask = new BozTask("MakeDir", strSaveRecogResultDir);
            bozTasks.Add(createDirTask);

            BozTask iniWriterTask = new BozTask("IniWriter.exe", @"Param_3DPos.ini -OutCsvFilename "+ strLRUResultsCsvFilename+" -LearningResultDir "+strTempDir+ @"\DCDResults" + " -RawImageDataDir "+ CaptureRawDataDir + " -SaveDataPath "+ strSaveRecogResultDir + " -DisparityTableDir "+ strTempDir + @"\table" +" -UmapImgDir "+strTempDir + @"\umap"+" -OutCsvFlag 1 -LearningScoreMinTh 2.0");
            bozTasks.Add(iniWriterTask);

            BozTask LRU3DTask = new BozTask("LRU3D.exe","");
            bozTasks.Add(LRU3DTask);
        }

        private void AddHeatmapTask()
        {
            BozTask BoZ1CTask = new BozTask("BoZ1C.exe", strLRUResultsCsvFilename+@" "+ CaptureRawDataDir +@"\timestamp.csv " + strTempDir +@"\heatmap_"+BoZConstant.nStartFrameNo+@"_"+BoZConstant.ProcessingFrameNum+".csv " + cameraSettingPage.CameraLocationLongitude+" "+cameraSettingPage.CameraLocationLatitude+" "+cameraSettingPage.TargetLocationLongitude+" "+cameraSettingPage.TargetLocationLatitude);
            bozTasks.Add(BoZ1CTask);
        }

        private void ProgressStatus(object sender, ProgressChangedEventArgs e)
        {
            string msg = e.UserState.ToString();
            processingPage.ProcessingLabel.Content = msg;
        }

        private void CallFinished(object sender, RunWorkerCompletedEventArgs e)
        {
            processingPage.ProcessingPageOKBtn.IsEnabled = true;
            processingPage.ProcessingLabel.Content = "Done";
        }

        private void RunBoZTasks(object sender, DoWorkEventArgs e)
        {
            if(bozTasks == null)
            {
                return;
            }
            foreach(var task in bozTasks)
            {
                //bgWorker.ReportProgress(i, message);
                worker.ReportProgress(0, task.exeFileName);
                switch (task.exeFileName)
                {
                    case "MakeDir": // make Directory
                        {
                            DirectoryInfo directoryCreation = new DirectoryInfo(task.exeParaStrs);
                            if (!directoryCreation.Exists)
                            {
                                directoryCreation.Create();
                            }
                        }
                        break;
                    case "DelFiles":// delete files identitied by *
                        {
                            string target = task.exeParaStrs;
                            if (target.Contains(@"*"))
                            {
                                string[] strs = target.Split(new string[] { "*" }, StringSplitOptions.RemoveEmptyEntries);
                                string splitdirectory = strs[0];
                                string splitfilepattern = strs[strs.Length - 1];
                                string pattern = @"*" + splitfilepattern;
                                string[] strFileName = Directory.GetFiles(splitdirectory, pattern);
                                foreach (var item in strFileName)
                                {
                                    worker.ReportProgress(1, "Delete File" + item);
                                    File.Delete(item);
                                }
                            }
                            else
                            {
                                FileInfo fileDelete = new FileInfo(task.exeParaStrs);
                                if (fileDelete.Exists)
                                {
                                    worker.ReportProgress(1, "Delete File" + fileDelete.Name);
                                    fileDelete.Delete();
                                }
                            }
                        }
                        break;
                    case "moveFiles": // move a file to Directory identitied by space
                        {
                            if(task.exeParaStrs.Contains("@"))
                            {
                                string[] paraStrs = task.exeParaStrs.Split(new string[] { @"@" }, StringSplitOptions.RemoveEmptyEntries);
                                List<string> srcFileList = new List<string>();
                                if (paraStrs[0].Contains(@"*"))
                                {
                                    string[] strs = paraStrs[0].Split(new string[] { "*" }, StringSplitOptions.RemoveEmptyEntries);
                                    string splitdirectory = strs[0];
                                    string splitfilepattern = strs[strs.Length - 1];
                                    string pattern = @"*" + splitfilepattern;
                                    string[] strFileName = Directory.GetFiles(splitdirectory, pattern);
                                    foreach (var item in strFileName)
                                    {
                                        srcFileList.Add(item);
                                    }
                                }
                                else
                                {
                                    srcFileList.Add(paraStrs[0]);
                                }


                                DirectoryInfo targetDirectory = new DirectoryInfo(paraStrs[1]);
                                if (!targetDirectory.Exists)
                                {
                                    targetDirectory.Create();
                                }

                                foreach(var srcFile in srcFileList)
                                {
                                    FileInfo info = new FileInfo(srcFile);
                                    worker.ReportProgress(1, "Move File" + info.Name);
                                    File.Move(info.FullName, System.IO.Path.Combine(targetDirectory.FullName,info.Name));
                                }
                            }
                        }
                        break;
                    default: // run kinds of exe
                        {
                            FileInfo info = new FileInfo(task.exeFileName);
                            if (!info.Exists)
                            {
                                Debug.WriteLine(task.exeFileName + " Not Found");
                                continue;
                            }
                            ProcessStartInfo processInfo = new ProcessStartInfo(info.FullName, @" " + task.exeParaStrs);
                            processInfo.CreateNoWindow = false;
                            processInfo.UseShellExecute = false;
                            
                            processInfo.WindowStyle = ProcessWindowStyle.Normal;
                            //processInfo.RedirectStandardOutput = true;
                            Process taskProcess = Process.Start(processInfo);
                            taskProcess.WaitForExit();

                        }
                        break;
                }
            }

        }

        //         private void CallIniWriterToWriteParameters(string fileName, Dictionary<string, string> paras)
        //         {
        //             string parastring = " ";
        //             foreach(var item in paras)
        //             {
        //                 parastring += " -" + item.Key.ToString() + " "+item.Value.ToString();
        //             }
        //             ProcessStartInfo processInfo = new ProcessStartInfo("IniWriter.exe", fileName + parastring);
        //             processInfo.CreateNoWindow = false;
        //             processInfo.UseShellExecute = false;
        //             processInfo.WindowStyle = ProcessWindowStyle.Hidden;
        //             processInfo.RedirectStandardOutput = true;
        // 
        //             Process iniWriteProcess = Process.Start(processInfo);
        //             iniWriteProcess.WaitForExit();
        //             //返回值为 0
        //             StreamReader reader = iniWriteProcess.StandardOutput;
        //             string outputstring = reader.ReadToEnd();
        // 
        //         }
        // 
        //         private void CallBirdsView()
        //         {
        //             //Set BirdView.ini
        //             Dictionary<string, string> birdsPara = new Dictionary<string, string>();
        //             birdsPara.Add(BoZConstant.BirdViewConfigPara_CameraHeight, cameraSettingPage.CameraHeight);
        //             birdsPara.Add(BoZConstant.BirdViewConfigPara_CameraPitch, cameraSettingPage.CameraAngle);
        //             birdsPara.Add(BoZConstant.BirdViewConfigPara_StartFrameNo, BoZConstant.nStartFrameNo);
        //             birdsPara.Add(BoZConstant.BirdViewConfigPara_ProcessingFrameNum, BoZConstant.ProcessingFrameNum);
        //             birdsPara.Add(BoZConstant.BirdViewConfigPara_ObjInfo, "1");
        //             birdsPara.Add(BoZConstant.BirdViewConfigPara_SaveDataPath, strTempDir);
        //             birdsPara.Add(BoZConstant.BirdViewConfigPara_FrameSkipNum, "1");
        //             CallIniWriterToWriteParameters(BoZConstant.BirdViewConfigFile, birdsPara);
        // 
        //             if (advancedSettingPage.IsBirdsViewCreation == true)
        //             {
        //                 ProcessStartInfo processInfo = new ProcessStartInfo("BirdsView.exe", @" " + CaptureRawDataDir);
        //                 processInfo.CreateNoWindow = false;
        //                 processInfo.UseShellExecute = false;
        //                 processInfo.WindowStyle = ProcessWindowStyle.Hidden;
        //                 processInfo.RedirectStandardOutput = true;
        //                 Process birdViewProcess = Process.Start(processInfo);
        //                 birdViewProcess.WaitForExit();
        // 
        //                 string path = System.IO.Path.Combine(UnstableAreaDetectionDir, "data", "matched", "matched");
        //                 string pattern = "*.png";
        //                 string[] strFileName = Directory.GetFiles(path, pattern);
        //                 foreach (var item in strFileName)
        //                 {
        //                     File.Delete(item);
        //                 }
        //                 string copyresultExePara = @" -org "+ strTempDir + @"\umap -dst "+UnstableAreaDetectionDir +@"\data\matched\train -mode 1 -nStartFrameNo " + BoZConstant.nStartFrameNo +@" -nFrameNum "+BoZConstant.ProcessingFrameNum;
        //                 ProcessStartInfo processCopyResultsInfo = new ProcessStartInfo(BoZConstant.CopyResultsExe, @" " + CaptureRawDataDir);
        //                 processInfo.CreateNoWindow = false;
        //                 processInfo.UseShellExecute = false;
        // 
        //             }
        // 
        //         }

    }
}
