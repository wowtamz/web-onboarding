using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using SoPro24Team06.Containers;
using SoPro24Team06.Data;
using SoPro24Team06.Enums;
using SoPro24Team06.Models;
using Xunit;

public class ProcessTemplateContainerTests
{
    private readonly ApplicationDbContext _context;
    private readonly ProcessTemplateContainer _container;

    public ProcessTemplateContainerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

        _context = new ApplicationDbContext(options);
        _container = new ProcessTemplateContainer(_context);

        // Seed the database with initial data
        SeedDatabase();
    }

    private void SeedDatabase()
    {
        if (!_context.Contracts.Any())
        {
            _context.Contracts.Add(new Contract { Id = 1, Label = "Contract 1" });
        }

        if (!_context.Departments.Any())
        {
            _context.Departments.Add(new Department { Id = 1, Name = "Department 1" });
        }

        if (!_context.Roles.Any())
        {
            _context.Roles.Add(new ApplicationRole { Id = "1", Name = "Role 1" });
        }

        if (!_context.Users.Any())
        {
            var user = new ApplicationUser { Id = "1", UserName = "testUser" };
            _context.Users.Add(user);
            _context.UserRoles.Add(new IdentityUserRole<string> { UserId = user.Id, RoleId = "1" });
        }

        _context.SaveChanges();
    }

    [Fact]
    public async Task TestCreateProcessTemplate()
    {
        // Arrange
        ProcessTemplate processTemplate = new ProcessTemplate
        {
            Title = "Test",
            Description = "Test",
            ContractOfRefWorker = _context.Contracts.FirstOrDefault(),
            DepartmentOfRefWorker = _context.Departments.FirstOrDefault(),
            RolesWithAccess = new List<ApplicationRole> { _context.Roles.FirstOrDefault() },
            AssignmentTemplates = new List<AssignmentTemplate> { }
        };

        // Act
        var result = await _container.AddProcessTemplateAsync(processTemplate);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test", result.Title);
    }

    [Fact]
    public async Task TestUpdateProcessTemplate()
    {
        // Arrange
        var processTemplate = new ProcessTemplate
        {
            Title = "Test",
            Description = "Test",
            ContractOfRefWorker = _context.Contracts.FirstOrDefault(),
            DepartmentOfRefWorker = _context.Departments.FirstOrDefault(),
            RolesWithAccess = new List<ApplicationRole> { _context.Roles.FirstOrDefault() },
            AssignmentTemplates = new List<AssignmentTemplate> { }
        };
        _context.ProcessTemplates.Add(processTemplate);
        await _context.SaveChangesAsync();

        processTemplate.Title = "Updated Test";
        processTemplate.Description = "Updated Test Description";

        // Act
        await _container.UpdateProcessTemplateAsync(processTemplate);

        // Assert
        var updatedTemplate = await _context.ProcessTemplates.FirstOrDefaultAsync(pt =>
            pt.Id == processTemplate.Id
        );
        Assert.NotNull(updatedTemplate);
        Assert.Equal("Updated Test", updatedTemplate.Title);
        Assert.Equal("Updated Test Description", updatedTemplate.Description);
    }

    [Fact]
    public async Task TestDeleteProcessTemplate()
    {
        // Arrange
        var processTemplate = new ProcessTemplate
        {
            Title = "Test",
            Description = "Test",
            ContractOfRefWorker = _context.Contracts.FirstOrDefault(),
            DepartmentOfRefWorker = _context.Departments.FirstOrDefault(),
            RolesWithAccess = new List<ApplicationRole> { _context.Roles.FirstOrDefault() },
            AssignmentTemplates = new List<AssignmentTemplate> { }
        };
        _context.ProcessTemplates.Add(processTemplate);
        await _context.SaveChangesAsync();

        // Act
        await _container.DeleteProcessTemplateAsync(processTemplate.Id);

        // Assert
        var deletedTemplate = await _context.ProcessTemplates.FirstOrDefaultAsync(pt =>
            pt.Id == processTemplate.Id
        );
        Assert.Null(deletedTemplate);
    }

    [Fact]
    public async Task TestGetProcessTemplateByIdAsync()
    {
        // Arrange
        var processTemplate = new ProcessTemplate
        {
            Title = "Test",
            Description = "Test",
            ContractOfRefWorker = _context.Contracts.FirstOrDefault(),
            DepartmentOfRefWorker = _context.Departments.FirstOrDefault(),
            RolesWithAccess = new List<ApplicationRole> { _context.Roles.FirstOrDefault() },
            AssignmentTemplates = new List<AssignmentTemplate> { }
        };
        _context.ProcessTemplates.Add(processTemplate);
        await _context.SaveChangesAsync();

        // Act
        var result = await _container.GetProcessTemplateByIdAsync(processTemplate.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(processTemplate.Id, result.Id);
    }
}
