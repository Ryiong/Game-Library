using Game_Library.Models;
using Game_Library.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace Game_Library.ViewModels
{
    public class GameViewModel : ViewModelBase
    {
        private double _volumeLevel = 100;
        private bool _isMuted = false;
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

        public double VolumeLevel
        {
            get => _volumeLevel;
            set
            {
                if (SetProperty(ref _volumeLevel, value))
                {
                    OnVolumeChanged();
                }
            }
        }

        public bool IsMuted
        {
            get => _isMuted;
            set
            {
                if (SetProperty(ref _isMuted, value))
                {
                    OnMuteStateChanged();
                }
            }
        }


        #region Action Callbacks & Events (Kết nối trực tiếp tới Code-Behind GameView)
        public Action<double> SetVolumeAction { get; set; }
        public Action<bool> SetMuteAction { get; set; }
        public event Action RequestCloseGame;
        public Action ToggleFullscreenAction { get; set; }
        #endregion

        #region Commands
        public ICommand ToggleMuteCommand { get; }
        public ICommand FullscreenCommand { get; }
        public ICommand ExitGameCommand { get; }
        #endregion
        public GameViewModel(GameModels selectedGame)
        {
            GameData = selectedGame;
            PlayingTitle = selectedGame.Title.ToUpper();
            ToggleMuteCommand = new RelayCommand(_ => IsMuted = !IsMuted);
            FullscreenCommand = new RelayCommand(_ => ExecuteFullscreen());
            ExitGameCommand = new RelayCommand(_ => ExecuteExitGame());
        }

        #region Logic Handlers
        private void OnVolumeChanged()
        {
            if (VolumeLevel > 0 && IsMuted)
            {
                IsMuted = false;
            }
            SetVolumeAction?.Invoke(VolumeLevel / 100.0);
        }

        private void OnMuteStateChanged()
        {
            SetMuteAction?.Invoke(IsMuted);
        }

        private void ExecuteFullscreen()
        {
            ToggleFullscreenAction?.Invoke();
        }

        private void ExecuteExitGame()
        {
            RequestCloseGame?.Invoke();

            GameDataService.Instance.Log($"Đã thoát trò chơi: {PlayingTitle}");

            if (System.Windows.Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.NavigateToList();
            }
        }
        #endregion
    }
}
