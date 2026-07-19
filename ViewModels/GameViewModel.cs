using Game_Library.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace Game_Library.ViewModels
{
    public class GameViewModel : ViewModelBase
    {
        private GameModels _gameData;
        private string _playingTitle;
        private bool _isLoadingVisible = true;
        public GameModels GameData
        {
            get => _gameData;
            set => SetProperty(ref _gameData, value);
        }

        public string PlayingTitle
        {
            get => _playingTitle;
            set => SetProperty(ref _playingTitle, value);
        }

        public bool IsLoadingVisible
        {
            get => _isLoadingVisible;
            set => SetProperty(ref _isLoadingVisible, value);
        }

        public ICommand ExitGameCommand { get; }

        public event Action RequestCloseGame;

        public GameViewModel(GameModels selectedGame)
        {
            GameData = selectedGame;
            PlayingTitle = selectedGame.Title.ToUpper();
            ExitGameCommand = new RelayCommand(_ => ExecuteExitGame());
        }

        private void ExecuteExitGame()
        {
            RequestCloseGame?.Invoke();
            if (System.Windows.Application.Current.MainWindow is MainWindow main)
            {
                main.ViewModel.ReturnCommand.Execute(null);
            }
        }
    }
}
