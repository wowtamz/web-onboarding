//-------------------------
// Author: Kevin Tornquist
//-------------------------

using SoPro24Team06.Models;

namespace SoPro24Team06.ViewModels;

public class DetailProcessTemplateViewModel
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public List<AssignmentTemplate> AssignmentTemplates { get; set; }
    public Contract ContractOfRefWorker { get; set; }
    public Department DepartmentOfRefWorker { get; set; }
    public List<ApplicationRole> RolesWithAccess { get; set; }

    public DetailProcessTemplateViewModel(ProcessTemplate pt)
    {
        this.Id = pt.Id;
        this.Title = pt.Title;
        this.Description = pt.Description;
        this.AssignmentTemplates = pt.AssignmentTemplates;
        this.ContractOfRefWorker = pt.ContractOfRefWorker;
        this.DepartmentOfRefWorker = pt.DepartmentOfRefWorker;
        this.RolesWithAccess = pt.RolesWithAccess;
    }
}
