using System.Security.Cryptography;

namespace ChestCounter;

/// <summary>Opt-in local preservation. Never moves, overwrites or deletes a source replay.</summary>
public static class ReplayArchive
{
    public static string Save(FileInfo source, string folder, string expectedFingerprint)
    {
        if (ReplayFileStability.HasChanged(source, expectedFingerprint))
            throw new IOException("La repetición cambió antes de archivarla.");
        Directory.CreateDirectory(folder);
        var temporary = Path.Combine(folder, Guid.NewGuid().ToString("N") + ".tmp");
        try
        {
            using (var input = new FileStream(source.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var output = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                input.CopyTo(output);
            if (ReplayFileStability.HasChanged(source, expectedFingerprint))
                throw new IOException("La repetición cambió durante el archivo.");
            using var copied = File.OpenRead(temporary);
            var name = Convert.ToHexString(SHA256.HashData(copied)).ToLowerInvariant() + ".replay";
            copied.Close();
            var destination = Path.Combine(folder, name);
            if (!File.Exists(destination)) File.Move(temporary, destination);
            return destination;
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
    }
}
