using System;
using System.Collections.Generic;

namespace StudentManagement.Models;

public partial class NotificationRecipient
{
    public int NotificationId { get; set; }

    public int RecipientId { get; set; }

    public bool IsRead { get; set; }

    public virtual Notification Notification { get; set; } = null!;

    public virtual User Recipient { get; set; } = null!;
}
