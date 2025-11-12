using System;
using System.Collections.Generic;

namespace StudentManagement.Models;

public partial class Report
{
    public int ReportId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }
}
