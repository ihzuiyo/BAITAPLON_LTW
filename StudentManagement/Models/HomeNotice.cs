using System;
using System.Collections.Generic;

namespace StudentManagement.Models;

public partial class HomeNotice
{
    public int NoticeId { get; set; }

    public string Title { get; set; } = null!;

    public string? Content { get; set; }

    public DateTime CreatedAt { get; set; }
}
