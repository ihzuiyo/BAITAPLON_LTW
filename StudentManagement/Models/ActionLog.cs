using System;
using System.Collections.Generic;

namespace StudentManagement.Models;

public partial class ActionLog
{
    public int LogId { get; set; }

    public int UserId { get; set; }

    public string Action { get; set; } = null!;

    public string? Details { get; set; }

    public DateTime LogDate { get; set; }

    public virtual User User { get; set; } = null!;
}
