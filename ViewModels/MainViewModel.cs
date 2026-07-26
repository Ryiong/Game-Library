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
        private string _searchKeyword = string.Empty;
        private bool _isNsfwChecked;
        private AllGamesView _allGamesView;
        private FutureGamesView _futureGamesView;
        private FlashClassicsGames _flashView;
        private HTML5IndieView _html5View;
        private FavoritesGameView _favoritesView;

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

        public string SearchKeyword
        {
            get => _searchKeyword;
            set
            {
                if (SetProperty(ref _searchKeyword, value))
                {
                    ApplySearchToCurrentView(value);
                }
            }
        }

        private void ApplySearchToCurrentView(string keyword)
        {
            if (CurrentView is System.Windows.Controls.UserControl uc)
            {
                if (uc.DataContext is GameListViewModel listVM)
                {
                    listVM.SearchKeyword = keyword;
                }
                else if (uc.DataContext is FutureGamesViewModel futureVM)
                {
                    futureVM.SearchKeyword = keyword;
                }
            }
        }

        public ICommand ReturnCommand { get; }
        public ICommand AddGameCommand { get; }
        public ICommand EditGameCommand { get; }

        public MainViewModel()
        {
            GameDataService.Instance.OnStatusChanged += (msg) =>
                System.Windows.Application.Current.Dispatcher.Invoke(() => StatusText = msg);

            GameDataService.Instance.OnLogChanged += (log) =>
                System.Windows.Application.Current.Dispatcher.Invoke(() => LogText = log);
            GameDataService.Instance.InitializeData();
            CurrentView = _allGamesView ?? new AllGamesView();

            SidebarSelectedIndex = 0;
            StatusText = "Server Status: Off";
            LogText = "Welcome.";

            ReturnCommand = new RelayCommand(_ => ExecuteReturn());
            AddGameCommand = new RelayCommand(_ => ExecuteAddGame());
            EditGameCommand = new RelayCommand(param => ExecuteEditGame(param as GameModels));
        }

        private void NavigateToTab(int tabIndex)
        {
            if (CurrentView is UserControl oldView && oldView.DataContext is IDisposable disposableVM)
            {
                disposableVM.Dispose();
            }
            _viewHistory.Clear();
            _sidebarHistory.Clear();

            switch (tabIndex)
            {
                case 0:
                    _allGamesView ??= new AllGamesView();
                    CurrentView = _allGamesView;
                    break;
                case 1:
                    _flashView ??= new FlashClassicsGames();
                    CurrentView = _flashView;
                    break;
                case 2:
                    _html5View ??= new HTML5IndieView();
                    CurrentView = _html5View;
                    break;
                case 3:
                    _favoritesView ??= new FavoritesGameView();
                    CurrentView = _favoritesView;
                    break;
                case 4:
                    _futureGamesView ??= new FutureGamesView();
                    CurrentView = _futureGamesView;
                    break;
            }
            ApplySearchToCurrentView(SearchKeyword);
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
        }

        private void ExecuteReturn()
        {
            while (_viewHistory.Count > 0 && _viewHistory.Peek().GetType().Name.Equals("GameView", StringComparison.OrdinalIgnoreCase))
            {
                var viewToDispose = _viewHistory.Pop();
                if (viewToDispose is System.Windows.Controls.UserControl uc && uc.DataContext is IDisposable disposableVM)
                {
                    disposableVM.Dispose();
                }

                _sidebarHistory.Pop();
            }

            if (_viewHistory.Count > 0)
            {
                if (CurrentView is System.Windows.Controls.UserControl currentUc && currentUc.DataContext is IDisposable currentDisposable)
                {
                    currentDisposable.Dispose();
                }

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
                MainWindow.SetNsfwStatus(false);
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

        private void ExecuteEditGame(GameModels gameToEdit)
        {
            if (gameToEdit == null) return;

            AddGameWindow editDialog = new AddGameWindow(gameToEdit);
            editDialog.Owner = System.Windows.Application.Current.MainWindow;

            if (editDialog.ShowDialog() == true)
            {
                GameDataService.Instance.LoadGames();

                var updatedGame = GameDataService.Instance.AllGames.FirstOrDefault(g => g.Id == gameToEdit.Id);

                if (updatedGame != null)
                {    
                    if (CurrentView is DetailGameView)
                    {
                        CurrentView = new DetailGameView(updatedGame);
                    }
                    else
                    {
                        RefreshCurrentTab();
                    }
                }
                else
                {
                    RefreshCurrentTab();
                }

                GameDataService.Instance.Log($"[UI RELOAD] Đã làm mới giao diện cho trò chơi: {gameToEdit.Title}");
            }
        }
    }
}
