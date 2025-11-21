namespace StudentManagement.Models.ViewModels
{
    public class UpdateScoreViewModel
    {
        public int EnrollmentId { get; set; }
        public int ClassId { get; set; }
        public string StudentName { get; set; } // <--- ĐÃ BỔ SUNG
        public List<ScoreUpdateModel> Scores { get; set; }
    }
}
