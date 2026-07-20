using Game_Library.Models;
using Game_Library.Services;
using Game_Library.Views;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace Game_Library.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private object _currentView;
        private int _sidebarSelectedIndex;
        private bool _isDefaultHeaderVisible = true;
        private bool _isDetailHeaderVisible = false;
        private string _statusText = "Server Status: Sleep";
        private string _logText = string.Empty;
        private bool _isNsfwChecked;

        private readonly Stack<object> _viewHistory = new Stack<object>();
        private readonly Stack<int> _sidebarHistory = new Stack<int>();

        public object CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        public int SidebarSelectedIndex
        {
            get => _sidebarSelectedIndex;
            set
            {
                if (SetProperty(ref _sidebarSelectedIndex, value) && value != -1)
                {
                    NavigateToTab(value);
                }
            }
        }

        public bool IsDefaultHeaderVisible
        {
            get => _isDefaultHeaderVisible;
            set => SetProperty(ref _isDefaultHeaderVisible, value);
        }

        public bool IsDetailHeaderVisible
        {
            get => _isDetailHeaderVisible;
            set => SetProperty(ref _isDetailHeaderVisible, value);
        }

        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        public string LogText
        {
            get => _logText;
            set => SetProperty(ref _logText, value);
        }

        public bool IsNsfwChecked
        {
            get => _isNsfwChecked;
            set
            {
                if (SetProperty(ref _isNsfwChecked, value))
                {
                    HandleNsfwToggle(value);
                }
            }
        }

        public ICommand ReturnCommand { get; }
        public ICommand AddGameCommand { get; }

        public MainViewModel()
        {
            GameDataService.Instance.OnStatusChanged += (msg) => StatusText = msg;
            GameDataService.Instance.OnLogChanged += (log) => LogText = log;
            GameDataService.Instance.InitializeData();
            CurrentView = new AllGamesView();

            SidebarSelectedIndex = 0;
            StatusText = "Server Status: Off";
            LogText = "Welcome.";

            ReturnCommand = new RelayCommand(_ => ExecuteReturn());
            AddGameCommand = new RelayCommand(_ => ExecuteAddGame());
        }

        private void NavigateToTab(int tabIndex)
        {
            _viewHistory.Clear();
            _sidebarHistory.Clear();

            switch (tabIndex)
            {
                case 0: CurrentView = new AllGamesView(); break;
                case 1: CurrentView = new FlashClassicsGames(); break;
                case 2: CurrentView = new HTML5IndieView(); break;
                case 3: CurrentView = new FavoritesGameView(); break;
            }
            ToggleHeader(isDetailMode: false);
        }

        public void NavigateToDetail(GameModels selectedGame)
        {
            if (CurrentView != null)
            {
                _viewHistory.Push(CurrentView);
                _sidebarHistory.Push(SidebarSelectedIndex);
            }

            CurrentView = new DetailGameView(selectedGame);
            _sidebarSelectedIndex = -1;
            OnPropertyChanged(nameof(SidebarSelectedIndex));
            ToggleHeader(isDetailMode: true);
        }

        public void NavigateToPlay(GameModels selectedGame)
        {
            if (CurrentView != null)
            {
                _viewHistory.Push(CurrentView);
                _sidebarHistory.Push(SidebarSelectedIndex);
            }

            CurrentView = new GameView(selectedGame);
            _sidebarSelectedIndex = -1;
            OnPropertyChanged(nameof(SidebarSelectedIndex));
            ToggleHeader(isDetailMode: true);
            GameDataService.Instance.Log($"Đang khởi chạy trò chơi: {selectedGame.Title}");
        }

        private void ExecuteReturn()
        {
            while (_viewHistory.Count > 0 && _viewHistory.Peek().GetType().Name.Equals("GameView", StringComparison.OrdinalIgnoreCase))
            {
                _viewHistory.Pop();
                _sidebarHistory.Pop();
            }

            if (_viewHistory.Count > 0)
            {
                CurrentView = _viewHistory.Pop();
                _sidebarSelectedIndex = _sidebarHistory.Pop();
                OnPropertyChanged(nameof(SidebarSelectedIndex));

                if (CurrentView.GetType().Name != "DetailGameView")
                {
                    ToggleHeader(isDetailMode: false);
                }
            }
            else
            {
                SidebarSelectedIndex = 0;
            }
        }

        private void ExecuteAddGame()
        {
            AddGameWindow addDialog = new AddGameWindow();
            addDialog.Owner = System.Windows.Application.Current.MainWindow;
            if (addDialog.ShowDialog() == true)
            {
                int currentTab = SidebarSelectedIndex == -1 ? 0 : SidebarSelectedIndex;
                NavigateToTab(currentTab);
            }
        }

        private void HandleNsfwToggle(bool isChecked)
        {
            if (isChecked)
            {
                PasswordDialog authDialog = new PasswordDialog();
                authDialog.Owner = System.Windows.Application.Current.MainWindow;
                GameDataService.Instance.Log("Mở khóa nội dung Giới hạn (NSFW).");

                if (authDialog.ShowDialog() == true)
                {
                    MainWindow.SetNsfwStatus(true);
                    RefreshCurrentTab();
                }
                else
                {
                    _isNsfwChecked = false;
                    OnPropertyChanged(nameof(IsNsfwChecked)); // Trả ngược trạng thái nút UI
                    MainWindow.SetNsfwStatus(false);
                }
            }
            else
            {
                MainWindow.SetNsfwStatus(true);
                GameDataService.Instance.Log("Hi");
                RefreshCurrentTab();
            }
        }

        private void RefreshCurrentTab()
        {
            int currentTab = SidebarSelectedIndex == -1 ? 0 : SidebarSelectedIndex;
            NavigateToTab(currentTab);
        }

        private void ToggleHeader(bool isDetailMode)
        {
            IsDefaultHeaderVisible = !isDetailMode;
            IsDetailHeaderVisible = isDetailMode;
        }

        public void Log(string message)
        {
            LogText = $"[{DateTime.Now:HH:mm:ss}] {message}";
        }
    }
}
