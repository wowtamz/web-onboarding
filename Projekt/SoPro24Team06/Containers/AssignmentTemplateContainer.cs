using SoPro24Team06.Models;
using SoPro24Team06.Data;
using System.Collections.Generic;
using SoPro24Team06.Enums;

namespace SoPro24Team06.Container;

public class AssignmentTemplateContainer
{
    private ModelContext db = new ModelContext();

    public void AddAssignmentTemplate(string title, string? instructions, DueTime dueIn, List<Department>? forDepartmentsList, List<Contract>? forContractsList, AssigneeType assigneType, List<Role>? assignedRoles)
    {
        db.Add(new AssignmentTemplate { Title = title, Instructions = instructions, DueIn = dueIn, ForDepartmentList = forDepartmentList, ForContractList = forContractList, AssigneType = assigneType, AssignedRoles = assignedRoles });
        db.SaveChanges();
    }
    public void DeleteAssignmentTemplate(int id)
    {
        if (db.AssignmentTemplates != null)
        {
            AssignmentTemplate? assignmentTemplate = db.AssignmentTemplates.FirstOrDefault(assignmentTemplate => assignmentTemplate.id == id);
            if (assignmentTemplate != null)
            {
                db.Remove(assignmentTemplate!);
                db.SaveChanges();
            }
        }
    }
    public void EditAssignmentTemplate(string title, string? instructions, DueTime dueIn, List<Department>? forDepartmentsList, List<Contract>? forContractsList, AssigneeType assigneeType, List<Role>? assignedRoles)
    {
        if (db.AssignmentTemplates != null)
        {
            AssignmentTemplate? assignmentTemplate = db.AssignmentTemplates.FirstOrDefault(assignmentTemplate => assignmentTemplate.id == id);
            if (assignmentTemplate != null)
            {
                assignmentTemplate.Title = title;
                assignmentTemplate.Instructions = instructions;
                assignmentTemplate.DueIn = dueIn;
                assignmentTemplate.ForDepartmentsList = forDepartmentsList;
                assignmentTemplate.ForContractsList = forContractsList;
                assignmentTemplate.AssigneeType = assigneeType;
                assignmentTemplate.AssignedRolesList = assignedRoles;
                db.Update(assignmentTemplate);
                db.SaveChanges();
            }
        }
    }
    public AssignmentTemplate GetAssignmentTemplate(int id)
    {
        if (db.AssignmentTemplates != null)
        {
            AssignmentTemplate? assignmentTemplate = db.AssignmentTemplates.FirstOrDefault(assignmentTemplate => assignmentTemplate.id == id);
            if (assignmentTemplate != null)
            {
                return assignmentTemplate!;
            }
        }
        return null;
    }

    public List<AssignmentTemplate> GetAllAssignmentTemplate()
    {
        List<AssignmentTemplate> assignmentTemplateList = new List<AssignmentTemplate>();
        if (db.AssignmentTemplates != null)
        {
            lessons = db.AssignmentTemplates.ToList();
        }
        return assignmentTemplateList;
    }
}