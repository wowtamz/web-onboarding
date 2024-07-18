using Microsoft.AspNetCore.Identity;
using SoPro24Team06.Data;
using SoPro24Team06.Enums;
using SoPro24Team06.Models;

namespace SoPro24Team06.Containers;

public class DueTimeContainer
{
    private readonly ApplicationDbContext _context;

    public DueTimeContainer(ApplicationDbContext context)
    {
        _context = context;
    }

    public void AddDueTime(DueTime dueIn)
    {
        _context.DueTimes.Add(dueIn);
        _context.SaveChanges();
    }

    public DueTime GetDueTime(string label)
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

    public void DeleteDueTime(int id)
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

    public List<DueTime> GetAllDueTimes()
    {
        List<DueTime> dueTimeList = new List<DueTime>();
        if (_context.DueTimes != null)
        {
            dueTimeList = _context.DueTimes.ToList();
        }
        return dueTimeList;
    }
}
