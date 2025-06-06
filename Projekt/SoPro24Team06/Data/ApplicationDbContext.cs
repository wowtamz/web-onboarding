//-------------------------
// Author: Tamas Varadi
//-------------------------

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SoPro24Team06.Models;

namespace SoPro24Team06.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Assignment> Assignments { get; set; }
    public DbSet<AssignmentTemplate> AssignmentTemplates { get; set; }
    public DbSet<Process> Processes { get; set; }
    public DbSet<ProcessTemplate> ProcessTemplates { get; set; }
    public DbSet<Contract> Contracts { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<DueTime> DueTimes { get; set; }

    /// <summary>
    /// Configures entity relations for ApplicationDbContext
    /// </summary>
    /// <param name="modelBuilder"></param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //
        // Beziehungen konfigurieren Process
        //
        modelBuilder
            .Entity<Process>()
            .HasOne(p => p.WorkerOfReference)
            .WithMany()
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder
            .Entity<Process>()
            .HasOne(p => p.Supervisor)
            .WithMany()
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder
            .Entity<Process>()
            .HasOne(p => p.ContractOfRefWorker)
            .WithMany()
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder
            .Entity<Process>()
            .HasOne(p => p.DepartmentOfRefWorker)
            .WithMany()
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder
            .Entity<Process>()
            .HasMany(p => p.Assignments)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);
        //
        // Beziehungen konfigurieren ProcessTemplate
        //
        modelBuilder
            .Entity<ProcessTemplate>()
            .HasOne(p => p.ContractOfRefWorker)
            .WithMany()
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder
            .Entity<ProcessTemplate>()
            .HasOne(p => p.DepartmentOfRefWorker)
            .WithMany()
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder
            .Entity<ProcessTemplate>()
            .HasMany(p => p.RolesWithAccess)
            .WithMany(r => r.ProcessTemplates)
            .UsingEntity("ProcessTemplateRolesWithAccess");

        modelBuilder
            .Entity<ProcessTemplate>()
            .HasMany(p => p.AssignmentTemplates)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);

        //
        // Beziehungen konfigurieren Assignment
        //
        modelBuilder
            .Entity<Assignment>()
            .HasOne(p => p.AssignedRole)
            .WithMany()
            .OnDelete(DeleteBehavior.Restrict);

		modelBuilder
			.Entity<Assignment>()
			.HasOne(p => p.Assignee)
			.WithMany()
			.OnDelete(DeleteBehavior.Restrict);

        modelBuilder
            .Entity<Assignment>()
            .HasMany(p => p.ForDepartmentsList)
            .WithMany(d => d.Assignments)
            .UsingEntity("AssignmentForDepartments");

        modelBuilder
            .Entity<Assignment>()
            .HasMany(p => p.ForContractsList)
            .WithMany(c => c.Assignments)
            .UsingEntity("AssignmentForContracts");

        modelBuilder
            .Entity<Assignment>()
            .HasOne(p => p.Assignee)
            .WithMany()
            .OnDelete(DeleteBehavior.Restrict);
        
        //
        // Beziehungen konfigurieren AssignmentTemplate
        //
        modelBuilder
            .Entity<AssignmentTemplate>()
            .HasOne(p => p.DueIn)
            .WithMany()
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder
            .Entity<AssignmentTemplate>()
            .HasMany(p => p.ForDepartmentsList)
            .WithMany(d => d.AssignmentsTemplates)
            .UsingEntity("AssignmentTemplateForDepartments");

        modelBuilder
            .Entity<AssignmentTemplate>()
            .HasMany(p => p.ForContractsList)
            .WithMany(c => c.AssignmentsTemplates)
            .UsingEntity("AssignmentTemplateForContracts");

        modelBuilder
            .Entity<AssignmentTemplate>()
            .HasOne(p => p.AssignedRole)
            .WithMany()
            .OnDelete(DeleteBehavior.Restrict);
    }
}
