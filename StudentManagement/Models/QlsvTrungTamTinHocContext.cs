using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace StudentManagement.Models;

public partial class QlsvTrungTamTinHocContext : DbContext
{
    public QlsvTrungTamTinHocContext()
    {
    }

    public QlsvTrungTamTinHocContext(DbContextOptions<QlsvTrungTamTinHocContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ActionLog> ActionLogs { get; set; }

    public virtual DbSet<Attendance> Attendances { get; set; }

    public virtual DbSet<Class> Classes { get; set; }

    public virtual DbSet<ClassSchedule> ClassSchedules { get; set; }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<Enrollment> Enrollments { get; set; }

    public virtual DbSet<FeaturedTeacher> FeaturedTeachers { get; set; }

    public virtual DbSet<HomeNotice> HomeNotices { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<NotificationRecipient> NotificationRecipients { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<Receipt> Receipts { get; set; }

    public virtual DbSet<Report> Reports { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Room> Rooms { get; set; }

    public virtual DbSet<Score> Scores { get; set; }

    public virtual DbSet<ScoreType> ScoreTypes { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<StudentStatus> StudentStatuses { get; set; }

    public virtual DbSet<Teacher> Teachers { get; set; }

    public virtual DbSet<Tuition> Tuitions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    //public virtual DbSet<RolePermission> RolePermissions { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
// #warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=DESKTOP-LJTIROB\\SQLEXPRESS;Initial Catalog=QLSV_TrungTamTinHoc;Integrated Security=True;Encrypt=True;Trust Server Certificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ActionLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__ActionLo__5E548648D47392A4");

            entity.Property(e => e.Action).HasMaxLength(100);
            entity.Property(e => e.LogDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.User).WithMany(p => p.ActionLogs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_ActionLogs_Users");
        });

        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.HasKey(e => e.AttendanceId).HasName("PK__Attendan__8B69261C6989CDC9");

            entity.Property(e => e.Status).HasMaxLength(20);

            entity.HasOne(d => d.Enrollment).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.EnrollmentId)
                .HasConstraintName("FK_Attendances_Enrollments");
        });

        modelBuilder.Entity<Class>(entity =>
        {
            entity.HasKey(e => e.ClassId).HasName("PK__Classes__CB1927C0E489D02C");

            entity.HasIndex(e => e.ClassCode, "UQ__Classes__2ECD4A55C09B5413").IsUnique();

            entity.Property(e => e.ClassCode).HasMaxLength(20);
            entity.Property(e => e.ClassName).HasMaxLength(100);

            entity.HasOne(d => d.Course).WithMany(p => p.Classes)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Classes_Courses");

            entity.HasOne(d => d.Teacher).WithMany(p => p.Classes)
                .HasForeignKey(d => d.TeacherId)
                .HasConstraintName("FK_Classes_Teachers");
        });

        modelBuilder.Entity<ClassSchedule>(entity =>
        {
            entity.HasKey(e => e.ScheduleId).HasName("PK__ClassSch__9C8A5B49627AD38C");

            entity.Property(e => e.Weekday).HasMaxLength(20);

            entity.HasOne(d => d.Class).WithMany(p => p.ClassSchedules)
                .HasForeignKey(d => d.ClassId)
                .HasConstraintName("FK_ClassSchedules_Classes");

            entity.HasOne(d => d.Room).WithMany(p => p.ClassSchedules)
                .HasForeignKey(d => d.RoomId)
                .HasConstraintName("FK_ClassSchedules_Rooms");
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.CourseId).HasName("PK__Courses__C92D71A7FF8D41B6");

            entity.HasIndex(e => e.CourseCode, "UQ__Courses__FC00E000819829E4").IsUnique();

            entity.Property(e => e.CourseCode).HasMaxLength(20);
            entity.Property(e => e.CourseName).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Duration).HasMaxLength(50);
            entity.Property(e => e.TuitionFee).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.HasKey(e => e.EnrollmentId).HasName("PK__Enrollme__7F68771B1EDE6437");

            entity.HasIndex(e => new { e.StudentId, e.ClassId }, "UQ_Student_Class").IsUnique();

            entity.Property(e => e.EnrollmentDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Enrolled");

            entity.HasOne(d => d.Class).WithMany(p => p.Enrollments)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Enrollments_Classes");

            entity.HasOne(d => d.Student).WithMany(p => p.Enrollments)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Enrollments_Students");
        });

