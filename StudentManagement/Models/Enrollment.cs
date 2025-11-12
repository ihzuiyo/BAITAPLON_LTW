using System;
using System.Collections.Generic;

namespace StudentManagement.Models;

public partial class Enrollment
{
    public int EnrollmentId { get; set; }

    public int StudentId { get; set; }

    public int ClassId { get; set; }

    public DateTime EnrollmentDate { get; set; }

    public string Status { get; set; } = null!;

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public virtual Class Class { get; set; } = null!;

    public virtual ICollection<Score> Scores { get; set; } = new List<Score>();

    public virtual Student Student { get; set; } = null!;

    public virtual ICollection<Tuition> Tuitions { get; set; } = new List<Tuition>();
}
