using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Core.Dtos;
using Data.Repositories;
namespace Services
{
    public class PageService
    {
        private readonly PageRepository _repo = new PageRepository();
        // ================== HÀM CŨ (giữ nguyên cho các màn khác) ==================
        // Pending theo menu (parent/child)
        public IEnumerable<ChangeRequestDto> GetChangeRequestsByMenu(
            string p, string c, string user, int? parentId, int? childId)
        {
            return _repo.GetChangeRequestsByMenu(p, c, user, parentId, childId);
        }
        // Query theo menu (parent/child) + các bộ lọc
        public IEnumerable<ChangeRequestDto> QueryChangeRequests(
            string p, string c, string user,
            string category, string doc, string title, string status,
            int? parentId, int? childId)
        {
            return _repo.QueryChangeRequests(p, c, user, category, doc, title, status, parentId, childId);
        }
        // Lookup (category/status/doc/title) theo menu
        public IEnumerable<SelectListItem> GetOptionList(
            string type,string plant ,string depCode)
        {
            var rows = _repo.GetLookups(type,plant,depCode);
            // Map sang SelectListItem
            return rows.Select(x => new SelectListItem
            {
                Value = string.IsNullOrWhiteSpace(x.Value) ? x.Text : x.Value,
                Text = string.IsNullOrWhiteSpace(x.Text) ? x.Value : x.Text
            }).ToList();
        }
        // ================== HÀM MỚI (theo PLANT + DEP_CODE) ==================
        /// <summary>
        /// Danh sách mặc định cho Query (giống Pending) theo Plant + DepCode.
        /// </summary>
        public IEnumerable<ChangeRequestDto> GetChangeRequestsByDep(string plant, string depCode, string user)
        {
            // Repository cần có SP/Query tương ứng (ví dụ: PCN_Query_ByDep)
            return _repo.GetChangeRequestsByDep(plant, depCode, user);
        }
        /// <summary>
        /// Lọc lại theo Plant + DepCode + các bộ lọc (chỉ Category là dropdown, phần còn lại text).
        /// </summary>
        public IEnumerable<ChangeRequestDto> QueryChangeRequestsByDep(
            string plant,
            string depCode,
            string user,
            string category,
            string doc,
            string title,
            string status)
        {
            // Repository cần có SP/Query tương ứng (ví dụ: PCN_Query_ByDep_Filter)
            return _repo.QueryChangeRequestsByDep(plant, depCode, user, category, doc, title, status);
        }
        /// <summary>
        /// Dropdown Category theo Plant + DepCode (riêng màn Query mới).
        /// </summary>
        public IEnumerable<SelectListItem> GetCategoryListByDep(string plant, string depCode)
        {
            var rows = _repo.GetCategoryLookupByDep(plant, depCode); // trả về (Text, Value)
            return rows.Select(x => new SelectListItem
            {
                Value = string.IsNullOrWhiteSpace(x.Value) ? x.Text : x.Value,
                Text = string.IsNullOrWhiteSpace(x.Text) ? x.Value : x.Text
            }).ToList();
        }
    }
}