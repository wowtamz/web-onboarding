using Microsoft.AspNetCore.Identity;
using SoPro24Team06.Models;

namespace SoPro24Team06.ViewModels;

public class ProcessTemplateViewModel
{
    public int? Id { get; }

    public string Title { get; set; }

    public string? Description { get; set; }

    public List<AssignmentTemplate> AssignmentTemplates { get; set; }

    public Contract ContractOfRefWorker { get; set; }

    public Department DepartmentOfRefWorker { get; set; }

    public List<IdentityRole> RolesWithAccess { get; set; }

    public ProcessTemplateViewModel(ProcessTemplate processTemplate)
    {
        Id = processTemplate.Id;
        Title = processTemplate.Title;
        Description = processTemplate.Description;
        AssignmentTemplates = processTemplate.AssignmentTemplates;
        ContractOfRefWorker = processTemplate.ContractOfRefWorker;
        DepartmentOfRefWorker = processTemplate.DepartmentOfRefWorker;
        RolesWithAccess = processTemplate.RolesWithAccess;
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
    }
}