        modelBuilder.Entity<FeaturedTeacher>(entity =>
        {
            entity.HasKey(e => e.FeaturedId).HasName("PK__Featured__F64A059610866CD6");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.ImagePath).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.Teacher).WithMany(p => p.FeaturedTeachers)
                .HasForeignKey(d => d.TeacherId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_FeaturedTeachers_Teachers");
        });

        modelBuilder.Entity<HomeNotice>(entity =>
        {
            entity.HasKey(e => e.NoticeId).HasName("PK__HomeNoti__CE83CBE550F0AD48");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Title).HasMaxLength(200);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__20CF2E12DCABADC0");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.Creator).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.CreatorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Notifications_Users");
        });

        modelBuilder.Entity<NotificationRecipient>(entity =>
        {
            entity.HasKey(e => new { e.NotificationId, e.RecipientId }).HasName("PK__Notifica__EFC54E36D2498277");

            entity.HasOne(d => d.Notification).WithMany(p => p.NotificationRecipients)
                .HasForeignKey(d => d.NotificationId)
                .HasConstraintName("FK_NotificationRecipients_Notifications");

            entity.HasOne(d => d.Recipient).WithMany(p => p.NotificationRecipients)
                .HasForeignKey(d => d.RecipientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_NotificationRecipients_Users");
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.PermissionId).HasName("PK__Permissi__EFA6FB2FB3DA38AB");

            entity.HasIndex(e => e.PermissionName, "UQ__Permissi__0FFDA357ACEE9BFA").IsUnique();

            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.PermissionName).HasMaxLength(100);
        });

        modelBuilder.Entity<Receipt>(entity =>
        {
            entity.HasKey(e => e.ReceiptId).HasName("PK__Receipts__CC08C4201379BA77");

            entity.HasIndex(e => e.ReceiptCode, "UQ__Receipts__1AB76D0083A13D3F").IsUnique();

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Note).HasMaxLength(500);
            entity.Property(e => e.PaymentDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.ReceiptCode).HasMaxLength(50);

            entity.HasOne(d => d.Cashier).WithMany(p => p.Receipts)
                .HasForeignKey(d => d.CashierId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Receipts_Users");

            entity.HasOne(d => d.Tuition).WithMany(p => p.Receipts)
                .HasForeignKey(d => d.TuitionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Receipts_Tuitions");
        });

        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasKey(e => e.ReportId).HasName("PK__Reports__D5BD4805001B681B");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Title).HasMaxLength(200);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE1A2A9BA8E0");

            entity.HasIndex(e => e.RoleName, "UQ__Roles__8A2B61607048E4CA").IsUnique();

            entity.Property(e => e.RoleName).HasMaxLength(50);

            entity.HasMany(d => d.Permissions).WithMany(p => p.Roles)
                .UsingEntity<Dictionary<string, object>>(
                    "RolePermission",
                    r => r.HasOne<Permission>().WithMany()
                        .HasForeignKey("PermissionId")
                        .HasConstraintName("FK_RolePermissions_Permissions"),
                    l => l.HasOne<Role>().WithMany()
                        .HasForeignKey("RoleId")
                        .HasConstraintName("FK_RolePermissions_Roles"),
                    j =>
                    {
                        j.HasKey("RoleId", "PermissionId").HasName("PK__RolePerm__6400A1A845E417D1");
                        j.ToTable("RolePermissions");
                    });
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(e => e.RoomId).HasName("PK__Rooms__32863939A494AEB0");

            entity.HasIndex(e => e.RoomName, "UQ__Rooms__6B500B55D8D1E7FC").IsUnique();

            entity.Property(e => e.RoomName).HasMaxLength(50);
        });

        modelBuilder.Entity<Score>(entity =>
        {
            entity.HasKey(e => e.ScoreId).HasName("PK__Scores__7DD229D1BD65CC83");

            entity.Property(e => e.ScoreValue).HasColumnType("decimal(5, 2)");

            entity.HasOne(d => d.Enrollment).WithMany(p => p.Scores)
                .HasForeignKey(d => d.EnrollmentId)
                .HasConstraintName("FK_Scores_Enrollments");

            entity.HasOne(d => d.ScoreType).WithMany(p => p.Scores)
                .HasForeignKey(d => d.ScoreTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Scores_ScoreTypes");
        });

        modelBuilder.Entity<ScoreType>(entity =>
        {
            entity.HasKey(e => e.ScoreTypeId).HasName("PK__ScoreTyp__D3D6CA871246F3CF");

            entity.Property(e => e.ScoreTypeName).HasMaxLength(50);
            entity.Property(e => e.Weight).HasColumnType("decimal(5, 2)");
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.StudentId).HasName("PK__Students__32C52B99E203A175");

            entity.HasIndex(e => e.UserId, "UQ__Students__1788CC4DEC765B4F").IsUnique();

            entity.HasIndex(e => e.StudentCode, "UQ__Students__1FC88604C6C6E88F").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Students__A9D105344D216723").IsUnique();

            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Gender).HasMaxLength(10);
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.StudentCode).HasMaxLength(20);

            entity.HasOne(d => d.Status).WithMany(p => p.Students)
                .HasForeignKey(d => d.StatusId)
                .HasConstraintName("FK_Students_StudentStatuses");

            entity.HasOne(d => d.User).WithOne(p => p.Student)
                .HasForeignKey<Student>(d => d.UserId)
                .HasConstraintName("FK_Students_Users");
        });

        modelBuilder.Entity<StudentStatus>(entity =>
        {
            entity.HasKey(e => e.StatusId).HasName("PK__StudentS__C8EE206337E76A17");

            entity.HasIndex(e => e.StatusName, "UQ__StudentS__05E7698AD91812B8").IsUnique();

            entity.Property(e => e.StatusName).HasMaxLength(50);
        });

        modelBuilder.Entity<Teacher>(entity =>
        {
            entity.HasKey(e => e.TeacherId).HasName("PK__Teachers__EDF259642362E5A8");

            entity.HasIndex(e => e.UserId, "UQ__Teachers__1788CC4D74FE40AD").IsUnique();

            entity.HasIndex(e => e.TeacherCode, "UQ__Teachers__9B8A194EAF189B94").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Teachers__A9D105348E46A85D").IsUnique();

            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Specialization).HasMaxLength(100);
            entity.Property(e => e.TeacherCode).HasMaxLength(20);

            entity.HasOne(d => d.User).WithOne(p => p.Teacher)
                .HasForeignKey<Teacher>(d => d.UserId)
                .HasConstraintName("FK_Teachers_Users");
        });

        modelBuilder.Entity<Tuition>(entity =>
        {
            entity.HasKey(e => e.TuitionId).HasName("PK__Tuitions__F3CBCB48A7161B54");

            entity.Property(e => e.AmountPaid).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Pending");
            entity.Property(e => e.TotalFee).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Enrollment).WithMany(p => p.Tuitions)
                .HasForeignKey(d => d.EnrollmentId)
                .HasConstraintName("FK_Tuitions_Enrollments");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4CE438FA51");

            entity.HasIndex(e => e.Username, "UQ__Users__536C85E4F566122D").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Users__A9D105349358ED5B").IsUnique();

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
                .HasConstraintName("FK_Users_Roles");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
