using System;
using System.Collections.Generic;

namespace StudentManagement.Models;

public partial class Attendance
{
    public int AttendanceId { get; set; }

    public int EnrollmentId { get; set; }

    public DateOnly SessionDate { get; set; }

    public string Status { get; set; } = null!;

    public virtual Enrollment Enrollment { get; set; } = null!;
}
