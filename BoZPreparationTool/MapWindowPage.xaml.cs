using GMap.NET;
using GMap.NET.MapProviders;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
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
    /// MapWindowPage.xaml 的交互逻辑
    /// </summary>
    /// 

    public delegate void MapWindowPageCancelBtnClickDelegate(object sender, EventArgs e);
    public delegate void MapWindowPageOkBtnClickDelegate(object sender, EventArgs e);

    public partial class MapWindowPage : UserControl,INotifyPropertyChanged
    {
        public event MapWindowPageCancelBtnClickDelegate MapWindowPageCancelBtnClickEvent;
        public event MapWindowPageOkBtnClickDelegate MapWindowPageOkBtnClickEvent;

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private string mapLongitude;
        public string MapLongitude
        {
            get
            {
                return mapLongitude;
            }
            set
            {
                mapLongitude = value;
                OnPropertyChanged("MapLongitude");
            }
        }

        private string mapLatitude;
        public string MapLatitude
        {
            get
            {
                return mapLatitude;
            }
            set
            {
                mapLatitude = value;
                OnPropertyChanged("MapLatitude");
            }
        }


        public MapWindowPage()
        {
            InitializeComponent();
            try
            {
                System.Net.IPHostEntry e = System.Net.Dns.GetHostEntry("ditu.google.cn");
            }
            catch
            {
                mapControl.Manager.Mode = AccessMode.CacheOnly;
                MessageBox.Show("No internet connection avaible, going to CacheOnly mode.", "GMap.NET Demo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            //GMapProvider.WebProxy = new WebProxy("","");

            //GMapProvider.WebProxy = new WebProxy("proxy.jp.ricoh.com", 8080);
            //GMapProvider.WebProxy.Credentials = new NetworkCredential(@"z00b03149", @"3DAF6R4A8NQy");

            mapControl.MapProvider = GMapProviders.GoogleChinaMap;
            mapControl.MinZoom = 2;  //最小缩放
            mapControl.MaxZoom = 17; //最大缩放
            mapControl.Zoom = 10;     //当前缩放
            mapControl.ShowCenter = false; //不显示中心十字点
            mapControl.DragButton = MouseButton.Left; //左键拖拽地图
            SetCoordinate("0", "0");
            this.DataContext = this;
        }

        public void SetCoordinate(string longitude, string latitude)
        {
            MapLongitude = longitude.ToString();
            MapLatitude = latitude.ToString();
            double longitudeMath = Convert.ToDouble(MapLongitude);
            double latitudeMath = Convert.ToDouble(MapLatitude);

            mapControl.Position = new PointLatLng(longitudeMath, latitudeMath); //地图中心位置：南京

        }

        private void MapWindowPageCancelBtn_Click(object sender, RoutedEventArgs e)
        {
            //MapWindowPageCancelBtnClickEvent?.Invoke(this, e);
            if (MapWindowPageCancelBtnClickEvent != null)
            {
                MapWindowPageCancelBtnClickEvent(this, e);
            }
        }

        private void MapWindowPageOKBtn_Click(object sender, RoutedEventArgs e)
        {
            //MapWindowPageOkBtnClickEvent?.Invoke(this, e);
            if (MapWindowPageOkBtnClickEvent != null)
            {
                MapWindowPageOkBtnClickEvent(this, e);
            }
        }

        private void MapWindowPageZoomInBtn_Click(object sender, RoutedEventArgs e)
        {
            if (mapControl.Zoom == 23)
            {
                MapWindowPageZoomInBtn.IsEnabled = false;
            }
            mapControl.Zoom = mapControl.Zoom + 1;
            MapWindowPageZoomOutBtn.IsEnabled = true;
        }

        private void MapWindowPageZoomOutBtn_Click(object sender, RoutedEventArgs e)
        {
            if (mapControl.Zoom == 2)
            {
                MapWindowPageZoomOutBtn.IsEnabled = false;
            }
            mapControl.Zoom = mapControl.Zoom - 1;
            MapWindowPageZoomInBtn.IsEnabled = true;
        }


    }
}
