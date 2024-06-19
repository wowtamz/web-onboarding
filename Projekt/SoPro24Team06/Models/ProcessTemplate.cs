using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace SoPro24Team06.Models;

public class ProcessTemplate
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; }

    [Required(ErrorMessage = "Title ist erforderlich")]
    public string Title { get; set; }

    public string? Description { get; set; }

    [Required(ErrorMessage = "Mindestens eine Aufgabe ist erforderlich")]
    public List<AssignmentTemplate> AssignmentTemplates { get; set; }

    [Required(ErrorMessage = "Vertrag ist erforderlich")]
    public Contract ContractOfRefWorker { get; set; }

    [Required(ErrorMessage = "Abteilung ist erforderlich")]
    public Department DepartmentOfRefWorker { get; set; }

    public List<IdentityRole> RolesWithAccess { get; set; }

    public ProcessTemplate(string title, string description, List<AssignmentTemplate> assignmentTemplates, Contract contractOfRefWorker, Department departmentOfRefWorker, List<IdentityRole> rolesWithAccess)
    {
        this.Title = title;
        this.Description = description;
        this.AssignmentTemplates = assignmentTemplates;
        this.ContractOfRefWorker = contractOfRefWorker;
        this.DepartmentOfRefWorker = departmentOfRefWorker;
        this.RolesWithAccess = rolesWithAccess;
    }

    public Process ToProcess()
    {
        /** #TODO: Roles with Access?? */
        return new Process(this);
    }
}
