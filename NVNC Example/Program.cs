#region

using System;
using System.Threading.Tasks;
using NVNC;

#endregion

namespace VNCTest
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var s = new VncServer("password", 5901, 5900, "Ulterius VNC");
            try
            {
                s.Start();
            }
            catch (ArgumentNullException ex)
            {
               s.Stop();
                return;
            }
            
        }
    }
}