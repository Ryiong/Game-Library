using Game_Library.Models;
using Game_Library.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Game_Library.ViewModels
{
    internal class FavoritesViewModel : GameListViewModel
    {
        protected override IEnumerable<GameModels> GetSourceGames()
        {
            return GameDataService.Instance.AllGames.Where(g => g.isFavorite);
        }
    }
}
