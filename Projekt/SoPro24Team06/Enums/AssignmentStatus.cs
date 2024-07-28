using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SoPro24Team06.Enums
{
    public enum AssignmentStatus
    {
        [Display(Name = "noch zu Erledigen")]
        NOT_STARTED,

        [Display(Name = "In Bearbeitung")]
        IN_PROGRESS,

        [Display(Name = "Fertig")]
        DONE
    }
}
