/* LUCAS CHANGE DETAIL */
namespace Core.Dtos
{
    // ===== GIỮ NGUYÊN LỚP CŨ =====
    public class ChangeRequestDto
    {
        public string Code { get; set; }
        public string Version { get; set; }
        public string ChangeTitle { get; set; }
        public string Department { get; set; }
        public string Status { get; set; }
        public string Category { get; set; }
        public string DocumentCode { get; set; }
        public string DepGroup { get; set; }   // map dep_group
        public string RequestID { get; set; }  // map RequestID
        public string TransDateTime { get; set; }

    }

    /* LUCAS CHANGE DETAIL: DTO MỚI CHO FILE ĐÍNH KÈM — CHỈ THÊM, KHÔNG SỬA LỚP CŨ */
    public class ChangeRequestFileDto
    {
        public string ChangeRequestID { get; set; }   // map ChangeRequestID trong bảng ChangeRequest_File
        public string LinkFile { get; set; }          // đường dẫn UNC đầy đủ (cột link_file)
        public string FileName
        {
            get
            {
                // Trả về chỉ tên file để hiển thị
                if (string.IsNullOrEmpty(LinkFile)) return "";
                try
                {
                    return System.IO.Path.GetFileName(LinkFile);
                }
                catch { return LinkFile; }
            }
        }
    }
}
