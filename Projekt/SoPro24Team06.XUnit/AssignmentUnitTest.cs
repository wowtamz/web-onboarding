using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using SoPro24Team06.Containers;
using SoPro24Team06.Data;
using SoPro24Team06.Models;
using SoPro24Team06.ViewModels;
using Xunit;

namespace SoPro24Team06.XUnit;

public class AssignmentUnitTest
{
    [Fact]
    public void TestFilterAssignment()
    {
        List<Process> processes = new List<Process>();
        List<Assignment> assignments = new List<Assignment>();
        try
        {
            List<Assignment> addToAssignments = new List<Assignment>
            {
                new Assignment
                {
                    Id = 1,
                    Title = "Test1",
                    DueDate = DateTime.Today,
                    AssigneeType = Enums.AssigneeType.ROLES
                },
                new Assignment
                {
                    Id = 2,
                    Title = "Test2",
                    DueDate = DateTime.Today,
                    AssigneeType = Enums.AssigneeType.ROLES
                },
                new Assignment
                {
                    Id = 3,
                    Title = "Test3",
                    DueDate = DateTime.Today,
                    AssigneeType = Enums.AssigneeType.ROLES
                }
            };
            assignments.AddRange(addToAssignments);
            List<Assignment> forProcess1 = assignments.Where(a => a.Id < 3).ToList();
            Process test1 = new Process(
                "Test1",
                "Test1",
                forProcess1,
                new ApplicationUser(),
                new ApplicationUser(),
                new Contract("Test"),
                new Department("Test")
            );
            List<Assignment> forProcess2 = assignments.Where(a => a.Id >= 3).ToList();
            Process test2 = new Process(
                "Test2",
                "Test2",
                forProcess2,
                new ApplicationUser(),
                new ApplicationUser(),
                new Contract("Test"),
                new Department("Test")
            );
            test1.Id = 1;
            test2.Id = 2;
            processes.Add(test1);
            processes.Add(test2);
        }
        catch (Exception e)
        {
            throw new Exception("Error while creating Testdata: " + e.Message);
        }

        AssignmentIndexViewModel model = new AssignmentIndexViewModel(assignments, processes);

        model.FilterAssignments(1);
        try
        {
            Assert.True(model.AssignmentList.Contains(assignments.Find(a => a.Id == 1)), "1");
            Assert.True(model.AssignmentList.Contains(assignments.Find(a => a.Id == 2)), "2");
            Assert.False(model.AssignmentList.Contains(assignments.Find(a => a.Id == 3)), "3");
        }
        catch (Exception e)
        {
            throw new Exception("Error: Filter for Process1: wrong result: " + e.Message);
        }

        model = new AssignmentIndexViewModel(assignments, processes);
        model.FilterAssignments(2);
        try
        {
            Assert.False(model.AssignmentList.Contains(assignments.Find(a => a.Id == 1)), "1");
            Assert.False(model.AssignmentList.Contains(assignments.Find(a => a.Id == 2)), "2");
            Assert.True(model.AssignmentList.Contains(assignments.Find(a => a.Id == 3)), "3");
        }
        catch (Exception e)
        {
            throw new Exception("Error: Filter for Process2: wrong result: " + e.Message);
        }
    }

