using Microsoft.EntityFrameworkCore;
using SoPro24Team06.Data;
using SoPro24Team06.Models;

namespace SoPro24Team06.Containers;

public class ProcessTemplateContainer
{
    private readonly ModelContext _context;

    public ProcessTemplateContainer()
    {
        _context = new ModelContext();
    }

    public async Task<List<ProcessTemplate>> GetProcessTemplatesAsync()
    {
        List<ProcessTemplate> processTemplateList = await _context.ProcessTemplates.ToListAsync();
        return processTemplateList;
    }

    public async Task<ProcessTemplate> GetProcessTemplateByIdAsync(int id)
    {
        ProcessTemplate processTemplate =
            await _context.ProcessTemplates.FindAsync(id)
            ?? throw new InvalidOperationException($"No Process found with Id {id}");
        return processTemplate;
    }

    public async Task AddProcessTemplateAsync(ProcessTemplate processTemplateToAdd)
    {
        _context.ProcessTemplates.Add(processTemplateToAdd);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateProcessTemplateAsync(
        int id,
        string title,
        string description,
        List<AssignmentTemplate> assignmentTemplates
    )
    {
        ProcessTemplate processTemplateToUpdate =
            await _context.ProcessTemplates.FirstOrDefaultAsync(x => x.Id.Equals(id))
            ?? throw new InvalidOperationException($"No Process found with Id {id}");

        processTemplateToUpdate.Title = title;
        processTemplateToUpdate.Description = description;
        processTemplateToUpdate.AssignmentTemplates = assignmentTemplates;

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
