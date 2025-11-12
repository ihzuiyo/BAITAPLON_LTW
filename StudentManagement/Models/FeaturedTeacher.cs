using System;
using System.Collections.Generic;

namespace StudentManagement.Models;

public partial class FeaturedTeacher
{
    public int FeaturedId { get; set; }

    public int? TeacherId { get; set; }

    public string Title { get; set; } = null!;

    public string? Summary { get; set; }

    public string? ImagePath { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Teacher? Teacher { get; set; }
}
