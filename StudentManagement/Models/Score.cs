using System;
using System.Collections.Generic;

namespace StudentManagement.Models;

public partial class Score
{
    public int ScoreId { get; set; }

    public int EnrollmentId { get; set; }

    public int ScoreTypeId { get; set; }

    public decimal ScoreValue { get; set; }

    public virtual Enrollment Enrollment { get; set; } = null!;

    public virtual ScoreType ScoreType { get; set; } = null!;
}
