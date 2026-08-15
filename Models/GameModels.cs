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
        public string Thumbnail { get; set; }
        public string FolderName { get; set; }
        public string MainFile { get; set; }

        public bool IsValid(out string errorMessage)
        {
            errorMessage = string.Empty;
            if (string.IsNullOrWhiteSpace(Title))
            {
                errorMessage = "Tên game không được để trống";
                return false;
            }
            if (string.IsNullOrWhiteSpace(Type))
            {
                errorMessage = "Phân loại game bắt buộc phải chọn";
            }
            return true;
        }
    }
}
