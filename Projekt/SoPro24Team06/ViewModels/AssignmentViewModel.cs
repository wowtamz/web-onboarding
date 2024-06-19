using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using SoPro24Team06.Enums;
using SoPro24Team06.Models;

namespace SoPro24Team06.ViewModels;

public class AssignmentViewModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("title")]
    public string? Title { get; set; }

    [JsonProperty("instructions")]
    public string? Instructions { get; set; }

    [JsonProperty("dueDate")]
    public DateTime DueDate { get; set; }

    [JsonProperty("forDepartments")]
    public List<Department> ForDepartmentsList { get; set; }

    [JsonProperty("forContract")]
    public List<Contract> ForContractsList { get; set; }

    [JsonProperty("assigneeType")]
    public AssigneeType AssigneeType { get; set; }

    [JsonProperty("assignedRoles")]
    public List<IdentityRole> AssignedRolesList { get; set; }

    // #TODO: Additional Attributes
    public AssignmentViewModel(AssignmentTemplate template)
    {
        Assignment assignment = template.ToAssignment(template);

        this.Title = assignment.Title;
        this.Instructions = assignment.Instructions;
        this.DueDate = assignment.DueDate;
        this.ForDepartmentsList = assignment.ForDepartmentsList;
        this.ForContractsList = assignment.ForContractsList;
        this.AssigneeType = assignment.AssigneeType;
        this.AssignedRolesList = assignment.AssignedRolesList;
    }
}

