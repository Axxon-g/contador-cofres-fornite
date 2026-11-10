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
    var now = DateTime.UtcNow;
    var presenceFolder = Path.Combine(folder,"presence"); Directory.CreateDirectory(presenceFolder);
    var presenceFile = Path.Combine(presenceFolder,"match.replay");
    File.Copy(livePath,presenceFile);
    Check(MatchPresence.IsRecording(presenceFolder,now), "Detecta partida activa sin descifrar cofres");
    File.Copy(finished,presenceFile,true);
    Check(!MatchPresence.IsRecording(presenceFolder,now), "Fin de partida retira aviso inmediatamente");
    File.Copy(livePath,presenceFile,true); File.SetLastWriteTimeUtc(presenceFile,now.AddMinutes(-5));
    Check(!MatchPresence.IsRecording(presenceFolder,now), "Repeticion abandonada no indica partida activa");
    Check(!MatchPresence.IsRecording(Path.Combine(folder,"absent"),now), "Carpeta ausente no indica partida activa");
    var sessionData = new TrackerData { Matches = [new() {SessionId="baseline",ChestsOpened=17,MatchStartUtc=now.AddMinutes(-10),DurationSeconds=600}] };
    var session = new SessionCounter(now); session.ExcludeHistory(sessionData);
    sessionData.Matches.Add(new() {SessionId="imported",ChestsOpened=800,MatchStartUtc=now.AddDays(-1),AnalyzedAtUtc=now.AddSeconds(5)});
    Check(session.Matches(sessionData).Count == 0, "Sesion excluye historial y repeticiones antiguas reanalizadas");
    sessionData.Matches.Add(new() {SessionId="new",ChestsOpened=9,MatchStartUtc=now.AddMinutes(-1),DurationSeconds=120,AnalyzedAtUtc=now.AddSeconds(60)});
    var sessionOverlay = session.Overlay(sessionData,true);
    Check(sessionOverlay.CurrentLive && sessionOverlay.Session == "9" && sessionOverlay.Last == "9", "Overlay refleja partida y sesion real");
    sessionData.Matches.Add(new() {SessionId="next",ChestsOpened=3,MatchStartUtc=now.AddSeconds(100),DurationSeconds=60});
    sessionOverlay = session.Overlay(sessionData,false);
    Check(!sessionOverlay.CurrentLive && sessionOverlay.Session == "12" && sessionOverlay.Last == "3", "Ultima partida y sesion se calculan por separado");
    if (args.Length == 2 && args[0] == "--overlay-preview")
    {
        var painted = 0;
        NativeOverlay.DrawCounterIcon = (dc,x,y,size) => { var ok = TokenIcon.Draw(dc,x,y,size); if(ok) Interlocked.Increment(ref painted); return ok; };
        Directory.CreateDirectory(args[1]);
        NativeOverlay.SetStyle("hud"); NativeOverlay.SetAppearance(100,100);
        NativeOverlay.Start(-20000,-20000,true,(_,_)=>{},_=>{});
        foreach(var active in new[] {false,true})
        {
            NativeOverlay.Update(new OverlayView("2290",active?"…":"17","última partida",active,active?"EN PARTIDA":null,[],"22","83","17"));
            Thread.Sleep(400);
            Check(NativeOverlay.Snapshot(Path.Combine(args[1],active?"overlay-en-partida.png":"overlay-espera.png")),"Captura overlay");
        }
        NativeOverlay.Stop();
        Check(painted > 0,"Moneda integrada se dibuja mediante icono nativo");
    }
    foreach(var size in new[] {16,24,32,48,64,128,256})
    {
        var frame = TokenIcon.Frame(size);
        Check(frame[0] == 137 && System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(frame.AsSpan(16,4)) == size,"Icono integrado multirresolucion " + size);
    }
    var obsStore = new Store(Path.Combine(folder,"obs"));
    var hudView = new OverlayView("2318","28","última partida",false,null,[],"23","0","28");
    ObsHud.Write(obsStore,hudView);
    var browserHud=File.ReadAllText(obsStore.OverlayPath);
    Check(browserHud.Contains("background:transparent!important") && !browserHud.Contains("class=\"box\""),"OBS usa HUD transparente sin tarjeta antigua");
    Check(browserHud.Contains("data:image/png;base64,") && browserHud.Contains("Última partida") && browserHud.Contains("Esta sesión") && browserHud.Contains("Tesoros"),"OBS usa moneda integrada y mismas etiquetas");
    Check(browserHud.Contains("overlay-data.js?t=") && !browserHud.Contains("http://") && !browserHud.Contains("https://") && !browserHud.Contains("http-equiv=\"refresh\""),"OBS actualiza localmente sin recargar ni acceder a red");
    var dataFile=Path.Combine(obsStore.Folder,"overlay-data.js");
    Check(File.ReadAllText(dataFile).Contains("\"inMatch\":false"),"OBS recibe estado en espera");
    ObsHud.Write(obsStore,hudView with {CurrentLive=true,Session="28"});
    var js=File.ReadAllText(dataFile);
    Check(js.Contains("\"inMatch\":true") && js.Contains("\"session\":\"28\""),"OBS actualiza estado y sesion desde la vista compartida");
    try
    {
        ReportWriter.OverlayWriter=(target,_)=>ObsHud.Write(target,hudView with {CurrentLive=true});
        ReportWriter.WriteAll(obsStore,new Config(),new TrackerData());
        var afterReport=File.ReadAllText(obsStore.OverlayPath);
        Check(afterReport.Contains("class=\"hud\"") && afterReport.Contains("\"inMatch\":true"),"Generar informe no restaura tarjeta ni borra estado de partida");
    }
    finally { ReportWriter.OverlayWriter=null; }
    if(args.Length==2 && args[0]=="--obs-preview")
    {
        var previewStore = new Store(args[1]);
        ObsHud.Write(previewStore,hudView);
    }
    try
    {
        ReportWriter.ReportBrandImage=()=>"data:image/png;base64,"+Convert.ToBase64String(TokenIcon.Frame(128));
        var styledReport=ReportWriter.Report(new Config(),sessionData,Profiles.List(sessionData));
        Check(styledReport.Contains("--bg:#101C2E") && styledReport.Contains("--card:#182D48") && styledReport.Contains("background:#6E2636"),"Reporte usa colores del launcher");
        Check(styledReport.Contains("class=\"brand-coin\"") && styledReport.Contains("data:image/png;base64,"),"Reporte incluye moneda sin archivos externos");
        Check(!styledReport.Contains("prefers-color-scheme:light") && !styledReport.Contains("--card:#f2e6c9"),"Reporte no vuelve al tema antiguo claro");
        Check(styledReport.Contains("Cofres por día") && styledReport.Contains("Últimas partidas") && styledReport.Contains("Por tipo de cofre"),"Reporte conserva graficos y tablas");
    }
    finally {ReportWriter.ReportBrandImage=null;}
    Console.WriteLine($"{checks} comprobaciones correctas.");
}
finally { Directory.Delete(folder, recursive:true); }
