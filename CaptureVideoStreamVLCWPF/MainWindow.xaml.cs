using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Vlc.DotNet.Wpf;

namespace CaptureVideoStreamVLCWPF
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var currentAssembly = Assembly.GetEntryAssembly();
            var currentDirectory = new FileInfo(currentAssembly.Location).DirectoryName;

            var vlcLibDirectory = new DirectoryInfo(Path.Combine(currentDirectory, "libvlc", IntPtr.Size == 4 ? "win-x86" : "win-x64"));

            var options = new string[]
            {
                // VLC options can be given here. Please refer to the VLC command line documentation.
            };

            foreach (var control in VLCs.Children)
            {
                var vlcControl = ((control as Grid).Children[0] as VlcControl);

                vlcControl.SourceProvider.IsAlphaChannelEnabled = false;


                vlcControl.SourceProvider.CreatePlayer(vlcLibDirectory, options);
            }

            // Load libvlc libraries and initializes stuff. It is important that the options (if you want to pass any) and lib directory are given before calling this method.

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {



        }

        private void WrapPanel_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var control in VLCs.Children)
            {

                var player = ((control as Grid).Children[0] as VlcControl)
                .SourceProvider.MediaPlayer;
                player.SetMedia("https://admin:pXmvbc12vX8@192.168.20.110/streamer/stream", "--file-caching=3000", "--no-fullscreen", "--ignore-config");

                player.Play();

            }
        }
    }
}
