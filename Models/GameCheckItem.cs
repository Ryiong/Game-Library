using Game_Library.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Game_Library.Models
{
    public class GameCheckItem : ViewModelBase
    {
        private bool _isSelected;
        public string Id { get; set; }
        public string Title { get; set; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
               SetProperty(ref _isSelected, value);
            }
        }

    }

}
