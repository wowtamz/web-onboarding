//-------------------------
// Author: Vincent Steiner
//-------------------------
using Microsoft.AspNetCore.Identity;
using SoPro24Team06.Data;
using SoPro24Team06.Enums;
using SoPro24Team06.Models;
using SoPro24Team06.Interfaces;

namespace SoPro24Team06.Containers;

public class ContractContainer : IContract
{
    private readonly ApplicationDbContext _context;
    public ContractContainer(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Contract> GetContract(string label)
    {
        if (_context.DueTimes != null)
        {
            Contract contract = _context.Contracts.FirstOrDefault(contract =>
                contract.Label == label
            );
            if (contract != null)
            {
                return contract!;
            }
        }
        return null;
    }
}
