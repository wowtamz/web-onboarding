using SoPro24Team06.Enums;
using SoPro24Team06.Models;
using SoPro24Team06.Models.SoPro24Team06.Models;
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
    public List<Role> AssignedRolesList { get; set; }
}
