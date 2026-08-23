using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Web.WebView2.Core;

namespace Game_Library.Services
{
    public static class WebView2Helper
    {
        private static CoreWebView2Environment _env;
        private static readonly SemaphoreSlim _lock = new(1, 1);

        public static async Task<CoreWebView2Environment> GetEnvironmentAsync()
        {
            if (_env != null) return _env;

            await _lock.WaitAsync();
            try
            {
                if (_env == null)
                {
                    string userDataFolder = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "GameLibrary", "WebView2");

                    _env = await CoreWebView2Environment.CreateAsync(
                        browserExecutableFolder: null,
                        userDataFolder: userDataFolder);
                }
            }
            finally
            {
                _lock.Release();
            }

            return _env;
        }
    }
}
