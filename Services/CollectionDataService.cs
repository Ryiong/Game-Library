using Game_Library.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Game_Library.Services
{
    public class CollectionDataService
    {
        private static readonly CollectionDataService _instance = new CollectionDataService();
        public static CollectionDataService Instance => _instance;

        private readonly string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "collections.json");
        private readonly ReaderWriterLockSlim _fileLock = new ReaderWriterLockSlim();
        public List<CollectionModel> Collections { get; set; } = new List<CollectionModel>();

        private CollectionDataService()
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
                    Collections = JsonSerializer.Deserialize<List<CollectionModel>>(jsonContent) ?? new List<CollectionModel>();
                }
            }
            catch (Exception ex)
            {
                GameDataService.Instance.Log($"Lỗi nạp collections.json: {ex.Message}");
                Collections = new List<CollectionModel>();
            }
            finally
            {
                _fileLock.ExitReadLock();
            }
        }

        public async Task<bool> SaveCollectionsAsync()
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
                    string jsonString = JsonSerializer.Serialize(Collections, options);
                    File.WriteAllText(jsonPath, jsonString, Encoding.UTF8);
                    return true;
                }
                catch (Exception ex)
                {
                    GameDataService.Instance.Log($"Lỗi lưu collections.json: {ex.Message}");
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
