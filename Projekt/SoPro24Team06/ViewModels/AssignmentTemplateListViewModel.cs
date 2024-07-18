using SoPro24Team06.Models;

namespace SoPro24Team06.ViewModels;

public class AssignmentTemplateListViewModel
{
    public List<AssignmentTemplate> AssignmentTemplateList { get; set; }

    public AssignmentTemplateListViewModel(List<AssignmentTemplate> assignmentTemplateList)
    {
        this.AssignmentTemplateList = assignmentTemplateList;
    }

    public AssignmentTemplateListViewModel()
    {
        this.AssignmentTemplateList = new List<AssignmentTemplate>();
    }
}
