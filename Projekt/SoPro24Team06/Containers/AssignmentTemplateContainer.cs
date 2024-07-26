using Microsoft.EntityFrameworkCore;
using SoPro24Team06.Data;
using SoPro24Team06.Enums;
using SoPro24Team06.Models;

namespace SoPro24Team06.Containers;

public class AssignmentTemplateContainer
{
    private readonly ApplicationDbContext _context;

    public AssignmentTemplateContainer(ApplicationDbContext context)
    {
        _context = context;
    }

    public AssignmentTemplate AddAssignmentTemplate(
        string title,
        string? instructions,
        DueTime dueIn,
        List<Department>? forDepartmentsList,
        List<Contract>? forContractsList,
        AssigneeType assigneType,
        ApplicationRole? assignedRole,
        int? processTemplateId
    )
    {
        AssignmentTemplate template = new AssignmentTemplate(
            title,
            instructions,
            dueIn,
            forDepartmentsList,
            forContractsList,
            assigneType,
            assignedRole,
            processTemplateId
        );
        var assignmentTemplateID = _context.AssignmentTemplates.Add(template);
        _context.SaveChanges();
        return assignmentTemplateID.Entity;
    }

    public void DeleteAssignmentTemplate(int id)
    {
        if (_context.AssignmentTemplates != null)
        {
            AssignmentTemplate? assignmentTemplate = GetAssignmentTemplateWithDepartmentsAsync(id);
            if (assignmentTemplate != null)
            {
                _context.AssignmentTemplates.Remove(assignmentTemplate!);
                _context.SaveChanges();
            }
        }
    }

    public void EditAssignmentTemplates(
        int id,
        string title,
        string? instructions,
        DueTime dueIn,
        List<Department>? forDepartmentsList,
        List<Contract>? forContractsList,
        AssigneeType assigneeType,
        ApplicationRole? assignedRole
    )
    {
        if (_context.AssignmentTemplates != null)
        {
            AssignmentTemplate? assignmentTemplate = GetAssignmentTemplateWithDepartmentsAsync(id);
            if (assignmentTemplate != null)
            {
                assignmentTemplate.Title = title;
                assignmentTemplate.Instructions = instructions;
                assignmentTemplate.DueIn = dueIn;
                assignmentTemplate.ForDepartmentsList = forDepartmentsList;
                assignmentTemplate.ForContractsList = forContractsList;
                assignmentTemplate.AssigneeType = assigneeType;
                assignmentTemplate.AssignedRole = assignedRole;

                _context.AssignmentTemplates.Update(assignmentTemplate);
                _context.SaveChanges();
            }
        }
    }

    public AssignmentTemplate GetAssignmentTemplate(int id)
    {
        if (_context.AssignmentTemplates != null)
        {
            AssignmentTemplate? assignmentTemplate = GetAssignmentTemplateWithDepartmentsAsync(id);

            if (assignmentTemplate != null)
            {
                return assignmentTemplate;
            }
        }

        return null;
    }

    public AssignmentTemplate? GetAssignmentTemplateWithDepartmentsAsync(int id)
    {
        AssignmentTemplate? at = _context
            .AssignmentTemplates.Include(at => at.ForDepartmentsList) // Include für das Laden der ForDepartmentsList
            .Include(at => at.ForContractsList) // Include für das Laden der ForContractList
            .Include(at => at.AssignedRole) // Include für das Laden der AssignedRole
            .Include(at => at.DueIn) // Include für das Laden der DueIn
            .FirstOrDefault(at => at.Id == id);
        return at;
    }

    public List<AssignmentTemplate> GetAllAssignmentTemplates()
    {
        List<AssignmentTemplate> assignmentTemplateList = new List<AssignmentTemplate>();
        if (_context.AssignmentTemplates != null)
        {
            assignmentTemplateList = _context.AssignmentTemplates.Include(a => a.DueIn).ToList();
        }
        return assignmentTemplateList;
    }
}
