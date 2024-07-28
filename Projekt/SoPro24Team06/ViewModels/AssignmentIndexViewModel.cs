using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using SoPro24Team06.Enums;
using SoPro24Team06.Helpers;
using SoPro24Team06.Models;

namespace SoPro24Team06.ViewModels;

public class AssignmentIndexViewModel
{
    /// <summary>
    /// List of Assignments, wich will be Displayed in View
    /// </summary>
    public List<Assignment> AssignmentList { get; set; }

    /// <summary>
    /// List of Processes, from wich the Assignemnts can be Filtered
    /// </summary>
    public List<Process> ProcessList { get; set; }

    /// <summary>
    /// true if User has Role Administrator
    /// also true if User is Supervisor of atleast one Process
    /// </summary>
    public Boolean IsAdminOrSupervisor { get; set; }

    /// <summary>
    /// Constructs an AssigmentIndexViewModel
    /// </summary>
    /// <param name="assignmentList">List of Assignments wich should be Displayed</param>
    /// <param name="processList">List of Processes for wich the Assignments can be Filtered</param>
    public AssignmentIndexViewModel(List<Assignment> assignmentList, List<Process> processList)
    {
        this.AssignmentList = assignmentList;
        this.ProcessList = processList;
    }

    /// <summary>
    /// Filters the AssignmentsList by a ProcessId (for the filter to apply ProcessId has to match the Id of Process in ProcessList)
    /// </summary>
    /// <param name="selectedProcessId">ProcessId of process for wich should be filtered</param>
    public void FilterAssignments(int selectedProcessId)
    {
        if (selectedProcessId != 0)
        {
            Process? process = ProcessList.Find(p => p.Id == selectedProcessId);
            if (process != null)
            {
                this.AssignmentList = process
                    .Assignments.Where(p => this.AssignmentList.Contains(p))
                    .ToList();
            }
        }
    }

    /// <summary>
    /// Used for sorting Assignments by predefindes Methods
    /// Only applies Sorting if sortingPropety matches
    /// </summary>
    /// <param name="sortingProperty">only applies Sorting if matches one of: "dueDate", "name"</param>
    public void SortAssingments(string sortingProperty)
    {
        switch (sortingProperty)
        {
            case "dueDate":
                AssignmentList.Sort((x, y) => DateTime.Compare(x.DueDate, y.DueDate));
                List<Assignment> tempAssignmentList = new List<Assignment>();
                foreach (Assignment a in AssignmentList)
                {
                    if (a.Status == AssignmentStatus.DONE)
                    {
                        tempAssignmentList.Add(a);
                    }
                    else
                    {
                        tempAssignmentList.Insert(0, a);
                    }
                }
                AssignmentList = tempAssignmentList;
                break;
            case "name":
                this.AssignmentList.Sort((x, y) => String.Compare(x.Title, y.Title));
                break;
            default:
                break;
        }
    }

    public string GetRowClass(Assignment assignment)
    {
        if (assignment.Status == AssignmentStatus.DONE)
            return "done";
        if (assignment.GetDaysTillDueDate() < 0)
            return "overdue";
        return string.Empty;
    }

    /// <summary>
    /// Gets Status of Assignment
    /// Used by view to correctly display assignment Status
    /// </summary>
    /// <param name="assignment">assignment for wich status should be displayed</param>
    /// <returns>Display Name of Status as String</returns>
    public string GetAssignmentStatus(Assignment assignment)
    {
        return EnumHelper.GetDisplayName(assignment.Status);
    }

    /// <summary>
    ///Used to get Title of Process wich contains Assignment
    /// </summary>
    /// <param name="assignment">assignment for wich Title of Process should be return</param>
    /// <returns>Title of Process as String</returns>
    public string GetProcessByAssingment(Assignment assignment)
    {
        Process? process = ProcessList.FirstOrDefault(p => p.Assignments.Contains(assignment));
        if (process != null)
        {
            return process.Title;
        }
        return "";
    }
}
