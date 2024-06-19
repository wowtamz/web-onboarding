using SoPro24Team06.Models;

namespace SoPro24Team06.ViewModels;

public class AssignmentListViewModel
{
    public List<Assignment> AssignmentList { get; set; }

    public AssignmentListViewModel(List<Assignment> assignmentList)
    {
        this.AssignmentList = assignmentList;
    }
}