using System;


namespace SoPro24Team06.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    namespace SoPro24Team06.Models
    {
        public class DueTime
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int Id { get; set; }

            [Required]
            public string Name { get; set; }

            [Range(0, int.MaxValue)]
            public int Days { get; set; }

            [Range(0, int.MaxValue)]
            public int Weeks { get; set; }

            [Range(0, int.MaxValue)]
            public int Months { get; set; }
        }
    }
}