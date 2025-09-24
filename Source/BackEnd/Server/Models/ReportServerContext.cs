using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace reportSystem01.Server.Models;

public partial class ReportServerContext : DbContext
{
    public ReportServerContext()
    {
    }

    public ReportServerContext(DbContextOptions<ReportServerContext> options)
        : base(options)
    {
    }

    //public virtual DbSet<ApiEndpoint> ApiEndpoints { get; set; }

    //public virtual DbSet<ApiLog> ApiLogs { get; set; }

    public virtual DbSet<Menu> Menus { get; set; }

    public virtual DbSet<Report> Reports { get; set; }

    public virtual DbSet<Report1> Reports1 { get; set; }

    public virtual DbSet<ReportConnection> ReportConnections { get; set; }

    public virtual DbSet<SystemOption> SystemOptions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserGroup> UserGroups { get; set; }

    public virtual DbSet<UserGroupMenuPermission> UserGroupMenuPermissions { get; set; }

    public virtual DbSet<UserGroupPermission> UserGroupPermissions { get; set; }

    public virtual DbSet<UserLog> UserLogs { get; set; }

    public virtual DbSet<UserMenuPermission> UserMenuPermissions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            optionsBuilder.UseSqlServer(connectionString);
        }
    }
    //    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
    //        => optionsBuilder.UseSqlServer("Server=DESKTOP-HSTJH9U;Initial Catalog=systemReport;User ID=loitp;Password=123;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //modelBuilder.Entity<ApiEndpoint>(entity =>
        //{
        //    entity.ToTable("ApiEndpoints");
        //    entity.HasKey(e => e.EndpointId).HasName("PK__ApiEndpo__A91563D1B1E72496");


        //    entity.Property(e => e.CreatedAt)
        //        .HasDefaultValueSql("(getdate())")
        //        .HasColumnType("datetime");
        //    entity.Property(e => e.EndpointUrl).HasMaxLength(255);
        //    entity.Property(e => e.IsSecure)
        //        .IsRequired()
        //        .HasDefaultValueSql("((1))");
        //    entity.Property(e => e.Method).HasMaxLength(10);
        //});

        //modelBuilder.Entity<ApiLog>(entity =>
        //{
        //    entity.HasKey(e => e.LogId).HasName("PK__ApiLogs__5E548648867C1D19");

        //    entity.Property(e => e.Datetime0)
        //        .HasDefaultValueSql("(getdate())")
        //        .HasColumnType("datetime")
        //        .HasColumnName("datetime0");
        //    entity.Property(e => e.Description).HasMaxLength(512);
        //    entity.Property(e => e.IpAddress).HasMaxLength(50);

        //    entity.HasOne(d => d.Endpoint).WithMany(p => p.ApiLogs)
        //        .HasForeignKey(d => d.EndpointId)
        //        .HasConstraintName("FK__ApiLogs__Endpoin__6383C8BA");

        //    entity.HasOne(d => d.User).WithMany(p => p.ApiLogs)
        //        .HasForeignKey(d => d.UserId)
        //        .HasConstraintName("FK__ApiLogs__UserId__6477ECF3");
        //});

        modelBuilder.Entity<Menu>(entity =>
        {
            entity.HasKey(e => e.MenuId).HasName("PK__Menus__C99ED230F8105CF7");

            entity.Property(e => e.MenuId)
                .HasMaxLength(8)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.MenuName).HasMaxLength(255);
            entity.Property(e => e.ParentMenuId)
                .HasMaxLength(8)
                .IsUnicode(false);
            entity.Property(e => e.SysId)
                .HasMaxLength(33)
                .IsUnicode(false)
                .HasColumnName("sysID");
            entity.Property(e => e.TypeMenu)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<Report>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("report");

            entity.Property(e => e.Content).HasMaxLength(512);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.Id)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.Title).HasMaxLength(512);
        });

        modelBuilder.Entity<Report1>(entity =>
        {
            entity.HasKey(e => e.ReportId).HasName("PK__Reports__D5BD4805E21C5BB1");

            entity.ToTable("Reports");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("((1))");
            entity.Property(e => e.ReportName).HasMaxLength(255);
            entity.Property(e => e.ReportType).HasMaxLength(50);
            entity.Property(e => e.ScheduleTime).HasColumnType("datetime");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Report1s)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__Reports__Created__3E52440B");
        });

        modelBuilder.Entity<ReportConnection>(entity =>
        {
            entity.HasKey(e => e.ConnectionId).HasName("PK__ReportCo__404A6493F4C59409");

            entity.Property(e => e.ConnectionName).HasMaxLength(100);
            entity.Property(e => e.ConnectionString).HasMaxLength(255);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DbType).HasMaxLength(50);
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("((1))");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<SystemOption>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SystemOp__3214EC076D50B77E");

            entity.Property(e => e.ConfigKey).HasMaxLength(100);
            entity.Property(e => e.ConfigValue).HasMaxLength(255);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4C3B2D872E");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FullName).HasMaxLength(255);
            entity.Property(e => e.LastLoginTime).HasColumnType("datetime");
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.PasswordSalt).HasMaxLength(255);
            entity.Property(e => e.Role).HasMaxLength(16);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.UserName).HasMaxLength(100);
        });

        modelBuilder.Entity<UserGroup>(entity =>
        {
            entity.HasKey(e => e.UserGroupId).HasName("PK__UserGrou__FA5A61C07BFB9F93");

            entity.Property(e => e.UserGroupId).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.GroupName).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<UserGroupMenuPermission>(entity =>
        {
            entity.HasNoKey();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.MenuId)
                .HasMaxLength(8)
                .IsUnicode(false);
            entity.Property(e => e.RAccess).HasColumnName("rAccess");
            entity.Property(e => e.RDel).HasColumnName("rDel");
            entity.Property(e => e.RInsert).HasColumnName("rInsert");
            entity.Property(e => e.RUpdate).HasColumnName("rUpdate");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Menu).WithMany()
                .HasForeignKey(d => d.MenuId)
                .HasConstraintName("FK__UserGroup__MenuI__19DFD96B");
        });

        modelBuilder.Entity<UserGroupPermission>(entity =>
        {
            entity.HasNoKey();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.UserGroup).WithMany()
                .HasForeignKey(d => d.UserGroupId)
                .HasConstraintName("FK__UserGroup__UserG__03F0984C");

            entity.HasOne(d => d.User).WithMany()
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__UserGroup__UserI__02FC7413");
        });

        modelBuilder.Entity<UserLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__UserLogs__5E548648C65CF378");

            entity.Property(e => e.Action).HasMaxLength(100);
            entity.Property(e => e.ActionTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.IpAddress).HasMaxLength(50);

            entity.HasOne(d => d.User).WithMany(p => p.UserLogs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__UserLogs__UserId__31EC6D26");
        });

        modelBuilder.Entity<UserMenuPermission>(entity =>
        {
            entity.HasNoKey();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.MenuId)
                .HasMaxLength(8)
                .IsUnicode(false);
            entity.Property(e => e.RAccess).HasColumnName("rAccess");
            entity.Property(e => e.RDel).HasColumnName("rDel");
            entity.Property(e => e.RInsert).HasColumnName("rInsert");
            entity.Property(e => e.RUpdate).HasColumnName("rUpdate");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Menu).WithMany()
                .HasForeignKey(d => d.MenuId)
                .HasConstraintName("FK__UserMenuP__MenuI__1AD3FDA4");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
