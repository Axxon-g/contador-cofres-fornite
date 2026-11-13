namespace Contador;

public static class AppVersion
{
    private static readonly Version Current = typeof(AppVersion).Assembly.GetName().Version
        ?? new Version(1, 3, 0);

    public static string Full => $"{Current.Major}.{Current.Minor}.{Math.Max(0, Current.Build)}";
    public static string Display => Current.Build > 0 ? Full : $"{Current.Major}.{Current.Minor}";
    public static string Label => "v" + Display;
}
