using System;
using System.Runtime.InteropServices;

namespace CtYun
{
    /// <summary>
    /// Windows 控制台窗口控制：用于隐藏/显示程序运行时的黑色控制台窗口。
    /// 在 Linux / macOS / Docker 下调用不生效（自动跳过），不影响原功能。
    /// </summary>
    internal static class WinConsole
    {
        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;

        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        /// <summary>隐藏当前控制台窗口（仅 Windows 生效）。</summary>
        public static void Hide()
        {
            if (!OperatingSystem.IsWindows())
            {
                return;
            }

            try
            {
                var hwnd = GetConsoleWindow();
                if (hwnd != IntPtr.Zero)
                {
                    ShowWindow(hwnd, SW_HIDE);
                }
            }
            catch
            {
                // 隐藏失败不致命，程序继续运行
            }
        }

        /// <summary>显示控制台窗口（仅 Windows 生效）。</summary>
        public static void Show()
        {
            if (!OperatingSystem.IsWindows())
            {
                return;
            }

            try
            {
                var hwnd = GetConsoleWindow();
                if (hwnd != IntPtr.Zero)
                {
                    ShowWindow(hwnd, SW_SHOW);
                }
            }
            catch
            {
                // 忽略
            }
        }
    }
}