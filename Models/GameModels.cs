using System;
using System.Collections.Generic;
using System.Text;

namespace Game_Library.Models
{
    public class GameModels
    {
        public int idGame {  get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public string Thumbnail {  get; set; }
        public bool isFavorite { get; set; }
        public string AddedDate { get; set; }
        public string ReleaseDate { get; set; }
        public string LastPlayText { get; set; }
    }
}
