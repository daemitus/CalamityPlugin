using Dalamud.Plugin;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Calamity
{
    public class CalamityPlugin : IDalamudPlugin
    {
        public string Name => "Calamity";

        private Dalamud.Dalamud dalamud;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.dalamud = (Dalamud.Dalamud)pluginInterface.GetType()
                .GetField("dalamud", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(pluginInterface);

            StartServer();
        }

        public void Dispose() => StopServer();

        #region HttpServer

        private readonly string[] PREFIXES = new string[] { "localhost", "127.0.0.1" };
        private readonly int PORT = 37435;
        private readonly string UNLOAD_DALAMUD_PATH = "/unload_dalamud";
        private readonly CancellationTokenSource tokenSource = new CancellationTokenSource();

        private HttpListener Listener { get; } = new HttpListener();

        private void ListenerLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var context = Listener.GetContext();
                if (context.Request.Url.PathAndQuery == UNLOAD_DALAMUD_PATH)
                {
                    context.Response.StatusCode = 200;
                    context.Response.ContentLength64 = 0;
                    context.Response.OutputStream.Close();

                    StopServer();

                    PluginLog.Information($"Unloading dalamud");
                    dalamud.Unload();

                    break;  // If it can even get this 
                }
                else
                {
                    context.Response.StatusCode = 404;
                    context.Response.ContentLength64 = 0;
                    context.Response.OutputStream.Close();
                }
            }
            Listener.Stop();
        }

        private void StartServer()
        {
            if (Listener.IsListening)
            {
                PluginLog.Warning($"Listener is already listening");
                return;
            }

            Listener.Prefixes.Clear();
            foreach (var prefix in PREFIXES)
                Listener.Prefixes.Add($"http://{prefix}:{PORT}/");

            try
            {
                Listener.Start();
            }
            catch (HttpListenerException hlex)
            {
                PluginLog.Error(hlex, $"Could not start Listener");
                return;
            }

            Task.Factory.StartNew(() => ListenerLoop(tokenSource.Token));

            PluginLog.Information("The Seventh Umbral Calamity looms");
        }

        public void StopServer()
        {
            if (!Listener.IsListening)
                return;
            tokenSource.Cancel();
        }

        #endregion
    }
}