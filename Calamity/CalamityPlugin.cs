using Dalamud.Plugin;
using System;
using System.ComponentModel;
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

        public void Dispose()
        {
            StopServer();
            GC.SuppressFinalize(this);
        }

        #region HttpServer

        private readonly string[] PREFIXES = new string[] { "localhost", "127.0.0.1" };
        private readonly int PORT = 37435;
        private readonly string UNLOAD_DALAMUD_PATH = "/unload_dalamud";

        private HttpListener Listener { get; set; }

        private CancellationTokenSource ShutdownTokenSource { get; set; }

        private Task ListenerTask { get; set; }

        private void ListenerLoop(CancellationToken token)
        {
            while (Listener.IsListening)
            {
                HttpListenerContext context;
                try
                {
                    context = Listener.GetContext();
                }
                catch (HttpListenerException ex)
                {
                    // Shutdown usually throws some sort of I/O exception.
                    // If we're shuttiing down, ignore it.
                    if (token.IsCancellationRequested)
                        return;
                    else
                        throw ex;
                }

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
        }

        private void StartServer()
        {
            Listener = new HttpListener();
            ShutdownTokenSource = new CancellationTokenSource();

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

            ListenerTask = Task.Factory.StartNew(() => ListenerLoop(ShutdownTokenSource.Token));

            PluginLog.Information("The Seventh Umbral Calamity looms");
        }

        public void StopServer()
        {
            if (!Listener.IsListening)
                return;

            ShutdownTokenSource?.Cancel();
            Listener?.Stop();
            Listener?.Close();
            ListenerTask?.Wait();
        }

        #endregion
    }
}