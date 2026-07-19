using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Media.Animation;
using System.Threading.Tasks;

namespace Game_Library.Services
{
    public class GameHttpServer
    {
        private HttpListener _listener;
        private string _gameFolderPath;
        private bool _isRunning = false;
        private readonly int _port = 8080;

        public string BaseUrl => $"http://localhost:{_port}/";

        public void Start(string gameFolderPath)
        {
            if (_isRunning) Stop();

            _gameFolderPath = Path.GetFullPath(gameFolderPath);
            _listener = new HttpListener();
            //string prefix = BaseUrl;
            //if (!prefix.EndsWith("/"))
            //{
            //    prefix += "/";
            //}
            _listener.Prefixes.Add(BaseUrl);
            _listener.Start();
            _isRunning = true;

            Task.Run(() => ListenLoop());
        }

        private async Task ListenLoop()
        {
            while (_isRunning)
            {
                try
                {
                    var context = await _listener.GetContextAsync();
                    ProcessRequest(context);
                }
                catch { /* Bỏ qua lỗi khi tắt server đột ngột */ }
            }
        }

        private void ProcessRequest(HttpListenerContext context)
        {
            try
            {
                string requestUrl = context.Request.Url.LocalPath.TrimStart('/');

                if (string.IsNullOrEmpty(requestUrl)) requestUrl = "index.html";

                string combinedPath = Path.Combine(_gameFolderPath, requestUrl);
                string finalFullPath = Path.GetFullPath(combinedPath);

                if (!finalFullPath.StartsWith(_gameFolderPath, StringComparison.OrdinalIgnoreCase))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    return;
                }

                if (File.Exists(finalFullPath))
                {
                    byte[] responseBytes = File.ReadAllBytes(finalFullPath);

                    string ext = Path.GetExtension(finalFullPath).ToLower();
                    context.Response.ContentType = ext switch
                    {
                        ".html" or ".htm" => "text/html; charset=utf-8",
                        ".js" => "application/javascript",
                        ".css" => "text/css",
                        ".png" => "image/png",
                        ".jpg" or ".jpeg" => "image/jpeg",
                        ".gif" => "image/gif",
                        ".svg" => "image/svg+xml",
                        ".mp3" => "audio/mpeg",
                        ".wav" => "audio/wav",
                        _ => "application/octet-stream",
                    };

                    context.Response.ContentLength64 = responseBytes.Length;
                    context.Response.OutputStream.Write(responseBytes, 0, responseBytes.Length);
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                }
            }
            catch (Exception)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
            finally
            {
                try { context.Response.OutputStream.Close(); } catch { }
            }
        }

        public void Stop()
        {
            if (_isRunning)
            {
                _isRunning = false;
                _listener?.Stop();
                _listener?.Close();
            }
        }
    }
}
