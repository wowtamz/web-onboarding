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
        return await GetProcessTemplatesWithIncludes().ToListAsync();
    }

    /// <summary>
    /// Retrieves a list of ProcessTemplates based on the access rights of the user.
    /// </summary>
    /// <param name="userName">The username of the user.</param>
    /// <returns>A list of ProcessTemplates accessible by the user.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the user or their role is not found.</exception>
    public async Task<List<ProcessTemplate>> GetProcessListByAccessRights(string userName)
    {
        var user = _context.Users.Where(u => u.UserName == userName).FirstOrDefault();

        if (user == null)
        {
            throw new InvalidOperationException($"No User found with UserName {userName}");
        }

        var userRole = _context.UserRoles.Where(r => r.UserId == user.Id).FirstOrDefault();
        var role = _context.Roles.Where(r => r.Id == userRole.RoleId).FirstOrDefault();

        if (role == null)
        {
            throw new InvalidOperationException($"No Role found for User {userName}");
        }

        if (role.Name == "Administrator")
        {
            return await GetProcessTemplatesAsync();
        }
        else
        {
            return await GetProcessTemplatesWithIncludes()
                .Where(pt => pt.RolesWithAccess.Contains(role))
                .ToListAsync();
        }
    }

    /// <summary>
    /// Retrieves a ProcessTemplate by its ID.
    /// </summary>
    /// <param name="id">The ID of the ProcessTemplate.</param>
    /// <returns>The ProcessTemplate with the specified ID.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no ProcessTemplate is found with the given ID.</exception>
    public async Task<ProcessTemplate> GetProcessTemplateByIdAsync(int id)
    {
        ProcessTemplate processTemplate =
            await GetProcessTemplatesWithIncludes().FirstOrDefaultAsync(pt => pt.Id == id)
            ?? throw new InvalidOperationException($"No Process found with Id {id}");
        return processTemplate;
    }

    /// <summary>
    /// Adds a new ProcessTemplate to the database.
    /// </summary>
    /// <param name="processTemplateToAdd">The ProcessTemplate to add.</param>
    /// <returns>The added ProcessTemplate.</returns>
    public async Task<ProcessTemplate> AddProcessTemplateAsync(ProcessTemplate processTemplateToAdd)
    {
        var pt = _context.ProcessTemplates.Add(processTemplateToAdd);

        await _context.SaveChangesAsync();

        return pt.Entity;
    }

    /// <summary>
    /// Updates an existing ProcessTemplate in the database.
    /// </summary>
    /// <param name="processTemplate">The ProcessTemplate to update.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no ProcessTemplate is found with the given ID.</exception>
    public async Task UpdateProcessTemplateAsync(ProcessTemplate processTemplate)
    {
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

    /// <summary>
    /// Deletes a ProcessTemplate from the database.
    /// </summary>
    /// <param name="id">The ID of the ProcessTemplate to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no ProcessTemplate is found with the given ID.</exception>
    public async Task DeleteProcessTemplateAsync(int id)
    {
        ProcessTemplate processTemplate =
            await _context.ProcessTemplates.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new InvalidOperationException($"No Process found with Id {id}");
        _context.ProcessTemplates.Remove(processTemplate);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Retrieves a queryable collection of ProcessTemplates with related data included.
    /// </summary>
    /// <returns>An IQueryable of ProcessTemplates with related data.</returns>
    private IQueryable<ProcessTemplate> GetProcessTemplatesWithIncludes()
    {
        return _context
            .ProcessTemplates.Include(pt => pt.DepartmentOfRefWorker)
            .Include(pt => pt.AssignmentTemplates)
            .Include(pt => pt.ContractOfRefWorker)
            .Include(pt => pt.RolesWithAccess);
    }
}
