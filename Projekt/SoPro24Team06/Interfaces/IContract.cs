//-------------------------
// Author: Vincent Steiner 
//-------------------------

using SoPro24Team06.Models;
using SoPro24Team06.Data;
using SoPro24Team06.Containers;

namespace SoPro24Team06.Interfaces
{
    public interface IContract
    {
         public Task<Contract> GetContract(string label);
    }
}