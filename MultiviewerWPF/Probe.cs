using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Management;


namespace MultiviewerWPF
{
    public class Probe
    {
        private string _link;

        public string FFmpegPath;

        public Probe(string url)
        {
            _link = url;
        }

        public void StartTest()
        {
            Process testProcess = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
           // startInfo.WorkingDirectory = FFmpegPath;
            startInfo.FileName =Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg\\bin\\ffprobe.exe");
            startInfo.Arguments = "-i " + _link;
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            testProcess.StartInfo = startInfo;
            if (File.Exists(startInfo.FileName))
            {
                testProcess.Start();
                StreamReader d = testProcess.StandardError;
                do
                {
                    string s = d.ReadLine();
                    if(s.Contains("Stream") && s.Contains("Video"))
                    {
                        ParseVideo(s);
                    }
                }
                while (!d.EndOfStream);
                testProcess.WaitForExit();
                testProcess.Close();
            }
        }

        private void ParseVideo(string videoConfigStrig)
        {
            string[] strings = videoConfigStrig.Split(',');
        }

    }
}
