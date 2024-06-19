namespace SoPro24Team06.Models
{
    public class Department
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public Department(string name)
        {
            Name = name;
        }
    }
}