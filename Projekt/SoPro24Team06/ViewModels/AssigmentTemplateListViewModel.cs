using SoPro24Team06.Models;

namespace SoPro24Team06.ViewModels;


public class AssigmentTemplateListViewModel
{
    public List<AssignmentTemplate> AssignmentTemplateList { get; set; }

    public AssigmentTemplateListViewModel(List<AssignmentTemplate> assignmentTemplateList)
    {
        this.AssignmentTemplateList = assignmentTemplateList;
    }

    public AssigmentTemplateListViewModel()
    {
        this.AssignmentTemplateList = new List<AssignmentTemplate>();
    }
}
