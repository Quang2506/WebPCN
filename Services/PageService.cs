using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Core.Dtos;
using Data.Repositories;
public class PageService
{
    private readonly PageRepository _repo = new PageRepository();
    // Pending
    public IEnumerable<ChangeRequestDto> GetChangeRequestsByMenu(
        string p, string c, string user, int? parentId, int? childId)
        => _repo.GetChangeRequestsByMenu(p, c, user, parentId, childId);
    // Query
    public IEnumerable<ChangeRequestDto> QueryChangeRequests(
        string p, string c, string user,
        string category, string doc, string title, string status,
        int? parentId, int? childId)
        => _repo.QueryChangeRequests(p, c, user, category, doc, title, status, parentId, childId);
    // ===== LẤY DỮ LIỆU DROPDOWN (category/status/doc/title) =====
    public IEnumerable<SelectListItem> GetOptionList(
        string optionType,               // "category" | "status" | "doc" | "title"
        string parentName, string childName,
        string userName,                 // <<< cần truyền user
        int? parentId, int? childId)
    {
        var rows = _repo.GetLookups(optionType, parentName, childName, userName, parentId, childId);
        // Map sang SelectListItem
        return rows.Select(x => new SelectListItem
        {
            Value = string.IsNullOrWhiteSpace(x.Value) ? x.Text : x.Value,
            Text = string.IsNullOrWhiteSpace(x.Text) ? x.Value : x.Text
        }).ToList();
    }
}