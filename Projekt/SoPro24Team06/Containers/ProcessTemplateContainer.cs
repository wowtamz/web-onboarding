//-------------------------
// Author: Kevin Tornquist
//-------------------------

using Microsoft.EntityFrameworkCore;
using SoPro24Team06.Data;
using SoPro24Team06.Models;

namespace SoPro24Team06.Containers;

public class ProcessTemplateContainer
{
    private readonly ApplicationDbContext _context;

    public ProcessTemplateContainer(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProcessTemplate>> GetProcessTemplatesAsync()
    {
        List<ProcessTemplate> processTemplateList = await _context
            .ProcessTemplates.Include(pt => pt.DepartmentOfRefWorker)
            .Include(pt => pt.AssignmentTemplates)
            .Include(pt => pt.ContractOfRefWorker)
            .Include(pt => pt.RolesWithAccess)
            .ToListAsync();
        return processTemplateList;
    }

    public async Task<ProcessTemplate> GetProcessTemplateByIdAsync(int id)
    {
        ProcessTemplate processTemplate =
            await _context
                .ProcessTemplates.Include(pt => pt.DepartmentOfRefWorker)
                .Include(pt => pt.AssignmentTemplates)
                .Include(pt => pt.ContractOfRefWorker)
                .Include(pt => pt.RolesWithAccess)
                .FirstOrDefaultAsync(pt => pt.Id.Equals(id))
            ?? throw new InvalidOperationException($"No Process found with Id {id}");
        return processTemplate;
    }

    public async Task<ProcessTemplate> AddProcessTemplateAsync(ProcessTemplate processTemplateToAdd)
    {
        var pt = _context.ProcessTemplates.Add(processTemplateToAdd);

        await _context.SaveChangesAsync();

        return pt.Entity;
    }

    public async Task UpdateProcessTemplateAsync(ProcessTemplate processTemplate)
    {
        // Retrieve the existing ProcessTemplate from the database with its related data
        var processTemplateToUpdate =
            await _context
                .ProcessTemplates.Include(pt => pt.AssignmentTemplates)
                .Include(pt => pt.RolesWithAccess)
                .FirstOrDefaultAsync(x => x.Id == processTemplate.Id)
            ?? throw new InvalidOperationException(
                $"No Process found with Id {processTemplate.Id}"
            );

        processTemplateToUpdate.Title = processTemplate.Title;
        processTemplateToUpdate.Description = processTemplate.Description;
        processTemplateToUpdate.DepartmentOfRefWorker = processTemplate.DepartmentOfRefWorker;
        processTemplateToUpdate.ContractOfRefWorker = processTemplate.ContractOfRefWorker;
        processTemplateToUpdate.RolesWithAccess = processTemplate.RolesWithAccess;
        processTemplateToUpdate.AssignmentTemplates = processTemplate.AssignmentTemplates;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteProcessTemplateAsync(int id)
    {
        ProcessTemplate processTemplate =
            await _context.ProcessTemplates.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new InvalidOperationException($"No Process found with Id {id}");
        _context.ProcessTemplates.Remove(processTemplate);
        await _context.SaveChangesAsync();
    }
}
