using System;
using System.Runtime.InteropServices;

namespace MachineLabel;

/// <summary>
/// Win32 interop for detecting taskbar position and size.
/// </summary>
public static class TaskbarHelper
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct APPBARDATA
    {
        public uint cbSize;
        public IntPtr hWnd;
        public uint uCallbackMessage;
        public uint uEdge;
        public RECT rc;
        public int lParam;
    }

    public enum TaskbarEdge : uint
    {
        Left = 0,
        Top = 1,
        Right = 2,
        Bottom = 3
    }

    [DllImport("shell32.dll")]
    private static extern IntPtr SHAppBarMessage(uint dwMessage, ref APPBARDATA pData);

    [DllImport("user32.dll")]
    public static extern IntPtr FindWindow(string lpClassName, string? lpWindowName);

    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
        int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll")]
    public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    public static extern bool IsWindowVisible(IntPtr hWnd);

    private const uint ABM_GETTASKBARPOS = 0x00000005;
    private const uint ABM_GETSTATE = 0x00000004;

    public static readonly IntPtr HWND_TOPMOST = new(-1);
    public const uint SWP_NOACTIVATE = 0x0010;
    public const uint SWP_SHOWWINDOW = 0x0040;
    public const int GWL_EXSTYLE = -20;
    public const int WS_EX_TOOLWINDOW = 0x00000080;
    public const int WS_EX_NOACTIVATE = 0x08000000;
    public const int WS_EX_TOPMOST = 0x00000008;

    public static (RECT rect, TaskbarEdge edge) GetTaskbarInfo()
    {
        var data = new APPBARDATA { cbSize = (uint)Marshal.SizeOf<APPBARDATA>() };
        SHAppBarMessage(ABM_GETTASKBARPOS, ref data);
        return (data.rc, (TaskbarEdge)data.uEdge);
    }

    public static bool IsTaskbarAutoHideEnabled()
    {
        var data = new APPBARDATA { cbSize = (uint)Marshal.SizeOf<APPBARDATA>() };
        var state = SHAppBarMessage(ABM_GETSTATE, ref data);
        return (state.ToInt32() & 0x01) != 0;
    }

    public static IntPtr GetTaskbarHandle()
    {
        return FindWindow("Shell_TrayWnd", null);
    }

    public static bool IsTaskbarVisible()
    {
        var hwnd = GetTaskbarHandle();
        if (hwnd == IntPtr.Zero) return false;

        if (!IsWindowVisible(hwnd)) return false;

        GetWindowRect(hwnd, out RECT rect);
        // If taskbar is auto-hidden and off-screen, its rect will be very small
        int height = rect.Bottom - rect.Top;
        int width = rect.Right - rect.Left;
        return height > 2 && width > 2;
    }
}
