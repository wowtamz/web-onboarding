using System.ComponentModel.DataAnnotations;

namespace SoPro24Team06.Enums
{
    public enum AssigneeType
    {
        [Display(Name = "Rolle")]
        ROLES,

        [Display(Name = "Vorgangsverantwortlicher")]
        SUPERVISOR,

        [Display(Name = "Bezugsperson")]
        WORKER_OF_REF,

        [Display(Name = "Benutzer")]
        USER,
    }
}
