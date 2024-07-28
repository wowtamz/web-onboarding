//-------------------------
// Author: Vincent Steiner
//-------------------------

using Microsoft.EntityFrameworkCore;
using SoPro24Team06.Data;
using SoPro24Team06.Enums;
using SoPro24Team06.Models;
using SoPro24Team06.Interfaces;

namespace SoPro24Team06.Containers;

public class AssignmentTemplateContainer : IAssignmentTemplate
{
    private readonly ApplicationDbContext _context;

    public AssignmentTemplateContainer(ApplicationDbContext context)
    {
        _context = context;
    }

    public AssignmentTemplate AddAssignmentTemplate( // Assignment Templates werden zur DB hinzugefügt
        string title,
        string? instructions,
        DueTime dueIn,
        List<Department>? forDepartmentsList,
        List<Contract>? forContractsList,
        AssigneeType assigneeType,
        ApplicationRole? assignedRole,
        int? processTemplateId
    )
    {
        try
        {
            AssignmentTemplate template = new AssignmentTemplate(
                title,
                instructions,
                dueIn,
                forDepartmentsList,
                forContractsList,
                assigneeType,
                assignedRole,
                processTemplateId
            );
            var assignmentTemplateID = _context.AssignmentTemplates.Add(template);
            _context.SaveChanges();
            return assignmentTemplateID.Entity;
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    public void DeleteAssignmentTemplate(int id) // Assignment Templates werden aus der DB gelöscht
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

    public void EditAssignmentTemplates( // Assignment Templates aus der DB werden editiert und dann wieder zurück gespeichert
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
            try{
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
             catch (Exception ex)
            {
            }
        }
    }

    public AssignmentTemplate GetAssignmentTemplate(int id) // Gewuünschtes Assignment Template wird mit der ID ausgegeben
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

    public AssignmentTemplate? GetAssignmentTemplateWithDepartmentsAsync(int id) // Laden der Daten aus der Datenbank 
    {
        AssignmentTemplate? at = _context
            .AssignmentTemplates.Include(at => at.ForDepartmentsList)
            .Include(at => at.ForContractsList)
            .Include(at => at.AssignedRole)
            .Include(at => at.DueIn) 
            .FirstOrDefault(at => at.Id == id);
        return at;
    }

    public List<AssignmentTemplate> GetAllAssignmentTemplates() // Alle AssignmentTemplates werden ausgegeben
    {
        List<AssignmentTemplate> assignmentTemplateList = new List<AssignmentTemplate>();
        if (_context.AssignmentTemplates != null)
        {
            assignmentTemplateList = _context.AssignmentTemplates.Include(a => a.DueIn).ToList();
        }
        return assignmentTemplateList;
    }
}
