using System;
using System.Collections.Generic;

namespace StudentManagement.Models;

public partial class Student
{
    public int StudentId { get; set; }

    public int UserId { get; set; }

    public int? StatusId { get; set; }

    public string StudentCode { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public DateOnly? DateOfBirth { get; set; }

    public string? Gender { get; set; }

    public string? Address { get; set; }

    public string? PhoneNumber { get; set; }

    public string Email { get; set; } = null!;

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public virtual StudentStatus? Status { get; set; }

    public virtual User User { get; set; } = null!;
}
