//-------------------------
// Author: Vincent Steiner 
//-------------------------
using SoPro24Team06.Models;
using SoPro24Team06.Data;
using SoPro24Team06.Containers;

namespace SoPro24Team06.Interfaces
{
    public interface IDueTime
    {
        public void AddDueTime(DueTime dueIn);
        public DueTime GetDueTime(string label);
        public void DeleteDueTime(int id);
        public List<DueTime> GetAllDueTimes();
    }
}