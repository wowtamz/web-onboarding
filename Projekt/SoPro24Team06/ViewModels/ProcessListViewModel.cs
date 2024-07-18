//-------------------------
// Author: Tamas Varadi
//-------------------------

using SoPro24Team06.Models;

namespace SoPro24Team06.ViewModels;

public class ProcessListViewModel
{
    public List<Process> ProcessList { get; set; }

    public ProcessListViewModel(List<Process> processList)
    {
        this.ProcessList = processList;
    }
}
