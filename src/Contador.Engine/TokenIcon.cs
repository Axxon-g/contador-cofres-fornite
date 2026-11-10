using System.Runtime.InteropServices;

namespace Contador.Engine;

/// <summary>Uses the same embedded multi-resolution coin icon as the executable.</summary>
public static class TokenIcon
{
    private static readonly Lazy<byte[]> Data = new(() =>
    {
        using var input = typeof(TokenIcon).Assembly.GetManifestResourceStream("Contador.Token.ico")
            ?? throw new InvalidOperationException("No se encontró el icono integrado.");
        using var buffer = new MemoryStream(); input.CopyTo(buffer); return buffer.ToArray();
    });

    public static byte[] Frame(int size)
    {
        var data = Data.Value;
        var count = BitConverter.ToUInt16(data,4);
        var entries = Enumerable.Range(0,count).Select(i => 6 + 16*i);
        var entry = entries.FirstOrDefault(i => (data[i] == 0 ? 256 : data[i]) >= size);
        if (entry == 0) entry = 6 + 16*(count-1);
        var length = checked((int)BitConverter.ToUInt32(data,entry+8));
        var offset = checked((int)BitConverter.ToUInt32(data,entry+12));
        return data.AsSpan(offset,length).ToArray();
    }

    public static bool Draw(IntPtr dc, int x, int y, int size)
    {
        var bytes = Frame(size);
        var icon = CreateIconFromResourceEx(bytes,(uint)bytes.Length,true,0x30000,size,size,0);
        if (icon == IntPtr.Zero) return false;
        try { return DrawIconEx(dc,x,y,icon,size,size,0,IntPtr.Zero,3); }
        finally { DestroyIcon(icon); }
    }

    [DllImport("user32.dll", SetLastError=true)]
    private static extern IntPtr CreateIconFromResourceEx(byte[] bits,uint length,[MarshalAs(UnmanagedType.Bool)] bool icon,uint version,int width,int height,uint flags);
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DrawIconEx(IntPtr dc,int x,int y,IntPtr icon,int width,int height,uint step,IntPtr brush,uint flags);
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DestroyIcon(IntPtr icon);
}
