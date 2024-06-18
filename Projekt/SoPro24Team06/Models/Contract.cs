using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace SoPro24Team06.Models;

public class Contract
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [JsonProperty("id")]
    public int Id { get; set; }

    [Required(ErrorMessage = "Term is required")]
    [JsonProperty("term")]
    public string Term { get; set; }


    public Contract(string term)
    {
        this.Term = term;
    }
}