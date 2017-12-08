using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
            //Check Frame


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
                AddDCDTask();
            }

            if (advancedSettingPage.IsObjectDetection == true)
            {
                AddLRU3DTask();
            }

            if(advancedSettingPage.IsHeapMapCreation == true)
            {
                AddHeatmapTask();
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

        private DateTime getDateTime(string[] timeDefine)
        {
            DateTime dateTime = DateTime.MinValue;
            if(timeDefine == null)
            {
                return dateTime;
            }
            if(timeDefine.Length != 7)
            {
                return dateTime;
            }

            try
            {
                dateTime = new DateTime(Convert.ToInt32(timeDefine[0]), Convert.ToInt32(timeDefine[1]), Convert.ToInt32(timeDefine[2]), Convert.ToInt32(timeDefine[3]), Convert.ToInt32(timeDefine[4]), Convert.ToInt32(timeDefine[5]), Convert.ToInt32(timeDefine[6]));
            }
            catch (ArgumentOutOfRangeException )
            {
                MessageBox.Show("Please input currect DateTime");
                return DateTime.MinValue;
            }
            catch (FormatException )
            {
                MessageBox.Show("Please input currect Number of DateTime");
                return DateTime.MinValue;
            }
            return dateTime;
        }

        private bool SetFrameParameter()
        {
            DateTime startDateTime = getDateTime(new string[] { "2007", "6", "27", pictureSettingPage.StartHour, pictureSettingPage.StartMinute, pictureSettingPage.StartSecond, "000" });
            if(startDateTime == DateTime.MinValue)
            {
                return false;
            }
            DateTime endDateTime = getDateTime(new string[] { "2017", "6", "27", pictureSettingPage.EndHour, pictureSettingPage.EndMinute, pictureSettingPage.EndSecond, "999" });
            if(endDateTime == DateTime.MinValue)
            {
                return false;
            }
            if (startDateTime >= endDateTime)
            {
                MessageBox.Show("Please input currect DateTime");
                return false;
            }
            //check file
            FileInfo timestampFileInfo = new FileInfo(System.IO.Path.Combine(CaptureRawDataDir, BoZConstant.FrameCSVFileName));
            if (!timestampFileInfo.Exists)
            { 
                MessageBox.Show(BoZConstant.FrameCSVFileName + " File Not found");
                return false;
            }

            string[] frameList = getFrameConfigFromCSVFile(timestampFileInfo.FullName, startDateTime, endDateTime);
            //readFile
            if(frameList == null)
            {
                BoZConstant.nStartFrameNo = "";
                BoZConstant.ProcessingFrameNum = "";
                return false;
            }

            BoZConstant.nStartFrameNo = frameList[0];
            BoZConstant.ProcessingFrameNum = frameList[1];

            return true;
        }

        private string[] getFrameConfigFromCSVFile(string filePath,DateTime startTime,DateTime endTime)
        {
            string[] frameList = null;
            string startFrame = "";
            string endFrame = "";

            FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            StreamReader streamReader = new StreamReader(fileStream);
            string strLine = "";
            string[] aryLine = null;

            while(((strLine = streamReader.ReadLine()) != null) && (!string.IsNullOrWhiteSpace(strLine)))
            {
                aryLine = strLine.Split(',');
                DateTime currentTime = new DateTime(Convert.ToInt32(aryLine[1]), Convert.ToInt32(aryLine[2]), Convert.ToInt32(aryLine[3]), Convert.ToInt32(aryLine[4]), Convert.ToInt32(aryLine[5]), Convert.ToInt32(aryLine[6]), Convert.ToInt32(aryLine[7]));
                if (currentTime < startTime)
                {
                    continue;
                }else if(startTime <= currentTime && currentTime <= endTime)
                {
                    if (string.IsNullOrWhiteSpace(startFrame))
                    {
                        startFrame = aryLine[0];
                    }
                    else
                    {
                        endFrame = aryLine[0];
                    }
                }
                else
                {
                    break;
                }
            }

            if((!string.IsNullOrWhiteSpace(startFrame)) && (!string.IsNullOrWhiteSpace(endFrame)))
            {
                frameList = new string[] { startFrame, endFrame };
            }

            return frameList;
        }

        private int getDateOrTimeFromString(string str)
        {
            int result = -1;
            if (string.IsNullOrWhiteSpace(str))
            {
                MessageBox.Show("Please input currect DateTime");
                return result;
            }

            try
            {
                result = Convert.ToInt32(str);
            }catch(Exception )
            {
                result = -1;
            }
            return result;
        }



    }
}
