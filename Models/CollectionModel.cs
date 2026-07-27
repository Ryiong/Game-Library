using System;
using System.Collections.Generic;
using System.Text;

namespace Game_Library.Models
{
    public class CollectionModel
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = string.Empty;
        public string Thumbnail { get; set; } = string.Empty;
        public List<string> GameIds { get; set; } = new List<string>();
    }
}
