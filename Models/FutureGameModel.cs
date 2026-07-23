using Game_Library.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json.Serialization;
using System.Windows.Media.Imaging;

namespace Game_Library.Models
{
    public class FutureGameModel : ViewModelBase
    {
        private string _id;
        private string _title;
        private string _type;
        private string _description;
        private string _releaseDate;
        private string _thumbnail;
        private bool _isActivated;
        private bool _isNsfw = false;
        private List<string> _relatedGameIds = new List<string>();

        public string Id { get => _id; set => SetProperty(ref _id, value); }
        public string Title { get => _title; set => SetProperty(ref _title, value); }
        public string Type { get => _type; set => SetProperty(ref _type, value); }
        public string Description { get => _description; set => SetProperty(ref _description, value); }
        public string ReleaseDate { get => _releaseDate; set => SetProperty(ref _releaseDate, value); }
        public string Thumbnail { get => _thumbnail; set => SetProperty(ref _thumbnail, value); }

        public bool IsActivated
        {
            get => _isActivated;
            set => SetProperty(ref _isActivated, value);
        }

        public bool IsNsfw { get => _isNsfw; set => SetProperty(ref _isNsfw, value); }

        public List<string> RelatedGameIds
        {
            get => _relatedGameIds;
            set => SetProperty(ref _relatedGameIds, value);
        }

        [JsonIgnore]
        public BitmapImage ThumbnailSource => LoadImageUnloaded(Thumbnail);

        private BitmapImage LoadImageUnloaded(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) return null;
            string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);
            if (!File.Exists(fullPath)) return null;

            try
            {
                using (var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = stream;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    return bitmap;
                }
            }
            catch { return null; }
        }
    }
}
