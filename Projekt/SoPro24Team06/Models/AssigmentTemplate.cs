using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SoPro24Team06.Models {
    public class AssigmentTemplate
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty ("title")]
        public string? Title { get; set; }
        [JsonProperty ("instructions")]
        public string? Instructions { get; set; }
        [JsonProperty ("dueTime")]
        public DueTime DueIn { get; set; }
        [JsonProperty ("forDepartments")]
        public List<Department> ForDepartmentsList { get; set; }
        [JsonProperty ("forContract")]
        public List<Contract> ForContractsList { get; set; }
        [JsonProperty ("assigneeType")]
        public AssigneeType AssigneeType { get; set; }
         [JsonProperty ("assignedRoles")]
        public List<Role>AssignedRolesList { get; set; }

    }

    public Assigment ToAssigment(AssigmentTemplate template)
    {

    }
}