#region

using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using NVNC.Utils;

#endregion

namespace NVNC
{
    /// <summary>
    ///     A wrapper class that should be used. It represents a VNC Server, and handles all the RFB procedures and
    ///     communication.
    /// </summary>
    public class VncServer
    {
        private Framebuffer fb;
        private VncHost host;
        private VncProxy proxy;

        /// <summary>
        ///     The default constructor using the default values for the parameters.
        ///     Port is set to 5900, the Name is set to Default, and there is no password.
        /// </summary>
        public VncServer()
            : this("", 5901, 5900, "Ulterius VNC")
        {
        }

        public VncServer(string password, int proxyPort, int port, string name)
        {
            Password = password;
            ProxyPort = proxyPort;
            Port = port;
            Name = name;

            var screenSize = ScreenSize();
            fb = new Framebuffer(screenSize.Width, screenSize.Height)
            {
                BitsPerPixel = 32,
                Depth = 24,
                BigEndian = false,
                TrueColor = true,
                RedShift = 16,
                GreenShift = 8,
                BlueShift = 0,
                BlueMax = 0xFF,
                GreenMax = 0xFF,
                RedMax = 0xFF,
                DesktopName = name
            };
        }


        public int Port { get; set; }
        public int ProxyPort { get; set; }
        public string Password { get; set; }

        /// <summary>
        ///     The VNC Server name.
        ///     <remarks>The variable value should be non-null.</remarks>
        /// </summary>
        public string Name { get; set; }

        public void Start()
        {
            if (string.IsNullOrEmpty(Name))
                throw new ArgumentNullException("Name", "The VNC Server Name cannot be empty.");
            if (Port == 0)

                throw new ArgumentNullException("Port", "The VNC Server port cannot be zero.");
            if (ProxyPort == 0)

                throw new ArgumentNullException("ProxyPort", "You must set a proxy port.");


            Console.WriteLine("Started VNC Server at port: " + Port + " and proxy port at: " + ProxyPort);

            proxy = new VncProxy(ProxyPort, Port);
            Task.Factory.StartNew(() =>
            {
              proxy.StartWebsockify();
            });
            host = new VncHost(Port, Name,
                new ScreenHandler(new Rectangle(0, 0, ScreenSize().Width, ScreenSize().Height), true));


            host.WriteProtocolVersion();
            Console.WriteLine("Wrote Protocol Version");

            host.ReadProtocolVersion();
            Console.WriteLine("Read Protocol Version");

            Console.WriteLine("Awaiting Authentication");
            if (!host.WriteAuthentication(Password))
            {
                Console.WriteLine("Authentication failed !");
                host.Close();
                //Start();
            }
            else
            {
                Console.WriteLine("Authentication successfull !");

                var share = host.ReadClientInit();
                Console.WriteLine("Share: " + share);

                Console.WriteLine("Server name: " + fb.DesktopName);
                host.WriteServerInit(fb);

                while (host.isRunning)
                {
                    switch (host.ReadServerMessageType())
                    {
                        case VncHost.ClientMessages.SetPixelFormat:
                            Console.WriteLine("Read SetPixelFormat");
                            var f = host.ReadSetPixelFormat(fb.Width, fb.Height);
                            if (f != null)
                                fb = f;
                            break;
                        case VncHost.ClientMessages.ReadColorMapEntries:
                            Console.WriteLine("Read ReadColorMapEntry");
                            host.ReadColorMapEntry();
                            break;
                        case VncHost.ClientMessages.SetEncodings:
                            Console.WriteLine("Read SetEncodings");
                            host.ReadSetEncodings();
                            break;
                        case VncHost.ClientMessages.FramebufferUpdateRequest:
                            Console.WriteLine("Read FrameBufferUpdateRequest");
                            host.ReadFrameBufferUpdateRequest(fb);
                            break;
                        case VncHost.ClientMessages.KeyEvent:
                            Console.WriteLine("Read KeyEvent");
                            host.ReadKeyEvent();
                            break;
                        case VncHost.ClientMessages.PointerEvent:
                            Console.WriteLine("Read PointerEvent");
                            host.ReadPointerEvent();
                            break;
                        case VncHost.ClientMessages.ClientCutText:
                            Console.WriteLine("Read CutText");
                            host.ReadClientCutText();
                            break;
                    }
                }
                if (!host.isRunning)
                proxy.StopWebsockify();
                //Start();
            }
        }

        /// <summary>
        ///     Closes all active connections, and stops the VNC Server from listening on the specified port.
        /// </summary>
        public void Stop()
        {
            Console.WriteLine("VNC server Stopped");
            proxy.StopWebsockify();
            host.Close();
        }


        private Size ScreenSize()
        {
            var s = new Size
            {
                Height = Screen.PrimaryScreen.Bounds.Height,
                Width = Screen.PrimaryScreen.Bounds.Width
            };
            return s;
        }
    }
}