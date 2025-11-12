using System;
using System.Collections.Generic;

namespace StudentManagement.Models;

public partial class User
{
    public int UserId { get; set; }

    public int? RoleId { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? FullName { get; set; }

    public string Email { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public string Status { get; set; } = null!;

    public DateTime DateCreated { get; set; }

    public virtual ICollection<ActionLog> ActionLogs { get; set; } = new List<ActionLog>();

    public virtual ICollection<NotificationRecipient> NotificationRecipients { get; set; } = new List<NotificationRecipient>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Receipt> Receipts { get; set; } = new List<Receipt>();

    public virtual Role? Role { get; set; }

    public virtual Student? Student { get; set; }

    public virtual Teacher? Teacher { get; set; }
}
