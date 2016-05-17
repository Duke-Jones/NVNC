#region

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;

#endregion

namespace NVNC
{
    public class VncProxy
    {
        private readonly int _proxyPort;
        private readonly int _vncPort;

        public VncProxy(int proxyPort = 5901, int vncPort = 5900)
        {
            _proxyPort = proxyPort;
            _vncPort = vncPort;
        }


        //Should hold us over until a native C# version can be written
        public void StartWebsockify()
        {
            //
            // Setup the process with the ProcessStartInfo class.
            //
            var webParams = $"{_proxyPort} {GetIPv4Address() + ":" + _vncPort}";
            var start = new ProcessStartInfo
            {
                FileName =
                    Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + @"\data\websockify\websockify.exe",
                Arguments = webParams,
                UseShellExecute = false,
                RedirectStandardOutput = true
            };
            Process.Start(start);
            Console.WriteLine("VNC Proxy Started");
        }

        public void StopWebsockify()
        {
            var webSockifyInstances = Process.GetProcessesByName("websockify");
            foreach (var instance in webSockifyInstances)
            {
                try
                {
                    instance.Kill();
                    instance.WaitForExit();
                    Console.WriteLine("VNC Proxy Killed");
                }
                catch (Exception)
                {
                    //who cares
                }
            }
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