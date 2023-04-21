
using AForge.Video;
using AForge.Video.FFMPEG;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Xml.Linq;

namespace AForge.Net.Streams
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //string url = "https://admin:pXmvbc12vX8@192.168.20.110/streamer/stream";
        string url = "https://192.168.20.110/streamer/stream";


        //string url = "http://192.168.100.7:8000/camera/mjpeg";

        ConcurrentDictionary<MGpegStreamExtended, System.Windows.Controls.Image> elements = new ConcurrentDictionary<MGpegStreamExtended, System.Windows.Controls.Image>();
        public MainWindow()
        {
            InitializeComponent();

            foreach (var element in Wrapper.Children)
            {
                var imageControl = ((element as Grid).Children[0] as System.Windows.Controls.Image);
                var videoSource = new MGpegStreamExtended(url);

                videoSource.ForceBasicAuthentication = true;
                videoSource.Login = "admin";
                videoSource.Password = "pXmvbc12vX8";

                videoSource.NewFrame += new NewFrameEventHandler(Video_NewFrame);

                videoSource.VideoSourceError += VideoSource_VideoSourceError;

                Task.Run(() =>
                {
                    videoSource.Start();
                    elements.AddOrUpdate(videoSource, imageControl, (key, oldValue) => oldValue);
                });


              

            }



        }

        private void VideoSource_VideoSourceError(object sender, VideoSourceErrorEventArgs eventArgs)
        {
            //throw new NotImplementedException();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {


        }

        private void Av_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {

        }

        public void Video_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                System.Drawing.Image img = (Bitmap)eventArgs.Frame.Clone();

                MemoryStream ms = new MemoryStream();
                img.Save(ms, ImageFormat.Bmp);
                ms.Seek(0, SeekOrigin.Begin);
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.StreamSource = ms;
                bi.EndInit();

                bi.Freeze();
                Dispatcher.BeginInvoke(new ThreadStart(delegate
                {
                    elements[sender as MGpegStreamExtended].Source = bi;
                    //frameHolder.Source = bi;
                }));
            }
            catch (Exception ex)
            {
            }
        }
    }
}
