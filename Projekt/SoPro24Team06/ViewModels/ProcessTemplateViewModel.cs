//-------------------------
// Author: Kevin Tornquist
//-------------------------

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using SoPro24Team06.Data;
using SoPro24Team06.Models;

namespace SoPro24Team06.ViewModels;

public class ProcessTemplateViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Titel ist erforderlich")]
    public string Title { get; set; }

    public string? Description { get; set; }

    public int DepartmentOfRefWorkerId { get; set; } = new();

    public List<int>? SelectedAssignmentTemplateIds { get; set; } = new();

    [Required(ErrorMessage = "Mindestens eine Rolle ist erforderlich")]
    public List<string> RolesWithAccess { get; set; } = new();

    public int ContractOfRefWorkerId { get; set; } = new();

    public ProcessTemplateViewModel(ProcessTemplate processTemplate)
    {
        Id = processTemplate.Id;
        Title = processTemplate.Title;
        Description = processTemplate.Description;
        ContractOfRefWorkerId = processTemplate.ContractOfRefWorker.Id;
        DepartmentOfRefWorkerId = processTemplate.DepartmentOfRefWorker.Id;
        RolesWithAccess = processTemplate.RolesWithAccess.Select(r => r.Name).ToList();
        SelectedAssignmentTemplateIds = processTemplate
            .AssignmentTemplates.Select(a => a.Id)
            .ToList();
    }

    public ProcessTemplateViewModel()
    {
        Id = null;
        Title = "";
        Description = "";
        ContractOfRefWorkerId = 0;
        DepartmentOfRefWorkerId = 0;
        RolesWithAccess = new();
        SelectedAssignmentTemplateIds = new();
    }
}
