using System.Text.Json;
using ChestCounter;

namespace Contador.Engine;

/// <summary>Transparent browser HUD fed by the exact immutable view sent to the native overlay.</summary>
public static class ObsHud
{
    private static readonly object Sync = new();
    private static readonly Lazy<string> Coin = new(() => "data:image/png;base64," + Convert.ToBase64String(TokenIcon.Frame(64)));

    private static string Payload(OverlayView v) => JsonSerializer.Serialize(new
    { total=v.Total, last=v.Last ?? "—", session=v.Session, treasures=v.Treasures, inMatch=v.CurrentLive });

    public static void Write(Store store, OverlayView view)
    {
        lock (Sync)
        {
            // Browser script tags can load local files in OBS without a server or fetch/file CORS.
            Atomic(Path.Combine(store.Folder,"overlay-data.js"), "window.renderOverlay(" + Payload(view) + ");");
            var html = Render(view);
            if (!File.Exists(store.OverlayPath) || File.ReadAllText(store.OverlayPath) != html)
                Atomic(store.OverlayPath,html);
        }
    }

    private static void Atomic(string path, string value)
    {
        var temporary = path + ".tmp";
        File.WriteAllText(temporary,value);
        File.Move(temporary,path,true);
    }

    public static string Render(OverlayView view) => $$"""
        <!doctype html>
        <html lang="es"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width, initial-scale=1">
        <title>Contador · Overlay HUD</title>
        <style>
        html,body{margin:0;background:transparent!important;overflow:hidden;color:#fff}
        .hud{box-sizing:border-box;width:256px;padding:8px 20px 8px 8px;text-align:right;
          font:italic 600 18px/1.22 "Bahnschrift SemiCondensed","Bahnschrift",sans-serif;font-stretch:semi-condensed}
        .outline{paint-order:stroke fill;-webkit-text-stroke:2px #0b0f18;text-shadow:1px 1px 0 #0b0f18}
        .total{display:flex;align-items:center;justify-content:flex-end;gap:8px;height:48px;margin-bottom:2px}
        .coin{width:34px;height:34px;object-fit:contain;flex:none}
        .number{font-size:40px;font-weight:700;font-family:"Bahnschrift Condensed","Bahnschrift",sans-serif;font-stretch:condensed;-webkit-text-stroke:3px #0b0f18;line-height:1}
        .row{height:22px;white-space:nowrap}.label{color:#ffe9a6}.sep{white-space:pre}.treasure{color:#ffd23f}
        .dot{display:none;width:9px;height:9px;border-radius:50%;background:#ff3b30;box-shadow:0 0 0 1px #0b0f18;margin-right:6px;vertical-align:middle}
        .playing .dot{display:inline-block}
        </style></head><body>
        <main class="hud" aria-label="Contador de cofres">
          <div class="total"><img class="coin" src="{{Coin.Value}}" alt=""><span id="total" class="number outline"></span></div>
          <div class="row"><span class="dot" aria-hidden="true"></span><span id="state" class="label outline"></span><span id="state-value" class="outline"></span></div>
          <div class="row outline"><span class="label">Última partida<span class="sep">  •  </span></span><span id="last"></span></div>
          <div class="row outline"><span class="label">Esta sesión<span class="sep">  •  </span></span><span id="session"></span></div>
          <div class="row outline"><span class="label">Tesoros<span class="sep">  •  </span></span><span id="treasures" class="treasure"></span></div>
        </main>
        <script>
        window.renderOverlay=function(d){
          for(const key of ['total','last','session','treasures'])document.getElementById(key).textContent=d[key];
          document.body.classList.toggle('playing',d.inMatch);
          document.getElementById('state').textContent=d.inMatch?'En partida':'Estado  •  ';
          document.getElementById('state-value').textContent=d.inMatch?'':'Esperando';
        };
        window.renderOverlay({{Payload(view)}});
        let loading=false;
        function refresh(){if(loading)return;loading=true;const s=document.createElement('script');
          s.src='overlay-data.js?t='+Date.now();s.onload=s.onerror=()=>{s.remove();loading=false};document.head.appendChild(s)}
        refresh();setInterval(refresh,2000);
        </script></body></html>
        """;
}
