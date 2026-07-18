using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Game_Library.Models
{
    public class GameCheckItem : INotifyPropertyChanged
    {
        private bool _isSelected;
        public string Id { get; set; }
        public string Title { get; set; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

}
