using System;
using System.Collections.Generic;

namespace StudentManagement.Models;

public partial class Teacher
{
    public int TeacherId { get; set; }

    public int UserId { get; set; }

    public string TeacherCode { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string? Specialization { get; set; }

    public string? PhoneNumber { get; set; }

    public string Email { get; set; } = null!;

    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();

    public virtual ICollection<FeaturedTeacher> FeaturedTeachers { get; set; } = new List<FeaturedTeacher>();

    public virtual User User { get; set; } = null!;
}
