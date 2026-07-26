using Game_Library.Models;
using Game_Library.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Game_Library.ViewModels
{
    internal class HTML5IndieViewModel : GameListViewModel
    {
        protected override IEnumerable<GameModels> GetSourceGames()
        {
            return GameDataService.Instance.AllGames.Where(g => string.Equals(g.Type, "HTML5", StringComparison.OrdinalIgnoreCase));
        }
    }
}
