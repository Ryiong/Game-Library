using System;
using System.Collections.Generic;
using System.Text;

namespace Game_Library.Models
{
    public class GameModels
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public string AddedDate { get; set; }
        public string ReleaseDate { get; set; }
        public bool isFavorite { get; set; }
        public bool isNSFW { get; set; }
        public string SeriesId { get; set; }
        public string Thumbnail { get; set; }
        public List<string> ImageInGame { get; set; } = new List<string>();
        public string FolderName { get; set; }
        public string MainFile { get; set; }
        public string LastPlayedText { get; set; }
    }
}
