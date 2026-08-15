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
        private FlashClassicsGames _flashView;
        private HTML5IndieView _html5View;

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
            CurrentView = _flashView ?? new FlashClassicsGames();

            SidebarSelectedIndex = 0;
            StatusText = "Server Status: Off";
            LogText = "Welcome.";

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
                    _flashView ??= new FlashClassicsGames();
                    CurrentView = _flashView;
                    break;
                case 1:
                    _html5View ??= new HTML5IndieView();
                    CurrentView = _html5View;
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
