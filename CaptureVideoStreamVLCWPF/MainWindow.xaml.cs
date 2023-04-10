using System;
using System.IO;
using System.Reflection;
using System.Windows;

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

            this.MyControl.SourceProvider.CreatePlayer(vlcLibDirectory, options);

            // Load libvlc libraries and initializes stuff. It is important that the options (if you want to pass any) and lib directory are given before calling this method.



        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MyControl.SourceProvider.MediaPlayer.Play(new Uri("https://backend:NzM3yMjIwOTdlYz1dmYj@192.168.20.127/streamer/stream"));

        }
    }
}
