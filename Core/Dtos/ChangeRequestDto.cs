namespace Core.Dtos
{
    public class ChangeRequestDto
    {
        public string Code { get; set; }
        public int Version { get; set; }
        public string ChangeTitle { get; set; }
        public string Department { get; set; }
        public string Status { get; set; }
        public string Category { get; set; }
        public string DocumentCode { get; set; }
    }
}
