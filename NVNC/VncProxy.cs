#region

using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;

#endregion

namespace NVNC
{
    public class VncProxy
    {
        private Process process;
        private readonly int proxyPort;
        private readonly int vncPort;

        public VncProxy(int proxyPort, int vncPort)
        {
            this.proxyPort = proxyPort;
            this.vncPort = vncPort;
        }



        //Should hold us over until a native C# version can be written
        public void StartWebsockify()
        {
            //
            // Setup the process with the ProcessStartInfo class.
            //
            var webParams = $"{proxyPort} {GetIPv4Address() + ":" + vncPort}";

            var start = new ProcessStartInfo
            {
                FileName = @"./data/websockify/websockify.exe",
                Arguments = webParams,
                UseShellExecute = false,
                RedirectStandardOutput = true
            };
            using (process = Process.Start(start))
            {
                if (process == null) return;
                using (var reader = process.StandardOutput)
                {
                    var result = reader.ReadToEnd();
                    Console.Write(result);
                }
            }
        }

        public void StopWebsockify()
        {
            if (process == null) return;
            process.Kill();
            Console.WriteLine("VNC Proxy Killed");
        }


        private string GetIPv4Address()
        {
            var ips = Dns.GetHostAddresses(Dns.GetHostName());
            foreach (var i in ips.Where(i => i.AddressFamily == AddressFamily.InterNetwork))
            {
                return i.ToString();
            }
            return "127.0.0.1";
        }
    }
}