    [Fact]
    public void DaysTillDueDateCalculation()
    {
        //setup:
        //create all neccecary Data
        // List<DueTime> testDueTimes = new List<DueTime>
        // {
        DueTime Test1 = new DueTime("TestMonths+", 0, 0, 2);
        DueTime Test2 = new DueTime("TestWeeks+", 0, 3, 0);
        DueTime Test3 = new DueTime("TestDays+", 20, 0, 0);
        DueTime Test4 = new DueTime("TestMonths-", 0, 0, -2);
        DueTime Test5 = new DueTime("TestWeeks-", 0, -3, 0);
        DueTime Test6 = new DueTime("TestDays-", -20, 0, 0);
        DueTime Test7 = new DueTime("TestCombined", 20, 3, -2);
        // };
        List<AssignmentTemplate> testAssignments = new List<AssignmentTemplate>
        {
            new AssignmentTemplate(
                "TestMonths+",
                "test",
                Test1,
                new List<Department>(),
                new List<Contract>(),
                Enums.AssigneeType.ROLES,
                new ApplicationRole("test"),
                0
            ),
            new AssignmentTemplate(
                "TestWeeks+",
                "test",
                Test2,
                new List<Department>(),
                new List<Contract>(),
                Enums.AssigneeType.ROLES,
                new ApplicationRole("test"),
                0
            ),
            new AssignmentTemplate(
                "TestDays+",
                "test",
                Test3,
                new List<Department>(),
                new List<Contract>(),
                Enums.AssigneeType.ROLES,
                new ApplicationRole("test"),
                0
            ),
            new AssignmentTemplate(
                "TestMonths-",
                "test",
                Test4,
                new List<Department>(),
                new List<Contract>(),
                Enums.AssigneeType.ROLES,
                new ApplicationRole("test"),
                0
            ),
            new AssignmentTemplate(
                "TestWeeks-",
                "test",
                Test5,
                new List<Department>(),
                new List<Contract>(),
                Enums.AssigneeType.ROLES,
                new ApplicationRole("test"),
                0
            ),
            new AssignmentTemplate(
                "TestDays-",
                "test",
                Test6,
                new List<Department>(),
                new List<Contract>(),
                Enums.AssigneeType.ROLES,
                new ApplicationRole("test"),
                0
            ),
            new AssignmentTemplate(
                "TestCombined",
                "test",
                Test7,
                new List<Department>(),
                new List<Contract>(),
                Enums.AssigneeType.ROLES,
                new ApplicationRole("test"),
                0
            ),
        };

        List<Assignment> assignments = new List<Assignment>();
        foreach (AssignmentTemplate t in testAssignments)
        {
            assignments.Add(t.ToAssignment(null, new DateTime(2024, 8, 1, 0, 0, 0)));
        }

        //Assert that results match that form using OnlineDateCalculator: https://www.timeanddate.com/date/dateadd.html
        //Test adding
        try
        {
            Assert.True(
                assignments
                    .Find(a => a.Title == "TestMonths+")
                    .DueDate.CompareTo(new DateTime(2024, 10, 1, 0, 0, 0, 0)) == 0
            );
        }
        catch
        {
            throw new Exception("Assert True Error: TestMonths+");
        }
        try
        {
            Assert.True(
                assignments
                    .Find(a => a.Title == "TestWeeks+")
                    .DueDate.CompareTo(new DateTime(2024, 8, 22, 0, 0, 0, 0)) == 0
            );
        }
        catch
        {
            throw new Exception("Assert True Error: TestWeeks+");
        }
        try
        {
            Assert.True(
                assignments
                    .Find(a => a.Title == "TestDays+")
                    .DueDate.CompareTo(new DateTime(2024, 8, 21, 0, 0, 0, 0)) == 0
            );
        }
        catch
        {
            throw new Exception("Assert True Error: TestDays +");
        }
        //Test Subtraktion
        try
        {
            Assert.True(
                assignments
                    .Find(a => a.Title == "TestMonths-")
                    .DueDate.CompareTo(new DateTime(2024, 6, 1, 0, 0, 0, 0)) == 0
            );
        }
        catch
        {
            throw new Exception("Assert True Error: TestMonths-");
        }
        try
        {
            Assert.True(
                assignments
                    .Find(a => a.Title == "TestWeeks-")
                    .DueDate.CompareTo(new DateTime(2024, 7, 11, 0, 0, 0, 0)) == 0
            );
        }
        catch
        {
            throw new Exception("Assert True Error: TestWeeks-");
        }
        try
        {
            Assert.True(
                assignments
                    .Find(a => a.Title == "TestDays-")
                    .DueDate.CompareTo(new DateTime(2024, 7, 12, 0, 0, 0, 0)) == 0
            );
        }
        catch
        {
            throw new Exception("Assert True Error: TestDays-");
        }
        //Test Combination
        try
        {
            Assert.True(
                assignments
                    .Find(a => a.Title == "TestCombined")
                    .DueDate.CompareTo(new DateTime(2024, 7, 12, 0, 0, 0, 0)) == 0
            );
        }
        catch
        {
            throw new Exception("Assert True Error: TestCombined");
        }
    }
}
