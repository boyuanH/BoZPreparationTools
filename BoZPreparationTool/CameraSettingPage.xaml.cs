using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BoZPreparation_Tool
{
    /// <summary>
    /// CameraSettingPage.xaml 的交互逻辑
    /// </summary>
    /// 
    public delegate void CameraSettingPageNextBtnClickDelegate(object sender, EventArgs e);
    public delegate void CameraSettingPageShowMapBtnClickDelegate(object sender, RoutedEventArgs e);



    public partial class CameraSettingPage : System.Windows.Controls.UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event CameraSettingPageNextBtnClickDelegate CameraSettingPageNextBtnClickEvent;
        public event CameraSettingPageShowMapBtnClickDelegate CameraSettingPageShowMapBtnClickEvent;


        public void OnPropertyChanged(string propertyName)
        {
            //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private string cameraLocationLongitude = "";
        public string CameraLocationLongitude
        {
            get
            {
                return cameraLocationLongitude;
            }
            set
            {
                cameraLocationLongitude = value;
                OnPropertyChanged("CameraLocationLongitude");
            }
        }

        private string cameraLocationLatitude = "";
        public string CameraLocationLatitude
        {
            get
            {
                return cameraLocationLatitude;
            }
            set
            {
                cameraLocationLatitude = value;
                OnPropertyChanged("CameraLocationLatitude");
            }
        }

        private string targetLocationLongitude = "";
        public string TargetLocationLongitude
        {
            get
            {
                return targetLocationLongitude;
            }
            set
            {
                targetLocationLongitude = value;
                OnPropertyChanged("TargetLocationLongitude");
            }
        }

        private string targetLocationLatitude = "";
        public string TargetLocationLatitude
        {
            get
            {
                return targetLocationLatitude;
            }
            set
            {
                targetLocationLatitude = value;
                OnPropertyChanged("TargetLocationLatitude");
            }
        }

        private string cameraSettingFilePath;
        public string CameraSettingFilePath
        {
            get
            {
                return cameraSettingFilePath;
            }
            set
            {
                cameraSettingFilePath = value;
                OnPropertyChanged("CameraSettingFilePath");
            }
        }

        private string cameraHeight = "";
        public string CameraHeight 
        {
            get
            {
                return cameraHeight;
            }
            set
            {
                cameraHeight = value;
                OnPropertyChanged("CameraHeight");
            }
        }

        private string cameraAngle = "";
        public string CameraAngle
        {
            get
            {
                return cameraAngle;
            }
            set
            {
                cameraAngle = value;
                OnPropertyChanged("CameraAngle");
            }
        }

        public string currentLongitude;
        public string currentLatitude;

        public CameraSettingPage()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void CameraSettingPageNextBtn_Click(object sender, RoutedEventArgs e)
        {
            //CameraSettingPageNextBtnClickEvent?.Invoke(this, e);
            if (CameraSettingPageNextBtnClickEvent != null)
            {
                CameraSettingPageNextBtnClickEvent(this, e);
            }
        }

        private void CameraSettingPageBrowerBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            string fileInfo = @".\";
            DirectoryInfo dInfo = new DirectoryInfo(fileInfo);
            fileDialog.InitialDirectory = dInfo.FullName;
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                CameraSettingFilePath = fileDialog.FileName.Trim();
            }
        }   

        private void CameraSettingPageSettingFileLoadBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(CameraSettingFilePath))
            {
                FileInfo fileInfo = new FileInfo(CameraSettingFilePath);
                if (fileInfo.Exists)
                {
                    IniFiles iniFile = new IniFiles(fileInfo.FullName);
                    CameraLocationLongitude = iniFile.IniReadValue("CameraSetting", "CameraLocationLongitude");
                    CameraLocationLatitude = iniFile.IniReadValue("CameraSetting", "CameraLocationLatitude");
                    TargetLocationLongitude = iniFile.IniReadValue("CameraSetting", "TargetLocationLongitude");
                    TargetLocationLatitude = iniFile.IniReadValue("CameraSetting", "TargetLocationLatitude");
                    CameraHeight = iniFile.IniReadValue("CameraSetting", "CameraHeight");
                    CameraAngle = iniFile.IniReadValue("CameraSetting", "CameraAngle");
                }
            }
        }

        private void CameraSettingPageSettingFileSaveBtn_Click(object sender, RoutedEventArgs e)
        {
            string filePath = BoZConstant.DefaultCameraSettingFileName;
            if (string.IsNullOrWhiteSpace(CameraSettingFilePath) == false)
            {
                filePath = CameraSettingFilePath;
            }

            FileInfo fileInfo = new FileInfo(filePath);
            if (!fileInfo.Exists)
            {
                fileInfo.Create();
            }
            IniFiles iniFile = new IniFiles(fileInfo.FullName);
            CameraLocationLongitude = iniFile.IniReadValue("CameraSetting", "CameraLocationLongitude");
            CameraLocationLatitude = iniFile.IniReadValue("CameraSetting", "CameraLocationLatitude");
            TargetLocationLongitude = iniFile.IniReadValue("CameraSetting", "TargetLocationLongitude");
            TargetLocationLatitude = iniFile.IniReadValue("CameraSetting", "TargetLocationLatitude");
            CameraHeight = iniFile.IniReadValue("CameraSetting", "CameraHeight");
            CameraAngle = iniFile.IniReadValue("CameraSetting", "CameraAngle");
        }

        private void CameraSettingPageCameraLocationShowmapBtn_Click(object sender, RoutedEventArgs e)
        {

            currentLongitude = CameraLocationLongitude;
            currentLatitude = CameraLocationLatitude;
            //CameraSettingPageShowMapBtnClickEvent?.Invoke(this, e);
            if(CameraSettingPageShowMapBtnClickEvent != null)
            {
                CameraSettingPageShowMapBtnClickEvent(this,e);
            }
        }

        private void CameraSettingPageTargetLocationShowmapBtn_Click(object sender, RoutedEventArgs e)
        {
            currentLongitude = TargetLocationLongitude;
            currentLatitude = TargetLocationLatitude;
            //CameraSettingPageShowMapBtnClickEvent?.Invoke(this, e);
            if (CameraSettingPageShowMapBtnClickEvent != null)
            {
                CameraSettingPageShowMapBtnClickEvent(this, e);
            }
        }
    }
}
