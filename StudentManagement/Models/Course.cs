using System;
using System.Collections.Generic;

namespace StudentManagement.Models;

public partial class Course
{
    public int CourseId { get; set; }

    public string CourseCode { get; set; } = null!;

    public string CourseName { get; set; } = null!;

    public string? Description { get; set; }

    public string? Duration { get; set; }

    public decimal TuitionFee { get; set; }

    public int? Credits { get; set; }
    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();
}
