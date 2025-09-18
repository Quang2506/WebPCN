using Core.Dtos;
using Data.Repositories;
using System.Collections.Generic;

namespace Services
{
    public class MenuService
    {
        private readonly MenuRepository _repo = new MenuRepository();

        public IEnumerable<MenuItem> GetMenu(int userId)
        {
            return _repo.GetMenu(userId);
        }
    }
}
