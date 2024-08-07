//-------------------------
// Author: Vincent Steiner
//-------------------------

using Microsoft.AspNetCore.Identity;
using SoPro24Team06.Data;
using SoPro24Team06.Enums;
using SoPro24Team06.Models;
using SoPro24Team06.Interfaces;

namespace SoPro24Team06.Containers;

public class DueTimeContainer : IDueTime
{
    private readonly ApplicationDbContext _context;

    public DueTimeContainer(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddDueTime(DueTime dueIn)
    {
        _context.DueTimes.Add(dueIn);
        _context.SaveChanges();
    }

    public async Task<DueTime> GetDueTime(string label)
    {
        if (_context.DueTimes != null)
        {
            DueTime? dueTime = _context.DueTimes.FirstOrDefault(dueTime => dueTime.Label == label);
            if (dueTime != null)
            {
                return dueTime!;
            }
        }
        return null;
    }

    public async Task DeleteDueTime(int id)
    {
        if (_context.DueTimes != null)
        {
            DueTime? dueTime = _context.DueTimes.FirstOrDefault(dueTime => dueTime.Id == id);
            if (dueTime != null)
            {
                _context.DueTimes.Remove(dueTime!);
                _context.SaveChanges();
            }
        }
    }

    public async Task<List<DueTime>> GetAllDueTimes()
    {
        List<DueTime> dueTimeList = new List<DueTime>();
        if (_context.DueTimes != null)
        {
            dueTimeList = _context.DueTimes.ToList();
        }
        return dueTimeList;
    }
}
