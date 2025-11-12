using System;
using System.Collections.Generic;

namespace StudentManagement.Models;

public partial class ClassSchedule
{
    public int ScheduleId { get; set; }

    public int ClassId { get; set; }

    public int? RoomId { get; set; }

    public string Weekday { get; set; } = null!;

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public virtual Class Class { get; set; } = null!;

    public virtual Room? Room { get; set; }
}
