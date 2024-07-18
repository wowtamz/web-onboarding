using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace SoPro24Team06.Models;

public class DueTime
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [JsonProperty("id")]
    public int Id { get; set; }

    [Required]
    [JsonProperty("label")]
    public string Label { get; set; }

    [Range(int.MinValue, int.MaxValue)]
    [JsonProperty("days")]
    public int Days { get; set; }

    [JsonProperty("weeks")]
    [Range(int.MinValue, int.MaxValue)]
    public int Weeks { get; set; }

    [JsonProperty("months")]
    [Range(int.MinValue, int.MaxValue)]
    public int Months { get; set; }

    public DueTime(string label, int days, int weeks, int months)
    {
        this.Label = label;
        this.Days = days;
        this.Weeks = weeks;
        this.Months = months;
    }

    public DueTime()
    {
        Label = string.Empty;
    }
}
