using System.Collections.Generic;
using Core.Dtos;
using Data.Repositories;

namespace Services
{
    public class PageService
    {
        private readonly PageRepository _repo = new PageRepository();

        public IEnumerable<ChangeRequestDto> GetChangeRequestsByMenu(string p, string c, string user)
            => _repo.GetChangeRequestsByMenu(p, c, user);

        public IEnumerable<ChangeRequestDto> QueryChangeRequests(
            string p, string c, string user,
            string category, string doc, string title, string status)
            => _repo.QueryChangeRequests(p, c, user, category, doc, title, status);
    }
}
