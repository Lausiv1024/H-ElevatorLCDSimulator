using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace UrbanAce_7
{
    public class Win32API
    {
        [DllImport("user32.dll")]
        public static extern short GetKeyState(int nVirtKey);

        public static bool isCtrlPressed => GetKeyState(0xA2) < 0 || GetKeyState(0xA3) < 0;
    }
}
