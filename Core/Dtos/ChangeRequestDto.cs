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
        public string DepGroup { get; set; }   // map dep_group
        public string RequestID { get; set; }  // map RequestID

        public string TransDateTime { get; set; }
        public string StatusCode { get; set; }



    }
}
