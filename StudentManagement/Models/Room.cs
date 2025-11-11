using System;
using System.Collections.Generic;

namespace StudentManagement.Models;

public partial class Room
{
    public int RoomId { get; set; }

    public string RoomName { get; set; } = null!;

    public int Capacity { get; set; }

    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();
}
