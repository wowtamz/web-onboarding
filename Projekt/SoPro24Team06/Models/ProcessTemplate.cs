//-------------------------
// Author: Kevin Tornquist
//-------------------------

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;

namespace SoPro24Team06.Models;

public class ProcessTemplate
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [JsonProperty("id")]
    public int Id { get; set; }

    [Required(ErrorMessage = "Titel ist erforderlich")]
    [JsonProperty("title")]
    public string Title { get; set; }

    [JsonProperty("description")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Mindestens eine Aufgabe ist erforderlich")]
    [JsonProperty("assignmentTemplates")]
    public List<AssignmentTemplate> AssignmentTemplates { get; set; }

    [Required(ErrorMessage = "Vertrag ist erforderlich")]
    [JsonProperty("contractOfRefWorker")]
    public Contract ContractOfRefWorker { get; set; }

    [Required(ErrorMessage = "Abteilung ist erforderlich")]
    [JsonProperty("departmentOfRefWorker")]
    public Department DepartmentOfRefWorker { get; set; }

    [JsonProperty("rolesWithAccess")]
    [Required(ErrorMessage = "Mindestens eine Rolle ist erforderlich")]
    public List<ApplicationRole> RolesWithAccess { get; set; }
    


    public ProcessTemplate(
        string title,
        string description,
        List<AssignmentTemplate> assignmentTemplates,
        Contract contractOfRefWorker,
        Department departmentOfRefWorker,
        List<ApplicationRole> rolesWithAccess
    )
    {
        this.Title = title;
        this.Description = description;
        this.AssignmentTemplates = assignmentTemplates;
        this.ContractOfRefWorker = contractOfRefWorker;
        this.DepartmentOfRefWorker = departmentOfRefWorker;
        this.RolesWithAccess = rolesWithAccess;
    }

    public ProcessTemplate()
    {
        this.Title = "Title";
        this.Description = "Description";
        this.AssignmentTemplates = new List<AssignmentTemplate> { };
        this.ContractOfRefWorker = new Contract();
        this.DepartmentOfRefWorker = new Department();
        this.RolesWithAccess = new List<ApplicationRole> { };
    }

    public Process ToProcess()
    {
        /** #TODO: Roles with Access?? */
        return new Process(this);
    }
}
