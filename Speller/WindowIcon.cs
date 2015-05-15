using System; // IntPtr
using System.Windows; // Window, RoutedEventArgs
using System.Windows.Interop; // WindowInteropHelper
using System.Runtime.InteropServices; // DllImportAttribute

namespace Speller
{
    static class WindowIcon
    {
        [DllImport("user32.dll")]
        static extern uint GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

        private const int GWL_STYLE = -16;
        private const uint WS_SYSMENU = 0x80000;

        public static void Remove(Window window)
        {
            IntPtr hwnd = new WindowInteropHelper(window).Handle;
            SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & (0xFFFFFFFF ^ WS_SYSMENU));
        }
    }
}
