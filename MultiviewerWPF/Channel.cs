using System.Xml.Linq;
using System.Runtime.InteropServices;
using System;
using System.Threading;
using System.Diagnostics;
using System.Windows.Threading;


namespace MultiviewerWPF
{
    public class Channel
    {
        public int number;
        public string name;
        private string link;
        public int x;
        public int y;
        private bool widescreen;
        private bool enable;
        private IntPtr winHNDL;
        private DateTime ChannelStartTime;

        private int processID;

        DispatcherTimer timer;

        public string _ffmpegPath;

        private Process process;

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        public Channel(XElement element, string FFmpegPath)
        {
            _ffmpegPath = FFmpegPath;
            NewChannel(element);
        }


        private void NewChannel(XElement element)
        {
            enable = true;

        //    FFplay = new Positioner();

            name = element.Element("name").Attribute("value").Value;

            link = element.Element("link").Attribute("value").Value;
            if (string.IsNullOrWhiteSpace(link)) enable = false;

            if (element.Element("aspect").Attribute("value").Value != "4:3") widescreen = true;
            else widescreen = false;
/*
            try
            {
                
                x = int.Parse(element.Element("position").Attribute("x").Value);
                y = int.Parse(element.Element("position").Attribute("y").Value);
            }
            catch { }*/
        }

        public void Start()
        {
            if (enable)
            {
                process = new Process();
                timer = new DispatcherTimer();
                timer.Tick += new EventHandler(Tick);

                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = "cmd.exe";
                startInfo.Arguments = "/C ffmpeg.exe -rtbufsize 5M -vsync 2 -i " + link + " -filter_complex \"[0:v]scale = 356x200[v1s];[0:a] showvolume=o=v:w=200:h=7:t=0:v=0:b=1:f=0:s=0:dm=1:r=10:ds=log,format=yuv420p[v1a];[v1s] [v1a] hstack=2\"" + " -s 371x200 -r 25 -c:v rawvideo -an -f rawvideo - | ffplay.exe -window_title \""+name+"\" -f rawvideo -framerate 25 -pixel_format yuv420p -video_size 371x200 -noborder -";
                startInfo.WorkingDirectory = _ffmpegPath;
                startInfo.UseShellExecute = false;
                startInfo.CreateNoWindow = true;

                process.StartInfo = startInfo;
                process.Start();
                processID = process.Id;

                timer.Start();
            }
        }

        public void Close()
        {
            if (Process.GetProcessById(processID) != null)
            {
                foreach (Process pr in ProcessExtensions.GetChildProcesses(process))
                {
                    try
                    {
                        pr.Kill();
                    }
                    catch (ArgumentException) { }
                }
            }
        }

        private void Tick(object sender, EventArgs e)
        {
            if (process != null && process.Responding)
            {
                winHNDL = GetFFplayProcess(process);
                if (winHNDL!=IntPtr.Zero)
                {
                    RECT Coord = new RECT();
                    GetWindowRect(winHNDL, out Coord);

                    if (Coord.Left != x  || Coord.Top != y)
                    {
                        MoveWindow(winHNDL, x, y, 371, 200, true);
                    }
                    else
                    {
                        timer.Stop();
                        timer.IsEnabled = false;
                    }
                }
            }
        }

        private IntPtr GetFFplayProcess(Process MainProcess)
        {
            foreach (Process pr in ProcessExtensions.GetChildProcesses(MainProcess))
            {
                if (pr.ProcessName == "ffplay")
                {
                    return pr.MainWindowHandle;
                }
            }
            return IntPtr.Zero;
        }

 

        /* public XElement ChannelToXElement()
         {
             XElement element = new XElement("Channel");

             XElement subElement = new XElement("number");
             subElement.Add(new XAttribute("value", number));
             element.Add(subElement);

             subElement = new XElement("name");
             subElement.Add(new XAttribute("value", name));
             element.Add(subElement);

             subElement = new XElement("link");
             subElement.Add(new XAttribute("value", link));
             element.Add(subElement);

             subElement = new XElement("aspect");
             if(widescreen) subElement.Add(new XAttribute("value", "16:9"));
             else subElement.Add(new XAttribute("value", "4:3"));
             element.Add(subElement);

             subElement = new XElement("position");
             subElement.Add(new XAttribute("x", x),new XAttribute("y",y));
             element.Add(subElement);

             return element;
         }*/
    }
}
