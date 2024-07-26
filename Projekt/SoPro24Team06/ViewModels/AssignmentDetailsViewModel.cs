//beginn codeownership Jan Pfluger
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SoPro24Team06.Models;

namespace SoPro24Team06.ViewModels
{
    public class AssignmentDetailsViewModel
    {
        public Assignment Assignment { get; set; }
        public string? ProcessTitle { get; set; }

        public AssignmentDetailsViewModel(Assignment assignment, Process? process)
        {
            if (process != null)
            {
                ProcessTitle = process.Title;
            }
            else
            {
                ProcessTitle = "es konnte kein zugehöriger Vorgang gefunden werden";
            }
            Assignment = assignment;
        }
    }
}
//end codeownership Jan Pfluger
