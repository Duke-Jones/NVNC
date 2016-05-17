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
            var s = new VncServer("a", 5901, 5900, "Ulterius VNC");
            try
            {
                s.Start();
                Console.Read();
                s.Stop();
            }
            catch (ArgumentNullException ex)
            {
               s.Stop();
                return;
            }
            
        }
    }
}