using System.Text;
using ChestCounter;
using OozSharp;
using OozSharp.Exceptions;
using Unreal.Core;

internal static class SecurityChecks
{
    public static void Run(Action<bool, string> check)
    {
        void Rejected(Action action, string label)
        {
            try { action(); }
            catch (Exception ex) when (ex is InvalidDataException or DecoderException)
            {
                check(true, label);
                return;
            }
            throw new Exception("No se rechazó: " + label);
        }

        var kraken = new Kraken();
        var raw = Enumerable.Range(0, 256).Select(i => (byte)i).ToArray();
        check(kraken.Decompress(new byte[] { 0xCC, 10 }.Concat(raw).ToArray(), 256)
            .Span.SequenceEqual(raw), "Bloque sin comprimir conserva bytes exactos");
        var multi = new byte[0x40000 + 16];
        Array.Fill(multi, (byte)37);
        var encoded = new byte[] { 0xCC, 10 }.Concat(multi.Take(0x40000))
            .Concat(new byte[] { 0xCC, 10 }).Concat(multi.Skip(0x40000)).ToArray();
        check(kraken.Decompress(encoded, multi.Length).Span.SequenceEqual(multi), "Cabeceras múltiples no se copian como datos");
        check(kraken.Decompress([0x8C, 10, 7, 255, 255, 171], 256).Span.ToArray()
            .All(b => b == 171), "Quantum de relleno repite el byte sin leer la pila");

        Rejected(() => kraken.Decompress([], 1), "Cabecera ausente");
        Rejected(() => kraken.Decompress([0x8C], 1), "Cabecera parcial");
        Rejected(() => kraken.Decompress([0x8C, 10, 0, 0], 256), "Quantum parcial");
        Rejected(() => kraken.Decompress([0x8C, 138, 0, 0, 0], 256), "Checksum parcial antes de leer punteros");
        Rejected(() => kraken.Decompress([0x8C, 10, 7, 255, 255], 256), "Relleno sin byte de valor");
        var started = System.Diagnostics.Stopwatch.StartNew();
        Rejected(() => kraken.Decompress([0x8C, 10, 0, 0, 50], 256), "Entrada truncada termina sin bucle");
        check(started.Elapsed < TimeSpan.FromSeconds(1), "Rechazo de cero progreso acotado");
        Rejected(() => kraken.Decompress([0xCC, 10, 1], 32), "Bloque raw insuficiente");
        Rejected(() => kraken.Decompress([], int.MaxValue), "Tamaño de salida excesivo antes de reservar");

        // Raw literal/command streams exercise the actual compressed Mermaid parser.
        byte[] Compressed(byte[] literals, byte[] commands, ushort[] offsets, byte[] far, byte[] lengths)
        {
            using var payload = new MemoryStream();
            payload.Write("ABCDEFGH"u8);
            void Raw(byte[] data)
            {
                payload.WriteByte((byte)(data.Length >> 16));
                payload.WriteByte((byte)(data.Length >> 8));
                payload.WriteByte((byte)data.Length);
                payload.Write(data);
            }
            Raw(literals);
            Raw(commands);
            payload.WriteByte((byte)offsets.Length);
            payload.WriteByte((byte)(offsets.Length >> 8));
            foreach (var offset in offsets)
            {
                payload.WriteByte((byte)offset);
                payload.WriteByte((byte)(offset >> 8));
            }
            payload.Write(far.Length == 0 ? new byte[3] : new byte[] { 0, 16, 0 });
            payload.Write(far);
            payload.Write(lengths);
            var size = checked((int)payload.Length);
            var chunk = new byte[] { 0x88, (byte)(size >> 8), (byte)size }.Concat(payload.ToArray()).ToArray();
            var quantum = chunk.Length - 1;
            return new byte[] { 0x8C, 10, (byte)(quantum >> 16), (byte)(quantum >> 8), (byte)quantum }
                .Concat(chunk).ToArray();
        }
        var pattern = Enumerable.Range(0, 256).Select(i => (byte)"ABCDEFGH"[i % 8]).ToArray();
        check(kraken.Decompress(Compressed([], [1], [8], [], [157]), 256).Span.SequenceEqual(pattern),
            "Match solapado repite el patrón hasta el último byte");
        check(kraken.Decompress(Compressed([], [2], [], [0, 0, 0], [219]), 256).Span.SequenceEqual(pattern),
            "Offset32 cero referencia prefijo ya producido");
        var expected = pattern.Take(114).Concat(Enumerable.Repeat((byte)'Z', 142)).ToArray();
        check(kraken.Decompress(Compressed(Enumerable.Repeat((byte)'Z', 142).ToArray(),
            [1, 248], [8], [], [0]), 256).Span.SequenceEqual(expected),
            "Reutiliza distancia cuando no quedan offsets y copia literales finales exactos");
        Rejected(() => kraken.Decompress(Compressed([], [1], [8], [], [158]), 256), "Match excede destino por un byte");
        Rejected(() => kraken.Decompress(Compressed([], [1], [0], [], [157]), 256), "Distancia cero no referencia salida producida");
        Rejected(() => kraken.Decompress(Compressed([], [1], [9], [], [157]), 256), "Match anterior al inicio");
        Rejected(() => kraken.Decompress(Compressed([], [2], [], [9, 0, 0], [219]), 256), "Offset32 anterior al inicio");
        Rejected(() => kraken.Decompress(Compressed([], [1], [], [], [157]), 256), "Match sin offset16");
        Rejected(() => kraken.Decompress(Compressed([], [0], [], [], [192]), 256), "Literal largo sin datos");
        Rejected(() => kraken.Decompress(Compressed([], [255], [], [], []), 256), "Literal corto sin datos");
        Rejected(() => kraken.Decompress(Compressed([], [1], [8], [], [156]), 256), "Literal final ausente");
        Rejected(() => kraken.Decompress(Compressed([], [1], [8], [], []), 256), "Longitud ausente");

        using (ReplayLimits.Begin())
        {
            using var input = new Unreal.Core.BinaryReader(new byte[] { 255, 255, 255, 127 });
            Rejected(() => input.ReadArray(input.ReadByte), "Array enorme sin datos");
            using var tuple = new Unreal.Core.BinaryReader(new byte[] { 255, 255, 255, 127 });
            Rejected(() => tuple.ReadTupleArray(tuple.ReadByte, tuple.ReadByte), "Tuplas enormes sin datos");
            check(ReplayLimits.Reserve(1000) == 1000, "Capacidad sparse no requiere 1000 registros codificados");
            Rejected(() => ReplayLimits.Reserve(uint.MaxValue), "Capacidad export/dinámica excesiva");
            Rejected(() => ReplayLimits.Reserve(1, int.MaxValue), "Reserva excede presupuesto");
        }
        using (var input = new Unreal.Core.BinaryReader(new byte[] { 2, 0, 0, 0, 7, 9 }))
            check(input.ReadArray(input.ReadByte).SequenceEqual(new byte[] { 7, 9 }), "Array válido al final de entrada");
        using (var oversized = new DeclaredLargeStream())
            Rejected(() => ReplayLimits.ReadInput(oversized), "Tamaño de archivo rechazado antes de leer");
        using (var small = new MemoryStream([1, 2, 3]))
            check(ReplayLimits.ReadInput(small).SequenceEqual(new byte[] { 1, 2, 3 }), "Lectura normal permanece intacta");

        foreach (var text in new[] { "=1+1", "+1", "-1", "@SUM(1)", " \t=1+1", "\r\n=1+1" })
            check(CsvText.Cell(text).StartsWith("\"'"), "Neutraliza fórmula: " + text.Trim());
        check(CsvText.Cell("Cofre; \"épico\"\nsegunda línea") == "\"Cofre; \"\"épico\"\"\nsegunda línea\"",
            "Comillas, separadores y saltos permanecen en una celda");
        var data = new TrackerData { Matches = [new MatchRecord { ReplayFile = "=1+1;prueba.replay" }] };
        check(ReportWriter.Csv(data).Contains(CsvText.Cell("=1+1;prueba.replay")),
            "CSV automático usa la protección compartida");
    }

    private sealed class DeclaredLargeStream : Stream
    {
        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => false;
        public override long Length => (long)ReplayLimits.MaxInputBytes + 1;
        public override long Position { get; set; }
        public override int Read(byte[] buffer, int offset, int count) => throw new Exception("No debe leer");
        public override void Flush() => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
