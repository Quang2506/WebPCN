
namespace Core.Dtos
{
    public class IdPairDto
    {
        public int? ParentId { get; set; }
        public int? ChildId { get; set; }
    }

    public class BreadcrumbDto
    {
        public string Parent { get; set; }
        public string Child { get; set; }
        public string Screen { get; set; }
    }
}
