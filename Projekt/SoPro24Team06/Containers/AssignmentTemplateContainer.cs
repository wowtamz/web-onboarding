using SoPro24Team06.Models;
using SoPro24Team06.Data;
using SoPro24Team06.Enums;
using Microsoft.AspNetCore.Identity;

namespace SoPro24Team06.Containers;

public class AssignmentTemplateContainer
{
    private readonly ModelContext _context;

    public AssignmentTemplateContainer(ModelContext context)
    {
        _context = context;
    }

    public void AddAssignmentTemplate(string title, string? instructions, DueTime dueIn, List<Department>? forDepartmentsList, List<Contract>? forContractsList, AssigneeType assigneType, List<IdentityRole>? assignedRoles)
    {
        _context.Add(new AssignmentTemplate(title, instructions, dueIn, forDepartmentsList, forContractsList, assigneType, assignedRoles) { Title = title, Instructions = instructions, DueIn = dueIn, ForDepartmentsList = forDepartmentsList, ForContractsList = forContractsList, AssigneeType = assigneType, AssignedRolesList = assignedRoles });
        _context.SaveChanges();
    }
    public void DeleteAssignmentTemplate(int id)
    {
        if (_context.AssignmentTemplates != null)
        {
            AssignmentTemplate? assignmentTemplate = _context.AssignmentTemplates.FirstOrDefault(assignmentTemplate => assignmentTemplate.Id == id);
            if (assignmentTemplate != null)
            {
                _context.Remove(assignmentTemplate!);
                _context.SaveChanges();
            }
        }
    }
    public void EditAssignmentTemplate(int id, string title, string? instructions, DueTime dueIn, List<Department>? forDepartmentsList, List<Contract>? forContractsList, AssigneeType assigneeType, List<IdentityRole>? assignedRoles)
    {
        if (_context.AssignmentTemplates != null)
        {
            AssignmentTemplate? assignmentTemplate = _context.AssignmentTemplates.FirstOrDefault(assignmentTemplate => assignmentTemplate.Id == id);
            if (assignmentTemplate != null)
            {
                assignmentTemplate.Title = title;
                assignmentTemplate.Instructions = instructions;
                assignmentTemplate.DueIn = dueIn;
                assignmentTemplate.ForDepartmentsList = forDepartmentsList;
                assignmentTemplate.ForContractsList = forContractsList;
                assignmentTemplate.AssigneeType = assigneeType;
                assignmentTemplate.AssignedRolesList = assignedRoles;
                _context.Update(assignmentTemplate);
                _context.SaveChanges();
            }
        }
    }
    public AssignmentTemplate GetAssignmentTemplate(int id)
    {
        if (_context.AssignmentTemplates != null)
        {
            AssignmentTemplate? assignmentTemplate = _context.AssignmentTemplates.FirstOrDefault(assignmentTemplate => assignmentTemplate.Id == id);
            if (assignmentTemplate != null)
            {
                return assignmentTemplate!;
            }
        }
        return null;
    }

    public List<AssignmentTemplate> GetAllAssignmentTemplates()
    {
        List<AssignmentTemplate> assignmentTemplateList = new List<AssignmentTemplate>();
        if (_context.AssignmentTemplates != null)
        {
            assignmentTemplateList = _context.AssignmentTemplates.ToList();
        }
        return assignmentTemplateList;
    }
}