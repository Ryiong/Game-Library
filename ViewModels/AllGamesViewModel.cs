using Game_Library.Models;
using Game_Library.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Game_Library.ViewModels
{
    public class AllGamesViewModel : GameListViewModel
    {
        protected override IEnumerable<GameModels> GetSourceGames()
        {
            return GameDataService.Instance.AllGames;
        }
    }
}
