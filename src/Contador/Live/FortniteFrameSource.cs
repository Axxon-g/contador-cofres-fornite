using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Contador.Live;

/// <summary>Reads bounded regions of the foreground Fortnite client only.</summary>
internal static class FortniteFrameSource
{
    internal sealed record Frame(byte[] Pixels, int Width, int Height);
    [StructLayout(LayoutKind.Sequential)] private struct Rect { public int Left, Top, Right, Bottom; }
    [StructLayout(LayoutKind.Sequential)] private struct Point { public int X, Y; }
    [StructLayout(LayoutKind.Sequential)]
    private struct BitmapInfo
    {
        public uint Size;
        public int Width, Height;
        public ushort Planes, BitCount;
        public uint Compression, SizeImage;
        public int XPels, YPels;
        public uint ColorsUsed, ColorsImportant;
    }

    public static Frame? Capture(bool roster)
    {
        using var dpi = new PhysicalPixelScope();
        var window = GetForegroundWindow();
        if (window == IntPtr.Zero || IsIconic(window)) return null;
        GetWindowThreadProcessId(window, out var pid);
        using var process = Process.GetProcessById((int)pid);
        if (!process.ProcessName.Equals("FortniteClient-Win64-Shipping", StringComparison.OrdinalIgnoreCase)) return null;
        if (!GetClientRect(window, out var client)) return null;
        var width = client.Right - client.Left;
        var height = client.Bottom - client.Top;
        if (width < 640 || height < 480 || width > 7680 || height > 4320) return null;
        var origin = new Point();
        if (!ClientToScreen(window, ref origin)) return null;
        // HUD only: no full-monitor, microphone, process memory or background-window capture.
        var x = roster ? 0.01 : 0.55;
        var y = roster ? 0.02 : 0.22;
        var w = roster ? 0.22 : 0.33;
        var h = roster ? 0.28 : 0.30;
        var cropWidth = (int)(width * w);
        var cropHeight = (int)(height * h);
        var source = GetDC(IntPtr.Zero);
        if (source == IntPtr.Zero) return null;
        var memory = IntPtr.Zero;
        var bitmap = IntPtr.Zero;
        var previous = IntPtr.Zero;
        try
        {
            memory = CreateCompatibleDC(source);
            bitmap = CreateCompatibleBitmap(source, cropWidth, cropHeight);
            if (memory == IntPtr.Zero || bitmap == IntPtr.Zero) return null;
            previous = SelectObject(memory, bitmap);
            if (!BitBlt(memory, 0, 0, cropWidth, cropHeight, source,
                origin.X + (int)(width * x), origin.Y + (int)(height * y), 0x00CC0020)) return null;
            SelectObject(memory, previous);
            previous = IntPtr.Zero;
            var info = new BitmapInfo
            {
                Size = (uint)Marshal.SizeOf<BitmapInfo>(),
                Width = cropWidth,
                Height = -cropHeight,
                Planes = 1,
                BitCount = 32
            };
            var pixels = new byte[checked(cropWidth * cropHeight * 4)];
            if (GetDIBits(memory, bitmap, 0, (uint)cropHeight, pixels, ref info, 0) != cropHeight) return null;
            // Drop a frame if focus changed during the copy, before it reaches OCR.
            return GetForegroundWindow() == window ? new Frame(pixels, cropWidth, cropHeight) : null;
        }
        finally
        {
            if (previous != IntPtr.Zero) SelectObject(memory, previous);
            if (bitmap != IntPtr.Zero) DeleteObject(bitmap);
            if (memory != IntPtr.Zero) DeleteDC(memory);
            ReleaseDC(IntPtr.Zero, source);
        }
    }

    // Keep client coordinates and desktop pixels in the same space on scaled monitors.
    // Restore the worker's previous awareness before returning it to the thread pool.
    private sealed class PhysicalPixelScope : IDisposable
    {
        private readonly IntPtr previous = SetThreadDpiAwarenessContext(new IntPtr(-4));
        public void Dispose()
        {
            if (previous != IntPtr.Zero) SetThreadDpiAwarenessContext(previous);
        }
    }

    [DllImport("user32.dll")] private static extern IntPtr SetThreadDpiAwarenessContext(IntPtr context);
    [DllImport("user32.dll")] private static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] private static extern bool IsIconic(IntPtr window);
    [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(IntPtr window, out uint pid);
    [DllImport("user32.dll")] private static extern bool GetClientRect(IntPtr window, out Rect rect);
    [DllImport("user32.dll")] private static extern bool ClientToScreen(IntPtr window, ref Point point);
    [DllImport("user32.dll")] private static extern IntPtr GetDC(IntPtr window);
    [DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr window, IntPtr dc);
    [DllImport("gdi32.dll")] private static extern IntPtr CreateCompatibleDC(IntPtr dc);
    [DllImport("gdi32.dll")] private static extern IntPtr CreateCompatibleBitmap(IntPtr dc, int width, int height);
    [DllImport("gdi32.dll")] private static extern IntPtr SelectObject(IntPtr dc, IntPtr value);
    [DllImport("gdi32.dll")] private static extern bool DeleteObject(IntPtr value);
    [DllImport("gdi32.dll")] private static extern bool DeleteDC(IntPtr dc);
    [DllImport("gdi32.dll")] private static extern bool BitBlt(IntPtr target, int x, int y, int width, int height, IntPtr source, int sx, int sy, uint operation);
    [DllImport("gdi32.dll")] private static extern int GetDIBits(IntPtr dc, IntPtr bitmap, uint first, uint count, [Out] byte[] data, ref BitmapInfo info, uint usage);
}
