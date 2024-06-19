using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoPro24Team06.Models;

public class DueTime
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public string Label { get; set; }

    [Range(int.MinValue, int.MaxValue)]
    public int Days { get; set; }

    [Range(int.MinValue, int.MaxValue)]
    public int Weeks { get; set; }

    [Range(int.MinValue, int.MaxValue)]
    public int Months { get; set; }

    public DueTime(string label, int days, int weeks, int months)
    {
        this.Label = label;
        this.Days = days;
        this.Weeks = weeks;
        this.Months = months;
    }
}

