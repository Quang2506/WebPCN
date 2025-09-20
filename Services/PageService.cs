using System.Collections.Generic;
using Core.Dtos;
using Data.Repositories;
namespace Services
{
    public class PageService
    {
        private readonly PageRepository _repo = new PageRepository();
        public IEnumerable<ChangeRequestDto> GetChangeRequestsByMenu(
            string p, string c, string user, int? parentId, int? childId)
            => _repo.GetChangeRequestsByMenu(p, c, user, parentId, childId);
        public IEnumerable<ChangeRequestDto> QueryChangeRequests(
            string p, string c, string user,
            string category, string doc, string title, string status,
            int? parentId, int? childId)
            => _repo.QueryChangeRequests(p, c, user, category, doc, title, status, parentId, childId);
    }
}