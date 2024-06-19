using Microsoft.AspNetCore.Identity;
using SoPro24Team06.Enums;
using SoPro24Team06.Models;
using System.ComponentModel.DataAnnotations;

namespace SoPro24Team06.ViewModels;


public class AssigmentTemplateViewModel
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Der Titel ist erforderlich")]
    public string Title { get; set; }
    public string? Instructions { get; set; }
    [Required(ErrorMessage = "Ein Fälligkeit ist erforderlich")]
    public DueTime DueIn { get; set; }
    public List<Department> ForDepartmentsList { get; set; }
    public List<Contract> ForContractsList { get; set; }
    [Required(ErrorMessage = "Ein Aufgabenverantwortlicher ist erforderlich")]
    public AssigneeType AssigneeType { get; set; }
    public List<IdentityRole> AssignedRolesList { get; set; }

    public AssigmentTemplateViewModel(AssignmentTemplate assignmentTemplate)
    {
        Id = assignmentTemplate.Id;
        Title = assignmentTemplate.Title ?? "";
        Instructions = assignmentTemplate.Instructions;
        DueIn = assignmentTemplate.DueIn;
        ForDepartmentsList = assignmentTemplate.ForDepartmentsList;
        ForContractsList = assignmentTemplate.ForContractsList;
        AssigneeType = assignmentTemplate.AssigneeType;
        AssignedRolesList = assignmentTemplate.AssignedRolesList;
    }
}
