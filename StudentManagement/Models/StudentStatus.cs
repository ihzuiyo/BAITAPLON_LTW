using System;
using System.Collections.Generic;

namespace StudentManagement.Models;

public partial class StudentStatus
{
    public int StatusId { get; set; }

    public string StatusName { get; set; } = null!;

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}
