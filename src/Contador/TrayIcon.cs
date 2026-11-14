using System.Buffers.Binary;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Contador;

/// <summary>Windows notification-area icon using the native Shell API.</summary>
public sealed class TrayIcon : IDisposable
{
    private const int CallbackMessage = 0x8001;
    private readonly HwndSource source;
    private readonly Action open;
    private readonly Action exit;
    private readonly uint taskbarCreated;
    private readonly IntPtr icon;
    private readonly bool ownsIcon;
    private bool disposed;
    private NotifyData data;
    public bool IsAvailable { get; private set; }
    public int RegistrationError { get; private set; }

    public TrayIcon(Action open, Action exit)
    {
        this.open = open;
        this.exit = exit;
        source = new HwndSource(new HwndSourceParameters("Contador.Tray")
        {
            WindowStyle = unchecked((int)0x80000000), ExtendedWindowStyle = 0x80,
            Width = 0, Height = 0
        });
        source.AddHook(WindowMessage);
        taskbarCreated = RegisterWindowMessage("TaskbarCreated");
        icon = LoadCoin();
        ownsIcon = icon != IntPtr.Zero;
        if (icon == IntPtr.Zero) icon = LoadIcon(IntPtr.Zero, (IntPtr)32512);
        data = new NotifyData
        {
            Size = (uint)Marshal.SizeOf<NotifyData>(), Window = source.Handle, Id = 1,
            Flags = 1 | 2 | 4 | 0x80, Callback = CallbackMessage, Icon = icon,
            Tip = $"Contador {AppVersion.Display}", Info = "", InfoTitle = "", Version = 4
        };
        Add();
    }

    private void Add()
    {
        IsAvailable = ShellNotifyIcon(0, ref data);
        RegistrationError = IsAvailable ? 0 : Marshal.GetLastWin32Error();
        if (IsAvailable) ShellNotifyIcon(4, ref data);
    }

    public void SetBackground(bool background)
    {
        if (disposed) return;
        data.Tip = $"Contador {AppVersion.Display}" + (background ? " · En segundo plano" : "");
        ShellNotifyIcon(1, ref data);
    }

    private IntPtr WindowMessage(IntPtr hwnd, int message, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (disposed) return IntPtr.Zero;
        if (taskbarCreated != 0 && (uint)message == taskbarCreated)
        {
            Add();
            if (!IsAvailable) source.Dispatcher.BeginInvoke(open);
        }
        else if (message == CallbackMessage)
        {
            var action = (int)(lParam.ToInt64() & 0xffff);
            if (action is 0x400 or 0x401 or 0x203) source.Dispatcher.BeginInvoke(open);
            else if (action is 0x7b or 0x205) source.Dispatcher.BeginInvoke(new Action(ShowMenu));
            handled = true;
        }
        return IntPtr.Zero;
    }

    private void ShowMenu()
    {
        if (disposed) return;
        var menu = CreatePopupMenu();
        if (menu == IntPtr.Zero) return;
        try
        {
            AppendMenu(menu, 0, 1, "Abrir Contador");
            AppendMenu(menu, 0x800, 0, null);
            AppendMenu(menu, 0, 2, "Salir");
            GetCursorPos(out var point);
            SetForegroundWindow(source.Handle);
            var command = TrackPopupMenuEx(menu, 0x100 | 0x80 | 0x2, point.X, point.Y, source.Handle, IntPtr.Zero);
            PostMessage(source.Handle, 0, IntPtr.Zero, IntPtr.Zero);
            if (command == 1) open();
            else if (command == 2) exit();
        }
        finally { DestroyMenu(menu); }
    }

    private static IntPtr LoadCoin()
    {
        var resource = Application.GetResourceStream(new Uri("/Contador;component/Assets/Contador.ico", UriKind.Relative));
        if (resource == null) return IntPtr.Zero;
        using var stream = resource.Stream;
        using var buffer = new MemoryStream();
        stream.CopyTo(buffer);
        var bytes = buffer.ToArray();
        if (bytes.Length < 6) return IntPtr.Zero;
        var count = BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4, 2));
        var chosen = -1;
        var distance = int.MaxValue;
        for (var i = 0; i < count && 6 + (i + 1) * 16 <= bytes.Length; i++)
        {
            var offset = 6 + i * 16;
            var size = bytes[offset] == 0 ? 256 : bytes[offset];
            if (Math.Abs(size - 32) >= distance) continue;
            chosen = offset;
            distance = Math.Abs(size - 32);
        }
        if (chosen < 0) return IntPtr.Zero;
        var length = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(chosen + 8, 4));
        var start = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(chosen + 12, 4));
        if (start < 0 || length <= 0 || (long)start + length > bytes.Length) return IntPtr.Zero;
        return CreateIconFromResourceEx(bytes.AsSpan(start, length).ToArray(), (uint)length, true, 0x30000, 32, 32, 0);
    }

    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        ShellNotifyIcon(2, ref data);
        IsAvailable = false;
        source.Dispose();
        if (ownsIcon) DestroyIcon(icon);
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct NotifyData
    {
        public uint Size;
        public IntPtr Window;
        public uint Id, Flags, Callback;
        public IntPtr Icon;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string Tip;
        public uint State, StateMask;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] public string Info;
        public uint Version;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)] public string InfoTitle;
        public uint InfoFlags;
        public Guid Guid;
        public IntPtr BalloonIcon;
    }
    [StructLayout(LayoutKind.Sequential)] private struct Point { public int X, Y; }
    [DllImport("shell32.dll", EntryPoint = "Shell_NotifyIconW", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
    private static extern bool ShellNotifyIcon(uint message, ref NotifyData data);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern uint RegisterWindowMessage(string name);
    [DllImport("user32.dll")] private static extern IntPtr CreatePopupMenu();
    [DllImport("user32.dll", EntryPoint = "AppendMenuW", CharSet = CharSet.Unicode)]
    private static extern bool AppendMenu(IntPtr menu, uint flags, nuint id, string? text);
    [DllImport("user32.dll")] private static extern uint TrackPopupMenuEx(IntPtr menu, uint flags, int x, int y, IntPtr owner, IntPtr parameters);
    [DllImport("user32.dll")] private static extern bool DestroyMenu(IntPtr menu);
    [DllImport("user32.dll")] private static extern bool GetCursorPos(out Point point);
    [DllImport("user32.dll")] private static extern bool SetForegroundWindow(IntPtr window);
    [DllImport("user32.dll", EntryPoint = "PostMessageW")] private static extern bool PostMessage(IntPtr window, uint message, IntPtr wParam, IntPtr lParam);
    [DllImport("user32.dll")] private static extern IntPtr CreateIconFromResourceEx(byte[] bytes, uint size, bool icon, uint version, int width, int height, uint flags);
    [DllImport("user32.dll", EntryPoint = "LoadIconW")] private static extern IntPtr LoadIcon(IntPtr instance, IntPtr name);
    [DllImport("user32.dll")] private static extern bool DestroyIcon(IntPtr icon);
}
