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
    public List<Assignment> AssignmentList { get; set; }
    public List<Process> ProcessList { get; set; }
    public Boolean IsAdminOrSupervisor { get; set; }

    public AssignmentIndexViewModel(List<Assignment> assignmentList, List<Process> processList)
    {
        this.AssignmentList = assignmentList;
        this.ProcessList = processList;
    }

    public void FilterAssignments(int selectedProcessId)
    {
        if (selectedProcessId != 0)
        {
            Process? process = ProcessList.FirstOrDefault(p => p.Id == selectedProcessId);
            if (process != null)
            {
                this.AssignmentList = process
                    .Assignments.Where(p => this.AssignmentList.Contains(p))
                    .ToList();
            }
        }
    }

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

    public string GetAssignmentStatus(Assignment assignment)
    {
        return EnumHelper.GetDisplayName(assignment.Status);
    }

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
