//-------------------------
// Author: Vincent Steiner
//-------------------------

using Microsoft.AspNetCore.Identity;
using SoPro24Team06.Data;
using SoPro24Team06.Enums;
using SoPro24Team06.Models;
using SoPro24Team06.Interfaces;

namespace SoPro24Team06.Containers;

public class DepartmentContainer : IDepartment
{
    private readonly ApplicationDbContext _context;
    public DepartmentContainer(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Department> GetDepartment(string name)
    {
        if (_context.DueTimes != null)
        {
            Department department = _context.Departments.FirstOrDefault(department =>
                department.Name == name
            );
            if (department != null)
            {
                return department!;
            }
        }
        return null;
    }
}
