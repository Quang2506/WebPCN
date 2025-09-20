namespace Core.Dtos
{
    // DTO khớp các field bạn đang dùng ở View/Controller
    public class ChangeRequests
    {
        public int Id { get; set; }

        public string DocumentCode { get; set; }
        public string version { get; set; }
        public string ChangeTitle { get; set; }
        public string Model { get; set; }

        public string group_dept { get; set; }
        public string pms_i_usr { get; set; }
        public string pms_i_ymd { get; set; }

        public string Status { get; set; }
        public string Request_detail { get; set; }
    }
}
