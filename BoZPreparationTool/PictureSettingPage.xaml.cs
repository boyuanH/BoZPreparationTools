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
    /// PictureSettingPage.xaml 的交互逻辑
    /// </summary>
    /// 

    public delegate void PictureSettingPageAdvancedBtnClickDelegate(object sender, EventArgs e);
    public delegate void PictureSettingPageOkBtnClickDelegate(object sender, EventArgs e);
    public delegate void PictureSettingPageBackBtnClickDelegate(object sender, EventArgs e);
    public partial class PictureSettingPage : System.Windows.Controls.UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event PictureSettingPageAdvancedBtnClickDelegate PictureSettingPageAdvancedBtnClickEvent;
        public event PictureSettingPageBackBtnClickDelegate PictureSettingPageBackBtnClickEvent;
        public event PictureSettingPageOkBtnClickDelegate PictureSettingPageOkBtnClickEvent;

        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        //public string CaptureRawDataDir = @"D:\Work\RICOH\BoZ\BoZData\Data\20170627091244S";
        public string CaptureRawDataDir = @"";

        private string pictureFolder;
        public string PictureFolder
        {
            get
            {
                return pictureFolder;
            }
            set
            {
                pictureFolder = value;
                OnPropertyChanged("PictureFolder");
            }
        }

        private DateTime startDateTime;
        

        private string startHour;
        public string StartHour
        {
            get
            {
                return startHour;
            }
            set
            {
                startHour = value;
                OnPropertyChanged("StartHour");
            }
        }

        private string startMinute;
        public string StartMinute
        {
            get
            {
                return startMinute;
            }
            set
            {
                startMinute = value;
                OnPropertyChanged("StartMinute");
            }
        }

        private string startSecond;
        public string StartSecond
        {
            get
            {
                return startSecond;
            }
            set
            {
                startSecond = value;
                OnPropertyChanged("StartSecond");
            }
        }

        private DateTime endDateTime;

        private string endHour;
        public string EndHour
        {
            get
            {
                return endHour;
            }
            set
            {
                endHour = value;
                OnPropertyChanged("EndHour");
            }
        }

        private string endMinute;
        public string EndMinute
        {
            get
            {
                return endMinute;
            }
            set
            {
                endMinute = value;
                OnPropertyChanged("EndMinute");
            }
        }

        private string endSecond;
        public string EndSecond
        {
            get
            {
                return endSecond;
            }
            set
            {
                endSecond = value;
                OnPropertyChanged("EndSecond");
            }
        }





        public PictureSettingPage()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void PictureSettingPageAdvancedBtn_Click(object sender, RoutedEventArgs e)
        {
            if(PictureSettingPageAdvancedBtnClickEvent != null)
            {
                PictureSettingPageAdvancedBtnClickEvent(this, e);
            }
        }

        private void PictureSettingPageBackBtn_Click(object sender, RoutedEventArgs e)
        {
            if (PictureSettingPageBackBtnClickEvent != null)
            {
                PictureSettingPageBackBtnClickEvent(this, e);
            }
        }

        private void PictureSettingPageOkBtn_Click(object sender, RoutedEventArgs e)
        {
            if (PictureSettingPageOkBtnClickEvent != null)
            {
                PictureSettingPageOkBtnClickEvent(this, e);
            }
        }

        private void PictureSettingPageFictureFolderSelectBtn_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folder_dialog = new FolderBrowserDialog();
            DialogResult result = folder_dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }
            PictureFolder = folder_dialog.SelectedPath.Trim();
        }
        
        private void PictureSettingPageFictureFolderSetBtn_Click(object sender, RoutedEventArgs e)
        {
            string path = PictureSettingPagePictureFolderTextBox.Text.Trim();
            if (!string.IsNullOrWhiteSpace(path))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(path);
                if (directoryInfo.Exists)
                {
                    pictureFolder = directoryInfo.FullName;
                    CaptureRawDataDir = pictureFolder;
                    System.Windows.MessageBox.Show("Picture Folder Set","BoZ Info");
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("Please Input Correct Directory Path");
                }
            }
        }
    }
}
