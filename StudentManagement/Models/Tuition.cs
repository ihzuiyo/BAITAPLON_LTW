using System;
using System.Collections.Generic;

namespace StudentManagement.Models;

public partial class Tuition
{
    public int TuitionId { get; set; }

    public int EnrollmentId { get; set; }

    public decimal TotalFee { get; set; }

    public decimal AmountPaid { get; set; }

    public DateOnly? DueDate { get; set; }

    public string Status { get; set; } = null!;

    public virtual Enrollment Enrollment { get; set; } = null!;

    public virtual ICollection<Receipt> Receipts { get; set; } = new List<Receipt>();
}
