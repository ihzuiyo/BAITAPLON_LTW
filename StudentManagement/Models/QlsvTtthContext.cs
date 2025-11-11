using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace StudentManagement.Models;

public partial class QlsvTtthContext : DbContext
{
    public QlsvTtthContext()
    {
    }

    public QlsvTtthContext(DbContextOptions<QlsvTtthContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ActionLog> ActionLogs { get; set; }

    public virtual DbSet<Class> Classes { get; set; }

    public virtual DbSet<ClassSchedule> ClassSchedules { get; set; }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<Enrollment> Enrollments { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<NotificationRecipient> NotificationRecipients { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<Receipt> Receipts { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Room> Rooms { get; set; }

    public virtual DbSet<Score> Scores { get; set; }

    public virtual DbSet<ScoreType> ScoreTypes { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<Tuition> Tuitions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DangHuy\\SQLEXPRESS;Database=QLSV_TTTH;Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ActionLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__ActionLo__5E5486484AD5F9FF");

            entity.Property(e => e.Action).HasMaxLength(100);
            entity.Property(e => e.LogDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.User).WithMany(p => p.ActionLogs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__ActionLog__UserI__10566F31");
        });

        modelBuilder.Entity<Class>(entity =>
        {
            entity.HasKey(e => e.ClassId).HasName("PK__Classes__CB1927C044EC7197");

            entity.HasIndex(e => e.ClassCode, "UQ__Classes__2ECD4A552FF5DBBF").IsUnique();

            entity.Property(e => e.ClassCode).HasMaxLength(20);
            entity.Property(e => e.ClassName).HasMaxLength(100);

            entity.HasOne(d => d.Course).WithMany(p => p.Classes)
                .HasForeignKey(d => d.CourseId)
                .HasConstraintName("FK__Classes__CourseI__68487DD7");

            entity.HasOne(d => d.Room).WithMany(p => p.Classes)
                .HasForeignKey(d => d.RoomId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Classes__RoomId__693CA210");
        });

        modelBuilder.Entity<ClassSchedule>(entity =>
        {
            entity.HasKey(e => e.ScheduleId).HasName("PK__ClassSch__9C8A5B493DE7CBD3");

            entity.Property(e => e.Weekday).HasMaxLength(20);

            entity.HasOne(d => d.Class).WithMany(p => p.ClassSchedules)
                .HasForeignKey(d => d.ClassId)
                .HasConstraintName("FK__ClassSche__Class__6C190EBB");
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.CourseId).HasName("PK__Courses__C92D71A7787BEA05");

            entity.HasIndex(e => e.CourseCode, "UQ__Courses__FC00E000849C18C3").IsUnique();

            entity.Property(e => e.CourseCode).HasMaxLength(20);
            entity.Property(e => e.CourseName).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Duration).HasMaxLength(50);
            entity.Property(e => e.TuitionFee).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.HasKey(e => e.EnrollmentId).HasName("PK__Enrollme__7F68771BF6FDCA7D");

            entity.HasIndex(e => new { e.StudentId, e.ClassId }, "UQ_Student_Class").IsUnique();

            entity.Property(e => e.EnrollmentDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Enrolled");

            entity.HasOne(d => d.Class).WithMany(p => p.Enrollments)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Enrollmen__Class__72C60C4A");

            entity.HasOne(d => d.Student).WithMany(p => p.Enrollments)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Enrollmen__Stude__71D1E811");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__20CF2E12721E8B3B");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.Creator).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.CreatorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Notificat__Creat__07C12930");
        });

        modelBuilder.Entity<NotificationRecipient>(entity =>
        {
            entity.HasKey(e => new { e.NotificationId, e.RecipientId }).HasName("PK__Notifica__EFC54E364EF7ABCF");

            entity.HasOne(d => d.Notification).WithMany(p => p.NotificationRecipients)
                .HasForeignKey(d => d.NotificationId)
                .HasConstraintName("FK__Notificat__Notif__0B91BA14");

            entity.HasOne(d => d.Recipient).WithMany(p => p.NotificationRecipients)
                .HasForeignKey(d => d.RecipientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Notificat__Recip__0C85DE4D");
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.PermissionId).HasName("PK__Permissi__EFA6FB2F3CA0A57F");

            entity.HasIndex(e => e.PermissionName, "UQ__Permissi__0FFDA357B7B77DFC").IsUnique();

            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.PermissionName).HasMaxLength(100);
        });

        modelBuilder.Entity<Receipt>(entity =>
        {
            entity.HasKey(e => e.ReceiptId).HasName("PK__Receipts__CC08C42002F56860");

            entity.HasIndex(e => e.ReceiptCode, "UQ__Receipts__1AB76D008CAA4EF1").IsUnique();

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Note).HasMaxLength(500);
            entity.Property(e => e.PaymentDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.ReceiptCode).HasMaxLength(50);

            entity.HasOne(d => d.Cashier).WithMany(p => p.Receipts)
                .HasForeignKey(d => d.CashierId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Receipts__Cashie__03F0984C");

            entity.HasOne(d => d.Tuition).WithMany(p => p.Receipts)
                .HasForeignKey(d => d.TuitionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Receipts__Tuitio__02FC7413");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE1A8DA360A5");

            entity.HasIndex(e => e.RoleName, "UQ__Roles__8A2B6160781FEB6E").IsUnique();

            entity.Property(e => e.RoleName).HasMaxLength(50);

            entity.HasMany(d => d.Permissions).WithMany(p => p.Roles)
                .UsingEntity<Dictionary<string, object>>(
                    "RolePermission",
                    r => r.HasOne<Permission>().WithMany()
                        .HasForeignKey("PermissionId")
                        .HasConstraintName("FK__RolePermi__Permi__5070F446"),
                    l => l.HasOne<Role>().WithMany()
                        .HasForeignKey("RoleId")
                        .HasConstraintName("FK__RolePermi__RoleI__4F7CD00D"),
                    j =>
                    {
                        j.HasKey("RoleId", "PermissionId").HasName("PK__RolePerm__6400A1A885A8E581");
                        j.ToTable("RolePermissions");
                    });
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(e => e.RoomId).HasName("PK__Rooms__32863939BE2E0038");

            entity.HasIndex(e => e.RoomName, "UQ__Rooms__6B500B55A8FD3A92").IsUnique();

            entity.Property(e => e.RoomName).HasMaxLength(50);
        });

        modelBuilder.Entity<Score>(entity =>
        {
            entity.HasKey(e => e.ScoreId).HasName("PK__Scores__7DD229D14C400F01");

            entity.Property(e => e.ScoreValue).HasColumnType("decimal(5, 2)");

            entity.HasOne(d => d.Enrollment).WithMany(p => p.Scores)
                .HasForeignKey(d => d.EnrollmentId)
                .HasConstraintName("FK__Scores__Enrollme__787EE5A0");

            entity.HasOne(d => d.ScoreType).WithMany(p => p.Scores)
                .HasForeignKey(d => d.ScoreTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Scores__ScoreTyp__797309D9");
        });

        modelBuilder.Entity<ScoreType>(entity =>
        {
            entity.HasKey(e => e.ScoreTypeId).HasName("PK__ScoreTyp__D3D6CA87CD3F4CA1");

            entity.Property(e => e.ScoreTypeName).HasMaxLength(50);
            entity.Property(e => e.Weight).HasColumnType("decimal(5, 2)");
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.StudentId).HasName("PK__Students__32C52B9938B63D4F");

            entity.HasIndex(e => e.UserId, "UQ__Students__1788CC4D08186A1C").IsUnique();

            entity.HasIndex(e => e.StudentCode, "UQ__Students__1FC88604036AD8A2").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Students__A9D105345B7151CB").IsUnique();

            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Gender).HasMaxLength(10);
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Đang học");
            entity.Property(e => e.StudentCode).HasMaxLength(20);

            entity.HasOne(d => d.User).WithOne(p => p.Student)
                .HasForeignKey<Student>(d => d.UserId)
                .HasConstraintName("FK__Students__UserId__5DCAEF64");
        });

        modelBuilder.Entity<Tuition>(entity =>
        {
            entity.HasKey(e => e.TuitionId).HasName("PK__Tuitions__F3CBCB48A83D4251");

            entity.Property(e => e.AmountPaid).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Pending");
            entity.Property(e => e.TotalFee).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Enrollment).WithMany(p => p.Tuitions)
                .HasForeignKey(d => d.EnrollmentId)
                .HasConstraintName("FK__Tuitions__Enroll__7E37BEF6");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4CBBBA035F");

            entity.HasIndex(e => e.Username, "UQ__Users__536C85E4AEA94CDA").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Users__A9D10534C6062DBD").IsUnique();

            entity.Property(e => e.DateCreated).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasMaxLength(256);
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Active");
            entity.Property(e => e.Username).HasMaxLength(50);

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Users__RoleId__571DF1D5");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
