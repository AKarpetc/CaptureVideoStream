using AForge.Video;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AForge.Net.Streams
{
    public class MGpegStreamExtended
    {

        private const int bufSize = 1048576;

        private const int readSize = 1024;

        private string source;

        private string login;

        private string password;

        private IWebProxy proxy;

        private int framesReceived;

        private long bytesReceived;

        private bool useSeparateConnectionGroup = true;

        private int requestTimeout = 10000;

        private bool forceBasicAuthentication;

        private Thread thread;

        private ManualResetEvent stopEvent;

        private ManualResetEvent reloadEvent;

        private string userAgent = "Mozilla/5.0";

        //
        // Сводка:
        //     Use or not separate connection group.
        //
        // Примечания:
        //     The property indicates to open web request in separate connection group.
        public bool SeparateConnectionGroup
        {
            get
            {
                return useSeparateConnectionGroup;
            }
            set
            {
                useSeparateConnectionGroup = value;
            }
        }

        //
        // Сводка:
        //     Video source.
        //
        // Примечания:
        //     URL, which provides MJPEG stream.
        public string Source
        {
            get
            {
                return source;
            }
            set
            {
                source = value;
                if (thread != null)
                {
                    reloadEvent.Set();
                }
            }
        }

        //
        // Сводка:
        //     Login value.
        //
        // Примечания:
        //     Login required to access video source.
        public string Login
        {
            get
            {
                return login;
            }
            set
            {
                login = value;
            }
        }

        //
        // Сводка:
        //     Password value.
        //
        // Примечания:
        //     Password required to access video source.
        public string Password
        {
            get
            {
                return password;
            }
            set
            {
                password = value;
            }
        }

        //
        // Сводка:
        //     Gets or sets proxy information for the request.
        //
        // Примечания:
        //     The local computer or application config file may specify that a default proxy
        //     to be used. If the Proxy property is specified, then the proxy settings from
        //     the Proxy property overridea the local computer or application config file and
        //     the instance will use the proxy settings specified. If no proxy is specified
        //     in a config file and the Proxy property is unspecified, the request uses the
        //     proxy settings inherited from Internet Explorer on the local computer. If there
        //     are no proxy settings in Internet Explorer, the request is sent directly to the
        //     server.
        public IWebProxy Proxy
        {
            get
            {
                return proxy;
            }
            set
            {
                proxy = value;
            }
        }

        //
        // Сводка:
        //     User agent to specify in HTTP request header.
        //
        // Примечания:
        //     Some IP cameras check what is the requesting user agent and depending on it they
        //     provide video in different formats or do not provide it at all. The property
        //     sets the value of user agent string, which is sent to camera in request header.
        //     Default value is set to "Mozilla/5.0". If the value is set to null, the user
        //     agent string is not sent in request header.
        public string HttpUserAgent
        {
            get
            {
                return userAgent;
            }
            set
            {
                userAgent = value;
            }
        }

        //
        // Сводка:
        //     Received frames count.
        //
        // Примечания:
        //     Number of frames the video source provided from the moment of the last access
        //     to the property.
        public int FramesReceived
        {
            get
            {
                int result = framesReceived;
                framesReceived = 0;
                return result;
            }
        }

        //
        // Сводка:
        //     Received bytes count.
        //
        // Примечания:
        //     Number of bytes the video source provided from the moment of the last access
        //     to the property.
        public long BytesReceived
        {
            get
            {
                long result = bytesReceived;
                bytesReceived = 0L;
                return result;
            }
        }

        //
        // Сводка:
        //     Request timeout value.
        //
        // Примечания:
        //     The property sets timeout value in milliseconds for web requests. Default value
        //     is 10000 milliseconds.
        public int RequestTimeout
        {
            get
            {
                return requestTimeout;
            }
            set
            {
                requestTimeout = value;
            }
        }

        //
        // Сводка:
        //     State of the video source.
        //
        // Примечания:
        //     Current state of video source object - running or not.
        public bool IsRunning
        {
            get
            {
                if (thread != null)
                {
                    if (!thread.Join(0))
                    {
                        return true;
                    }

                    Free();
                }

                return false;
            }
        }

        //
        // Сводка:
        //     Force using of basic authentication when connecting to the video source.
        //
        // Примечания:
        //     For some IP cameras (TrendNET IP cameras, for example) using standard .NET's
        //     authentication via credentials does not seem to be working (seems like camera
        //     does not request for authentication, but expects corresponding headers to be
        //     present on connection request). So this property allows to force basic authentication
        //     by adding required HTTP headers when request is sent.
        //     Default value is set to false.
        public bool ForceBasicAuthentication
        {
            get
            {
                return forceBasicAuthentication;
            }
            set
            {
                forceBasicAuthentication = value;
            }
        }

        //
        // Сводка:
        //     New frame event.
        //
        // Примечания:
        //     Notifies clients about new available frame from video source.
        //     Since video source may have multiple clients, each client is responsible for
        //     making a copy (cloning) of the passed video frame, because the video source disposes
        //     its own original copy after notifying of clients.
        public event NewFrameEventHandler NewFrame;

        //
        // Сводка:
        //     Video source error event.
        //
        // Примечания:
        //     This event is used to notify clients about any type of errors occurred in video
        //     source object, for example internal exceptions.
        public event VideoSourceErrorEventHandler VideoSourceError;

        //
        // Сводка:
        //     Video playing finished event.
        //
        // Примечания:
        //     This event is used to notify clients that the video playing has finished.
        public event PlayingFinishedEventHandler PlayingFinished;

        //
        // Сводка:
        //     Initializes a new instance of the AForge.Video.MJPEGStream class.
        public MGpegStreamExtended()
        {
        }

        //
        // Сводка:
        //     Initializes a new instance of the AForge.Video.MJPEGStream class.
        //
        // Параметры:
        //   source:
        //     URL, which provides MJPEG stream.
        public MGpegStreamExtended(string source)
        {
            this.source = source;
        }

        //
        // Сводка:
        //     Start video source.
        //
        // Исключения:
        //   T:System.ArgumentException:
        //     Video source is not specified.
        //
        // Примечания:
        //     Starts video source and return execution to caller. Video source object creates
        //     background thread and notifies about new frames with the help of AForge.Video.MJPEGStream.NewFrame
        //     event.
        public void Start()
        {
            if (!IsRunning)
            {
                if (source == null || source == string.Empty)
                {
                    throw new ArgumentException("Video source is not specified.");
                }

                framesReceived = 0;
                bytesReceived = 0L;
                stopEvent = new ManualResetEvent(initialState: false);
                reloadEvent = new ManualResetEvent(initialState: false);
                thread = new Thread(WorkerThread);
                thread.Name = source;
                thread.Start();
            }
        }

        //
        // Сводка:
        //     Signal video source to stop its work.
        //
        // Примечания:
        //     Signals video source to stop its background thread, stop to provide new frames
        //     and free resources.
        public void SignalToStop()
        {
            if (thread != null)
            {
                stopEvent.Set();
            }
        }

        //
        // Сводка:
        //     Wait for video source has stopped.
        //
        // Примечания:
        //     Waits for source stopping after it was signalled to stop using AForge.Video.MJPEGStream.SignalToStop
        //     method.
        public void WaitForStop()
        {
            if (thread != null)
            {
                thread.Join();
                Free();
            }
        }

        //
        // Сводка:
        //     Stop video source.
        //
        // Примечания:
        //     Stops video source aborting its thread.
        //     Since the method aborts background thread, its usage is highly not preferred
        //     and should be done only if there are no other options. The correct way of stopping
        //     camera is signaling it stop and then waiting for background thread's completion.
        public void Stop()
        {
            if (IsRunning)
            {
                stopEvent.Set();
                thread.Abort();
                WaitForStop();
            }
        }

        //
        // Сводка:
        //     Free resource.
        private void Free()
        {
            thread = null;
            stopEvent.Close();
            stopEvent = null;
            reloadEvent.Close();
            reloadEvent = null;
        }

        private void WorkerThread()
        {
            byte[] array = new byte[1048576];
            byte[] array2 = new byte[3] { 255, 216, 255 };
            int num = 3;
            ASCIIEncoding aSCIIEncoding = new ASCIIEncoding();
            while (!stopEvent.WaitOne(0, exitContext: false))
            {
                reloadEvent.Reset();
                HttpWebRequest httpWebRequest = null;
                WebResponse webResponse = null;
                Stream stream = null;
                byte[] array3 = null;
                string text = null;
                bool flag = false;
                int num2 = 0;
                int num3 = 0;
                int num4 = 0;
                int num5 = 1;
                int num6 = 0;
                int num7 = 0;
                try
                {
                    httpWebRequest = (HttpWebRequest)WebRequest.Create(source);

                    ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                    if (userAgent != null)
                    {
                        httpWebRequest.UserAgent = userAgent;
                    }

                    if (proxy != null)
                    {
                        httpWebRequest.Proxy = proxy;
                    }

                    httpWebRequest.Timeout = requestTimeout;
                    if (login != null && password != null && login != string.Empty)
                    {
                        httpWebRequest.Credentials = new NetworkCredential(login, password);
                    }

                    if (useSeparateConnectionGroup)
                    {
                        httpWebRequest.ConnectionGroupName = GetHashCode().ToString();
                    }

                    if (forceBasicAuthentication)
                    {
                        string s = $"{login}:{password}";
                        s = Convert.ToBase64String(Encoding.Default.GetBytes(s));
                        httpWebRequest.Headers["Authorization"] = "Basic " + s;
                    }

                    webResponse = httpWebRequest.GetResponse();
                    string contentType = webResponse.ContentType;
                    string[] array4 = contentType.Split('/');
                    int num8;
                    if (array4[0] == "application" && array4[1] == "octet-stream")
                    {
                        num8 = 0;
                        array3 = new byte[0];
                    }
                    else
                    {
                        if (!(array4[0] == "multipart") || !contentType.Contains("mixed"))
                        {
                            throw new Exception("Invalid content type.");
                        }

                        int num9 = contentType.IndexOf("boundary", 0);
                        if (num9 != -1)
                        {
                            num9 = contentType.IndexOf("=", num9 + 8);
                        }

                        if (num9 == -1)
                        {
                            num8 = 0;
                            array3 = new byte[0];
                        }
                        else
                        {
                            text = contentType.Substring(num9 + 1);
                            text = text.Trim(' ', '"');
                            array3 = aSCIIEncoding.GetBytes(text);
                            num8 = array3.Length;
                            flag = false;
                        }
                    }

                    stream = webResponse.GetResponseStream();
                    stream.ReadTimeout = requestTimeout;
                    while (!stopEvent.WaitOne(0, exitContext: false) && !reloadEvent.WaitOne(0, exitContext: false))
                    {
                        if (num3 > 1047552)
                        {
                            num3 = (num4 = (num2 = 0));
                        }

                        int num10;
                        if ((num10 = stream.Read(array, num3, 1024)) == 0)
                        {
                            throw new ApplicationException();
                        }

                        num3 += num10;
                        num2 += num10;
                        bytesReceived += num10;
                        if (num8 != 0 && !flag)
                        {
                            num4 = ByteArrayUtilsNew.Find(array, array3, 0, num2);
                            if (num4 == -1)
                            {
                                continue;
                            }

                            for (int num11 = num4 - 1; num11 >= 0; num11--)
                            {
                                byte b = array[num11];
                                if (b == 10 || b == 13)
                                {
                                    break;
                                }

                                text = (char)b + text;
                            }

                            array3 = aSCIIEncoding.GetBytes(text);
                            num8 = array3.Length;
                            flag = true;
                        }

                        if (num5 == 1 && num2 >= num)
                        {
                            num6 = ByteArrayUtilsNew.Find(array, array2, num4, num2);
                            if (num6 != -1)
                            {
                                num4 = num6 + num;
                                num2 = num3 - num4;
                                num5 = 2;
                            }
                            else
                            {
                                num2 = num - 1;
                                num4 = num3 - num2;
                            }
                        }

                        while (num5 == 2 && num2 != 0 && num2 >= num8)
                        {
                            num7 = ByteArrayUtilsNew.Find(array, (num8 != 0) ? array3 : array2, num4, num2);
                            if (num7 != -1)
                            {
                                num4 = num7;
                                num2 = num3 - num4;
                                framesReceived++;
                                if (this.NewFrame != null && !stopEvent.WaitOne(0, exitContext: false))
                                {
                                    Bitmap bitmap = (Bitmap)Image.FromStream(new MemoryStream(array, num6, num7 - num6));
                                    this.NewFrame(this, new NewFrameEventArgs(bitmap));
                                    bitmap.Dispose();
                                    bitmap = null;
                                }

                                num4 = num7 + num8;
                                num2 = num3 - num4;
                                Array.Copy(array, num4, array, 0, num2);
                                num3 = num2;
                                num4 = 0;
                                num5 = 1;
                            }
                            else if (num8 != 0)
                            {
                                num2 = num8 - 1;
                                num4 = num3 - num2;
                            }
                            else
                            {
                                num2 = 0;
                                num4 = num3;
                            }
                        }
                    }
                }
                catch (ApplicationException)
                {
                    Thread.Sleep(250);
                }
                catch (ThreadAbortException)
                {
                    break;
                }
                catch (Exception ex3)
                {
                    if (this.VideoSourceError != null)
                    {
                        this.VideoSourceError(this, new VideoSourceErrorEventArgs(ex3.Message));
                    }

                    Thread.Sleep(250);
                }
                finally
                {
                    if (httpWebRequest != null)
                    {
                        httpWebRequest.Abort();
                        httpWebRequest = null;
                    }

                    if (stream != null)
                    {
                        stream.Close();
                        stream = null;
                    }

                    if (webResponse != null)
                    {
                        webResponse.Close();
                        webResponse = null;
                    }
                }

                if (stopEvent.WaitOne(0, exitContext: false))
                {
                    break;
                }
            }

            if (this.PlayingFinished != null)
            {
                this.PlayingFinished(this, ReasonToFinishPlaying.StoppedByUser);
            }
        }

    }
}
