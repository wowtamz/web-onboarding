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
        public Task AddDueTime(DueTime dueIn);
        public Task<DueTime> GetDueTime(string label);
        public Task DeleteDueTime(int id);
        public Task<List<DueTime>> GetAllDueTimes();
    }
}