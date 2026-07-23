using Game_Library.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Game_Library.Services
{
    public class FutureGameDataService
    {
        private static readonly FutureGameDataService _instance = new FutureGameDataService();
        public static FutureGameDataService Instance => _instance;

        private readonly string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "future_games.json");
        private readonly ReaderWriterLockSlim _fileLock = new ReaderWriterLockSlim();

        public List<FutureGameModel> FutureGames { get; set; } = new List<FutureGameModel>();

        private FutureGameDataService()
        {
            InitializeData();
        }

        public void InitializeData()
        {
            _fileLock.EnterReadLock();
            try
            {
                if (File.Exists(jsonPath))
                {
                    string jsonContent = File.ReadAllText(jsonPath, Encoding.UTF8);
                    FutureGames = JsonSerializer.Deserialize<List<FutureGameModel>>(jsonContent) ?? new List<FutureGameModel>();
                }
            }
            catch (Exception ex)
            {
                GameDataService.Instance.Log($"Lỗi nạp future_games.json: {ex.Message}");
                FutureGames = new List<FutureGameModel>();
            }
            finally
            {
                _fileLock.ExitReadLock();
            }
        }

        public async Task<bool> SaveFutureGamesAsync()
        {
            return await Task.Run(() =>
            {
                _fileLock.EnterWriteLock();
                try
                {
                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    };
                    string jsonString = JsonSerializer.Serialize(FutureGames, options);
                    File.WriteAllText(jsonPath, jsonString, Encoding.UTF8);
                    return true;
                }
                catch (Exception ex)
                {
                    GameDataService.Instance.Log($"Lỗi lưu future_games.json: {ex.Message}");
                    return false;
                }
                finally
                {
                    _fileLock.ExitWriteLock();
                }
            });
        }
    }
}
