using System;
using System.Collections.Generic;

namespace StudentManagement.Models;

public partial class ScoreType
{
    public int ScoreTypeId { get; set; }

    public string ScoreTypeName { get; set; } = null!;

    public decimal? Weight { get; set; }

    public virtual ICollection<Score> Scores { get; set; } = new List<Score>();
}
