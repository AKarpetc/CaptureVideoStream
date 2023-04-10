using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CaptureVideoStream
{
    public partial class Form1 : Form
    {


        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll")]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        Dictionary<string, Process> ffplays = new Dictionary<string, Process>();

        public Form1()
        {
            InitializeComponent();
            Application.EnableVisualStyles();
            this.DoubleBuffered = true;
        }

        private void ffplayPlay(string source)
        {
            try
            {
                var plainTextBytes = System.Text.Encoding.UTF8.GetBytes("backend:NzM3yMjIwOTdlYz1dmYj");
                var auth = System.Convert.ToBase64String(plainTextBytes);

                var ffplay = new Process
                {
                    StartInfo =
                            {
                                FileName = "ffplay.exe",
                                Arguments = $"-headers \"Authorization: Basic ${auth}\" ${source}",
                               // Arguments=source,
                                CreateNoWindow = true,
                                RedirectStandardError = true,
                                RedirectStandardOutput = true,
                                UseShellExecute = false,
                               // RedirectStandardInput= true,

                            }
                };

                ffplay.EnableRaisingEvents = true;
                ffplay.OutputDataReceived += (o, e) => Debug.WriteLine(e.Data ?? "NULL", "ffplay");
                ffplay.ErrorDataReceived += (o, e) => MessageBox.Show(e.Data);
                ffplay.Exited += (o, e) => Debug.WriteLine("Exited", "ffplay");

                ffplay.Start();

               // Thread.Sleep(200); // you need to wait/check the process started, then...

                // child, new parent
                // make 'this' the parent of ffmpeg (presuming you are in scope of a Form or Control)
                // SetParent(ffplay.MainWindowHandle, this.Handle);

                // window, x, y, width, height, repaint
                // move the ffplayer window to the top-left corner and set the size to 320x280
                // MoveWindow(ffplay.MainWindowHandle, 0, 0, 320, 280, true);

                ffplays.Add(source, ffplay);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }


        }

        private void button1_Click(object sender, EventArgs e)
        {
            var source = listBox1?.SelectedItem?.ToString();

            if (source == null)
            {
                MessageBox.Show("Select the source of streaming");
                return;
            }

            if (!ffplays.ContainsKey(source))
            {
                ffplayPlay(source);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var source = listBox1.SelectedItem.ToString();

            if (ffplays.ContainsKey(source))
            {
                ffplays[source].Kill();
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            foreach (var process in ffplays)
            {
                process.Value?.Kill();
            }
        }

   
            
            
    }
}
