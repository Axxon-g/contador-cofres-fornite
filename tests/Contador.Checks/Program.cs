using ChestCounter;
using System.Text;
using System.Text.Json;
using Contador.Engine;
L.Set("es");
int checks = 0;
void Check(bool value, string message) { if (!value) throw new Exception(message); checks++; }
var folder = Path.Combine(Path.GetTempPath(), "ContadorChecks-" + Guid.NewGuid());
Directory.CreateDirectory(folder);
try
{
    string Header(string name, bool live, byte key, bool encrypted = true, bool wide = false, uint version = 6)
    {
        var path = Path.Combine(folder, name + ".replay");
        using var w = new BinaryWriter(File.Create(path));
        w.Write(0x1CA2E27Fu); w.Write(version);
        if (version >= 7) w.Write(0);
        w.Write(120000u); w.Write(1u); w.Write(1u);
        var bytes = wide ? Encoding.Unicode.GetBytes("Prueba\0") : Encoding.UTF8.GetBytes("Prueba\0");
        w.Write(wide ? -bytes.Length / 2 : bytes.Length); w.Write(bytes);
        w.Write(live ? 1u : 0u); w.Write(0L); w.Write(1u);
        w.Write(encrypted ? 1u : 0u); w.Write(32); w.Write(Enumerable.Repeat(key, 32).ToArray());
        return path;
    }
    var livePath = Header("live", true, 0);
    Check(ReplayHeaderProbe.IsLive(livePath) == true, "Detecta partida en curso");
    Check(ReplayHeaderProbe.KeyMissing(livePath), "No analiza antes de recibir la clave");
    var finished = Header("finished", false, 8);
    Check(ReplayHeaderProbe.IsLive(finished) == false, "Detecta partida finalizada");
    Check(!ReplayHeaderProbe.KeyMissing(finished), "Acepta clave finalizada");
    Check(!ReplayHeaderProbe.KeyMissing(Header("plain", false, 0, false)), "Repetición sin cifrar");
    var wide = Header("wide", false, 8, true, true, 7);
    Check(ReplayHeaderProbe.IsLive(wide) == false && !ReplayHeaderProbe.KeyMissing(wide), "Cabecera v7 y texto UTF-16");
    var bad = Path.Combine(folder, "bad.replay"); File.WriteAllText(bad, "invalid");
    Check(ReplayHeaderProbe.IsLive(bad) == null && ReplayHeaderProbe.KeyMissing(bad), "Archivo inválido");
    Check(ReplayHeaderProbe.IsLive(Path.Combine(folder,"missing.replay")) == null, "Archivo desaparecido");
    var d = new TrackerData { Matches = [
        new() { EpicId="A", PlayerName="Uno", ChestsOpened=5, SessionId="one", AnalyzedAtUtc=DateTime.UtcNow },
        new() { EpicId="B", PlayerName="Dos", ChestsOpened=12, SessionId="two", AnalyzedAtUtc=DateTime.UtcNow },
        new() { EpicId="A", PlayerName="Nuevo nombre", ChestsOpened=3, SessionId="three", AnalyzedAtUtc=DateTime.UtcNow.AddSeconds(1) }
    ]};
    var c = new Config { Profile="A", DemosFolder=folder, ShowOverlay=false };
    Check(Profiles.List(d).Count == 2, "Cuentas separadas por Epic ID");
    Check(Profiles.View(d,c).Matches.Sum(m=>m.ChestsOpened) == 8, "No mezcla cofres de otros jugadores");
    Check(Profiles.Current(d,c)?.Name == "Nuevo nombre", "Conserva perfil después de cambiar el nombre");
    var store = new Store(Path.Combine(folder,"data"));
    store.SaveData(d); store.SaveConfig(c);
    Check(store.LoadData().Matches.Count == 3, "Historial persiste");
    Check(store.LoadConfig()?.Profile == "A", "Preferencias persisten");
    var countBefore = new FileInfo(finished).Length;
    ReplayHeaderProbe.IsLive(finished); ReplayHeaderProbe.KeyMissing(finished);
    Check(new FileInfo(finished).Length == countBefore, "La lectura conserva el archivo");
    var importsRoot = Path.Combine(folder, "imports");
    var importedStore = new Store(Path.Combine(importsRoot, "ContadorModern"));
    var local = new TrackerData { AnalyzerVersion = 15, Matches = [
        new() { SessionId="shared", EpicId="A", ChestsOpened=14, AnalyzerVersion=15 },
        new() { SessionId="local-only", EpicId="A", ChestsOpened=4, AnalyzerVersion=15 }
    ]};
    importedStore.SaveData(local);
    var beforeImport = File.ReadAllText(importedStore.DataPath);
    var legacyFolder = Path.Combine(importsRoot, "ChestCounter");
    Directory.CreateDirectory(legacyFolder);
    var legacyPath = Path.Combine(legacyFolder, "data.json");
    var legacy = new TrackerData { AnalyzerVersion=10, Matches = [
        new() { SessionId="shared", EpicId="A", ChestsOpened=12, AnalyzerVersion=10 },
        new() { SessionId="old-only", EpicId="A", ChestsOpened=7, AnalyzerVersion=10 },
        new() { SessionId="other-account", EpicId="B", ChestsOpened=9, AnalyzerVersion=10 }
    ]};
    var legacyText = JsonSerializer.Serialize(legacy);
    File.WriteAllText(legacyPath, legacyText);
    var result = HistoryImporter.Import(importedStore, local, importsRoot);
    Check(result.Added == 2 && result.Updated == 0, "Importa partidas antiguas sin reemplazar el lector nuevo");
    Check(local.Matches.Sum(m=>m.ChestsOpened) == 34 && local.Matches.Count == 4, "Total combinado sin duplicados");
    Check(Profiles.View(local,new Config {Profile="A"}).Matches.Sum(m=>m.ChestsOpened) == 25, "Importacion conserva separacion de cuentas");
    Check(File.ReadAllText(legacyPath) == legacyText, "No modifica el contador anterior");
    var backups = Directory.GetDirectories(Path.Combine(importedStore.Folder,"Backups"));
    Check(backups.Length == 1 && File.ReadAllText(Path.Combine(backups[0],"ContadorModern-data.json")) == beforeImport, "Respalda historial antes de importar");
    Check(importedStore.LoadData().Matches.Count == 4, "Importacion persiste en disco");
    result = HistoryImporter.Import(importedStore, local, importsRoot);
    Check(result.Added == 0 && result.Updated == 0 && Directory.GetDirectories(Path.Combine(importedStore.Folder,"Backups")).Length == 1, "Reabrir no duplica ni crea respaldos innecesarios");
    var oldestFolder = Path.Combine(importsRoot,"ChestTracker"); Directory.CreateDirectory(oldestFolder);
    File.WriteAllText(Path.Combine(oldestFolder,"data.json"), "{incomplete");
    result = HistoryImporter.Import(importedStore, local, importsRoot);
    Check(result.Warnings.Count == 1 && local.Matches.Count == 4, "Historial corrupto no borra partidas existentes");
    var newer = new TrackerData { Matches = [new() {SessionId="shared", EpicId="A", ChestsOpened=15, AnalyzerVersion=16}] };
    File.WriteAllText(Path.Combine(oldestFolder,"data.json"), JsonSerializer.Serialize(newer));
    result = HistoryImporter.Import(importedStore, local, importsRoot);
    Check(result.Updated == 1 && result.Warnings.Count == 0 && local.Matches.Sum(m=>m.ChestsOpened) == 35, "Reintenta historial reparado y respeta analizador mas reciente");
    newer.Matches[0].EpicId="CONFLICT";
    File.WriteAllText(Path.Combine(oldestFolder,"data.json"), JsonSerializer.Serialize(newer));
    result = HistoryImporter.Import(importedStore, local, importsRoot);
    Check(result.Warnings.Count == 1 && local.Matches.Sum(m=>m.ChestsOpened) == 35, "Conflicto de cuenta se rechaza sin modificar totales");
    var emptyStore = new Store(Path.Combine(folder,"fresh"));
    result = HistoryImporter.Import(emptyStore,new TrackerData(),Path.Combine(folder,"no-legacy"));
    Check(result.Added == 0 && !File.Exists(emptyStore.DataPath), "Equipo sin contador previo inicia sin importaciones");
    var blockedStore = new Store(Path.Combine(folder,"backup-blocked"));
    var blockedData = new TrackerData(); blockedStore.SaveData(blockedData);
    var blockedBefore = File.ReadAllText(blockedStore.DataPath);
    File.WriteAllText(Path.Combine(blockedStore.Folder,"Backups"),"not a directory");
    bool backupFailed = false;
    try { HistoryImporter.Import(blockedStore,blockedData,importsRoot); } catch (IOException) { backupFailed = true; }
    Check(backupFailed && blockedData.Matches.Count == 0 && File.ReadAllText(blockedStore.DataPath) == blockedBefore, "Si falla el respaldo no cambia memoria ni historial");
    if (args.Length == 2 && args[0] == "--histories")
    {
        var realStore = new Store(Path.Combine(folder,"real-history-copy"));
        File.Copy(Path.Combine(args[1],"ContadorModern","data.json"),realStore.DataPath);
        var realData = realStore.LoadData();
        result = HistoryImporter.Import(realStore,realData,args[1]);
        Console.WriteLine($"Copia de historial real: {realData.Matches.Count} partidas, {realData.Matches.Sum(m=>m.ChestsOpened)} cofres, {result.Added} incorporadas.");
        Check(realData.Matches.Select(m=>m.SessionId).Distinct().Count() == realData.Matches.Count, "Historial real combinado sin duplicados");
        result = HistoryImporter.Import(realStore,realData,args[1]);
        Check(result.Added == 0 && result.Updated == 0, "Historial real no cambia al reabrir");
    }
    Console.WriteLine($"{checks} comprobaciones correctas.");
}
finally { Directory.Delete(folder, recursive:true); }
