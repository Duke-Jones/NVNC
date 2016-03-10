#region

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using MiscUtil.Conversion;

#endregion

namespace NVNC
{
    public class VncProxy
    {

        private readonly int proxyPort;
        private readonly int vncPort;
    
        private Process process;

        public VncProxy(int proxyPort = 5901, int vncPort = 5900)
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
                
                FileName = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + @"\data\websockify\websockify.exe",
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

      
        public void StartProxy()
        {

        }
    }
}