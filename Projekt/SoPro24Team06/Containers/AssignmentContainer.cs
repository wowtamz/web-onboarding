using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SoPro24Team06.Data;
using SoPro24Team06.Enums;
using SoPro24Team06.Models;

namespace SoPro24Team06.Container;

public class AssingmentContainer
{
    private readonly ApplicationDbContext _context;

    public AssingmentContainer(ApplicationDbContext context)
    {
        this._context = context;
    }

    //beginn codeownership Jan Pfluger
    public Assignment? GetAssignmentById(int id)
    {
        return _context
            .Assignments.Include(a => a.AssignedRole)
            .Include(a => a.Assignee)
            .ToList()
            .Find(a => a.Id == id);
    }

    public async Task SaveEditedAssignment(Assignment assignment)
    {
        _context.Update(assignment);
        switch (assignment.AssigneeType)
        {
            case AssigneeType.ROLES:
                _context.Entry(assignment).Reference(a => a.Assignee).CurrentValue = null;
                break;
            case AssigneeType.USER:
                _context.Entry(assignment).Reference(a => a.AssignedRole).CurrentValue = null;
                break;
            default:
                break;
        }
        await _context.SaveChangesAsync();
    }

    public async Task AddAssingmentAsync(Assignment assignmentToAdd)
    {
        _context.Assignments.Add(assignmentToAdd);
        await _context.SaveChangesAsync();
    }

    public List<Assignment> GetAllAssignments()
    {
        return _context.Assignments.Include(a => a.AssignedRole).Include(a => a.Assignee).ToList();
    }

    public async Task DeleteAssignmentAsync(Assignment assignment)
    {
        if (_context.Assignments.Contains(assignment))
        {
            _context.Assignments.Remove(assignment);
            await _context.SaveChangesAsync();
        }
        else
        {
            throw new InvalidOperationException();
        }
    }
}
//end codeownership Jan Pfluger
