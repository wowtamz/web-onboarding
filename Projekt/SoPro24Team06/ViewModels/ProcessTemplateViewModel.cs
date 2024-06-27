using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using SoPro24Team06.Data;
using SoPro24Team06.Models;

namespace SoPro24Team06.ViewModels;

public class ProcessTemplateViewModel
{
    private readonly ModelContext _modelContext = new();

    // Values to be selected by the user
    public int? Id { get; }

    [Required(ErrorMessage = "Titel ist erforderlich")]
    public string Title { get; set; }

    public string? Description { get; set; }

    public Department DepartmentOfRefWorker { get; set; } = new();

    [Required(ErrorMessage = "Mindestens eine Aufgabe ist erforderlich")]
    public List<AssignmentTemplate> SelectedAssignmentTemplates { get; set; } = new();

    public List<IdentityRole> RolesWithAccess { get; set; } = new();

    public Contract ContractOfRefWorker { get; set; } = new();

    // Values to choose from
    public List<AssignmentTemplate> AssignmentTemplates { get; set; } = new();
    public List<Contract> Contracts { get; set; } = new();
    public List<Department> Departments { get; set; } = new();
    public List<IdentityRole> Roles { get; set; } = new();

    public ProcessTemplateViewModel(ProcessTemplate processTemplate)
    {
        Id = processTemplate.Id;
        Title = processTemplate.Title;
        Description = processTemplate.Description;
        AssignmentTemplates = processTemplate.AssignmentTemplates;
        ContractOfRefWorker = processTemplate.ContractOfRefWorker;
        DepartmentOfRefWorker = processTemplate.DepartmentOfRefWorker;
        RolesWithAccess = processTemplate.RolesWithAccess;
        AssignmentTemplates = _modelContext.AssignmentTemplates.ToList();
        Contracts = _modelContext.Contracts.ToList();
        Departments = _modelContext.Departments.ToList();
        Roles = new List<IdentityRole>();

    }

    public ProcessTemplateViewModel()
    {
        Id = null;
        Title = "";
        Description = "";
        AssignmentTemplates = new List<AssignmentTemplate>();
        ContractOfRefWorker = new Contract();
        DepartmentOfRefWorker = new Department();
        RolesWithAccess = new List<IdentityRole>();
        AssignmentTemplates = _modelContext.AssignmentTemplates.ToList();
        Contracts = _modelContext.Contracts.ToList();
        Departments = _modelContext.Departments.ToList();
        Roles = new List<IdentityRole>();
    }
}