namespace StudentManagement.Models.ViewModels
{
    public class ScoreUpdateModel
    {
        public int ScoreId { get; set; }
        public int ScoreTypeId { get; set; }
        public string ScoreTypeName { get; set; } // <--- ĐÃ BỔ SUNG
        public decimal ScoreValue { get; set; }
    }
}
