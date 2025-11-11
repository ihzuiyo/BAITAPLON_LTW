using System;
using System.Collections.Generic;

namespace StudentManagement.Models;

public partial class Notification
{
    public int NotificationId { get; set; }

    public int CreatorId { get; set; }

    public string Title { get; set; } = null!;

    public string? Content { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual User Creator { get; set; } = null!;

    public virtual ICollection<NotificationRecipient> NotificationRecipients { get; set; } = new List<NotificationRecipient>();
}
