using System;
using System.Collections.Generic;

namespace StudentManagement.Models;

public partial class Class
{
    public int ClassId { get; set; }

    public int CourseId { get; set; }

    public int RoomId { get; set; }

    public string ClassCode { get; set; } = null!;

    public string ClassName { get; set; } = null!;

    public int? MaxStudents { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public virtual ICollection<ClassSchedule> ClassSchedules { get; set; } = new List<ClassSchedule>();

    public virtual Course Course { get; set; } = null!;

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public virtual Room Room { get; set; } = null!;
}
