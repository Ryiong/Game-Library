using Game_Library.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Game_Library.Services
{
    public class GameDataService
    {
        private static readonly GameDataService _instance = new GameDataService();
        public static GameDataService Instance => _instance;

        private readonly string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "game.json");
        private readonly string backupFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "backups");

        private readonly ReaderWriterLockSlim _fileLock = new ReaderWriterLockSlim();

        public delegate void LogChangedHandler(string logMessage);
        public event LogChangedHandler OnLogChanged;

        public List<GameModels> AllGames { get; set; } = new List<GameModels>();

        public event Action<string> OnStatusChanged;
        private GameDataService()
        {
            try
            {
                if (!Directory.Exists(backupFolder)) Directory.CreateDirectory(backupFolder);
            }
            catch (Exception ex)
            {
                LogToFile($"Không thể khởi tạo thư mục Sao lưu (Backup): {ex.Message}");
            }
        }

        /// <summary>
        /// TRY-CATCH LOAD FILE: Load data safe from JSON, auto reload from backup if crash
        /// </summary>
        public void InitializeData()
        {
            _fileLock.EnterReadLock();
            try
            {
                if (File.Exists(jsonPath))
                {
                    string jsonContent = File.ReadAllText(jsonPath, Encoding.UTF8);
                    AllGames = JsonSerializer.Deserialize<List<GameModels>>(jsonContent) ?? new List<GameModels>();
                }
            }
            catch (Exception ex)
            {
                LogToFile($"Tệp dữ liệu game.json gốc bị lỗi hoặc hư hại: {ex.Message}. Đang kích hoạt tiến trình phục hồi dữ liệu...");
                RestoreLatestBackup();
            }
            finally
            {
                _fileLock.ExitReadLock();
            }
        }
        ///<summary>
        /// UPDATE SERVICE: Save or Update metadata game in background
        /// </summary>
        public async Task<bool> SaveGameAsync(GameModels targetGame, bool isEditMode)
        {
            OnStatusChanged?.Invoke("System: Ứng dụng đang thực hiện tối ưu hóa ảnh và đóng gói dữ liệu ngầm...");
            Log("System: Đang tối ưu hoá ảnh và chuyển dữ liệu");
            return await Task.Run(() =>
            {
                _fileLock.EnterWriteLock();
                try
                {
                    CreateBackup();

                    if (!isEditMode)
                    {
                        AllGames.Add(targetGame);
                    }
                    else
                    {
                        int idx = AllGames.FindIndex(g => g.Id == targetGame.Id);
                        if (idx != -1) AllGames[idx] = targetGame;
                    }

                    foreach (var game in AllGames.Where(g => g.Id != targetGame.Id))
                    {
                        if (targetGame.RelatedGameIds != null && targetGame.RelatedGameIds.Contains(game.Id))
                        {
                            if (game.RelatedGameIds == null) game.RelatedGameIds = new List<string>();
                            if (!game.RelatedGameIds.Contains(targetGame.Id)) game.RelatedGameIds.Add(targetGame.Id);
                        }
                        else
                        {
                            game.RelatedGameIds?.Remove(targetGame.Id);
                        }
                    }

                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    };
                    Log("Đã lưu thành công game");

                    string jsonString = JsonSerializer.Serialize(AllGames, options);
                    File.WriteAllText(jsonPath, jsonString, Encoding.UTF8);

                    OnStatusChanged?.Invoke("Server Status: Online");
                    return true;
                }
                catch (Exception ex)
                {
                    LogToFile($"Lỗi phát sinh trong tiến trình SaveGameAsync: {ex.Message}");
                    Log("Lỗi phát sinh trong tiến trình Save Game");
                    OnStatusChanged?.Invoke("Server Status: Xảy ra lỗi khi lưu tệp tin");
                    return false;
                }
                finally
                {
                    _fileLock.ExitWriteLock();
                }
            });
        }

        ///<summary>
        /// DELETE GAME SERVICE: None Null Reference Exception
        /// </summary>
        public async Task<bool> DeleteGameAsync(string gameId, string centralStoragePath)
        {
            OnStatusChanged?.Invoke("System: Đang gỡ bỏ liên kết hệ thống và xóa dữ liệu nguồn...");

            return await Task.Run(() =>
            {
                _fileLock.EnterWriteLock();
                try
                {
                    CreateBackup();

                    var gameToRemove = AllGames.FirstOrDefault(g => g.Id == gameId);
                    if (gameToRemove == null) return false;

                    AllGames.Remove(gameToRemove);

                    foreach (var game in AllGames)
                    {
                        game.RelatedGameIds?.Remove(gameId);
                    }

                    var options = new JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
                    File.WriteAllText(jsonPath, JsonSerializer.Serialize(AllGames, options), Encoding.UTF8);

                    string gameFolder = Path.Combine(centralStoragePath, gameId);
                    if (Directory.Exists(gameFolder))
                    {
                        Directory.Delete(gameFolder, true);
                    }

                    OnStatusChanged?.Invoke("Server Status: Online");
                    Log("Đã xoá game khỏi thư viện");
                    return true;
                }
                catch (Exception ex)
                {
                    LogToFile($"Lỗi hệ thống khi xóa game có mã ID [{gameId}]: {ex.Message}");
                    OnStatusChanged?.Invoke("Server Status: Xảy ra lỗi khi xóa dữ liệu");
                    return false;
                }
                finally
                {
                    _fileLock.ExitWriteLock();
                }
            });
        }

        ///<summary>
        /// BACKUP ENGINE
        /// </summary>
        private void CreateBackup()
        {
            try
            {
                if (File.Exists(jsonPath))
                {
                    string backupPath = Path.Combine(backupFolder, $"game_backup_{DateTime.Now:yyyyMMdd_HHmmss}.json");
                    File.Copy(jsonPath, backupPath, true);

                    var files = new DirectoryInfo(backupFolder).GetFiles("*.json")
                                    .OrderByDescending(f => f.CreationTime).Skip(2);
                    foreach (var file in files) file.Delete();
                }
            }
            catch { /* Chặn lỗi âm thầm để không ảnh hưởng luồng công việc chính của người dùng */ }
        }
        private void RestoreLatestBackup()
        {
            try
            {
                var latestBackup = new DirectoryInfo(backupFolder).GetFiles("*.json")
                                    .OrderByDescending(f => f.CreationTime).FirstOrDefault();
                if (latestBackup != null)
                {
                    File.Copy(latestBackup.FullName, jsonPath, true);
                    string jsonContent = File.ReadAllText(jsonPath, Encoding.UTF8);
                    AllGames = JsonSerializer.Deserialize<List<GameModels>>(jsonContent) ?? new List<GameModels>();
                }
            }
            catch
            {
                AllGames = new List<GameModels>();
            }
        }

        private void LogToFile(string message)
        {
            try
            {
                string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app_error.log");
                File.AppendAllText(logPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}");
            }
            catch { }
        }

        public void Log(string message)
        {
            string formattedLog = $"{message}";
            OnLogChanged?.Invoke(formattedLog);
        }
    }
}
