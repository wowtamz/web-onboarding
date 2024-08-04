//beginn codeownership Jan Pfluger

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
    /// <summary>
    /// Test if the FilterAssignments method from the AssignmentIndexViewModel works as intended
    /// </summary>
    /// <exception cref="Exception"></exception>
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

    /// <summary>
    /// Test if the ToAssignment Funktionalitiy from the AssignmentTemplate, creates a correct DueDate for the Assignemnt
    /// Does not thest if other Attributes of the Assignment are set correktly
    /// </summary>
    /// <exception cref="Exception"></exception>
    [Fact]
    public void DaysTillDueDateCalculation()
    {
        ApplicationUser user1 = new ApplicationUser { FullName = "user1" };
        ApplicationUser user2 = new ApplicationUser { FullName = "user2" };
        ApplicationRole role1 = new ApplicationRole { Name = "role1" };
        ApplicationRole role2 = new ApplicationRole { Name = "role2" };
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

        //assignmentTemplate Instructions where generated with https://www.blindtextgenerator.com/lorem-ipsum and https://www.textfixer.com/tools/random-string-generator.php
        List<AssignmentTemplate> testAssignments = new List<AssignmentTemplate>
        {
            new AssignmentTemplate(
                "TestMonths+",
                "BJadw*ETs*Aze@htzYp&",
                Test1,
                new List<Department>(),
                new List<Contract>(),
                Enums.AssigneeType.SUPERVISOR,
                role1,
                0
            ),
            new AssignmentTemplate(
                "TestWeeks+",
                "bgv#VJm$hvuoJmFjgUHG",
                Test2,
                new List<Department>(),
                new List<Contract>(),
                Enums.AssigneeType.ROLES,
                role2,
                0
            ),
            new AssignmentTemplate(
                "TestDays+",
                "s73o=X&fjJoCPSsXkKSb",
                Test3,
                new List<Department>(),
                new List<Contract>(),
                Enums.AssigneeType.ROLES,
                role1,
                0
            ),
            new AssignmentTemplate(
                "TestMonths-",
                "Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Aenean commodo ligula eget dolor. Aenean massa.",
                Test4,
                new List<Department>(),
                new List<Contract>(),
                Enums.AssigneeType.ROLES,
                role2,
                0
            ),
            new AssignmentTemplate(
                "TestWeeks-",
                "Cum sociis natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Donec quam felis, ultricies nec, pellentesque eu, pretium quis, sem.",
                Test5,
                new List<Department>(),
                new List<Contract>(),
                Enums.AssigneeType.ROLES,
                role1,
                0
            ),
            new AssignmentTemplate(
                "TestDays-",
                " In enim justo, rhoncus ut, imperdiet a, venenatis vitae, justo. Nullam dictum felis eu pede mollis pretium. Integer tincidunt. ",
                Test6,
                new List<Department>(),
                new List<Contract>(),
                Enums.AssigneeType.ROLES,
                role2,
                0
            ),
            new AssignmentTemplate(
                "TestCombined",
                "Quisque rutrum. Aenean imperdiet. Etiam ultricies nisi vel augue. Curabitur ullamcorper ultricies nisi. Nam eget dui. Etiam rhoncus. Maecenas tempus,",
                Test7,
                new List<Department>(),
                new List<Contract>(),
                Enums.AssigneeType.ROLES,
                role1,
                0
            ),
        };

        DateTime refDate = new DateTime(2024, 8, 1, 0, 0, 0);

        foreach (AssignmentTemplate t in testAssignments)
        {
            Assignment a1 = t.ToAssignment(null, refDate);
            Assignment a2 = t.ToAssignment(user1, refDate);
            Assignment a3 = t.ToAssignment(user2, refDate);
            // Assert that title is set correctly
            try
            {
                Assert.True(a1.Title == t.Title);
                Assert.True(a2.Title == t.Title);
                Assert.True(a3.Title == t.Title);
            }
            catch
            {
                throw new Exception("Title of Assignent not set correctly: Template:" + t.Title);
            }

            // assert that Instructions are set correctly
            try
            {
                Assert.True(a1.Instructions == t.Instructions);
                Assert.True(a2.Instructions == t.Instructions);
                Assert.True(a3.Instructions == t.Instructions);
            }
            catch
            {
                throw new Exception("Title of Assignent not set correctly: Template:" + t.Title);
            }

            //test if Assignment responsability is set correctly (AssignmeType, Assignee, AssignedRole)
            if (t.AssigneeType == Enums.AssigneeType.ROLES)
            {
                //check if Templates Assignment Type was Roles
                try
                {
                    //check for a1
                    Assert.True(a1.AssigneeType == Enums.AssigneeType.ROLES);
                    Assert.True(a1.AssignedRole == t.AssignedRole);
                    Assert.True(a1.Assignee == null);
                    //check for a2
                    Assert.True(a2.AssigneeType == Enums.AssigneeType.ROLES);
                    Assert.True(a2.AssignedRole == t.AssignedRole);
                    Assert.True(a2.Assignee == null);
                    //check for a3
                    Assert.True(a3.AssigneeType == Enums.AssigneeType.ROLES);
                    Assert.True(a3.AssignedRole == t.AssignedRole);
                    Assert.True(a3.Assignee == null);
                }
                catch
                {
                    throw new Exception("Assingment Role not set correctly");
                }
            }
            else
            {
                //check if Assingment Type was not Roles
                try
                {
                    //check for a1
                    Assert.True(a1.AssigneeType == Enums.AssigneeType.USER);
                    Assert.True(a1.AssignedRole == null);
                    Assert.True(a1.Assignee == null);
                    //check for a2
                    Assert.True(a2.AssigneeType == Enums.AssigneeType.USER);
                    Assert.True(a2.AssignedRole == null);
                    Assert.True(a2.Assignee == user1);
                    //check for a3
                    Assert.True(a3.AssigneeType == Enums.AssigneeType.USER);
                    Assert.True(a3.AssignedRole == null);
                    Assert.True(a3.Assignee == user2);
                }
                catch
                {
                    throw new Exception("Assignee was not set correctly");
                }
                break;
            }

            //chekc forDepartmentsListt
            try
            {
                if (t.ForDepartmentsList != null)
                {
                    Assert.True(a1.ForDepartmentsList == t.ForDepartmentsList);
                    Assert.True(a2.ForDepartmentsList == t.ForDepartmentsList);
                    Assert.True(a3.ForDepartmentsList == t.ForDepartmentsList);
                }
                else
                {
                    Assert.NotNull(a1.ForDepartmentsList);
                    Assert.NotNull(a2.ForDepartmentsList);
                    Assert.NotNull(a3.ForDepartmentsList);
                }
            }
            catch
            {
                throw new Exception("ForDepartmentsList was not set correctly");
            }

            //check if ForContractsList is set correctly
            try
            {
                if (t.ForContractsList != null)
                {
                    Assert.True(a1.ForContractsList == t.ForContractsList);
                    Assert.True(a2.ForContractsList == t.ForContractsList);
                    Assert.True(a3.ForContractsList == t.ForContractsList);
                }
                else
                {
                    Assert.NotNull(a1.ForContractsList);
                    Assert.NotNull(a2.ForContractsList);
                    Assert.NotNull(a3.ForContractsList);
                }
            }
            catch
            {
                throw new Exception("ForContratcsList was not set correctly");
            }

            //test if DueDate is set correctly:
            //Assert that results match that form using OnlineDateCalculator: https://www.timeanddate.com/date/dateadd.html
            int numberOfTemplatesChecked = 0;
            switch (t.Title)
            {
                case "TestMonths+":
                    try
                    {
                        Assert.True(
                            a1.DueDate.CompareTo(new DateTime(2024, 10, 1, 0, 0, 0, 0)) == 0
                        );
                        Assert.True(
                            a2.DueDate.CompareTo(new DateTime(2024, 10, 1, 0, 0, 0, 0)) == 0
                        );
                        Assert.True(
                            a3.DueDate.CompareTo(new DateTime(2024, 10, 1, 0, 0, 0, 0)) == 0
                        );
                        numberOfTemplatesChecked = +1;
                    }
                    catch
                    {
                        throw new Exception("Assignemnt Due Date was set Incorrectly: " + t.Title);
                    }
                    break;
                case "TestWeeks+":
                    try
                    {
                        Assert.True(
                            a1.DueDate.CompareTo(new DateTime(2024, 8, 22, 0, 0, 0, 0)) == 0
                        );
                        Assert.True(
                            a2.DueDate.CompareTo(new DateTime(2024, 8, 22, 0, 0, 0, 0)) == 0
                        );
                        Assert.True(
                            a3.DueDate.CompareTo(new DateTime(2024, 8, 22, 0, 0, 0, 0)) == 0
                        );
                        numberOfTemplatesChecked = +1;
                    }
                    catch
                    {
                        throw new Exception("Assignemnt Due Date was set Incorrectly: " + t.Title);
                    }
                    break;
                case "TestDays+":
                    try
                    {
                        Assert.True(
                            a1.DueDate.CompareTo(new DateTime(2024, 8, 21, 0, 0, 0, 0)) == 0
                        );
                        Assert.True(
                            a2.DueDate.CompareTo(new DateTime(2024, 8, 21, 0, 0, 0, 0)) == 0
                        );
                        Assert.True(
                            a3.DueDate.CompareTo(new DateTime(2024, 8, 21, 0, 0, 0, 0)) == 0
                        );
                        numberOfTemplatesChecked = +1;
                    }
                    catch
                    {
                        throw new Exception("Assignemnt Due Date was set Incorrectly: " + t.Title);
                    }
                    break;
                case "TestMonths-":
                    try
                    {
                        Assert.True(
                            a1.DueDate.CompareTo(new DateTime(2024, 6, 1, 0, 0, 0, 0)) == 0
                        );
                        Assert.True(
                            a2.DueDate.CompareTo(new DateTime(2024, 6, 1, 0, 0, 0, 0)) == 0
                        );
                        Assert.True(
                            a3.DueDate.CompareTo(new DateTime(2024, 6, 1, 0, 0, 0, 0)) == 0
                        );
                        numberOfTemplatesChecked = +1;
                    }
                    catch
                    {
                        throw new Exception("Assignemnt Due Date was set Incorrectly: " + t.Title);
                    }
                    break;
                case "TestWeeks-":
                    try
                    {
                        Assert.True(
                            a1.DueDate.CompareTo(new DateTime(2024, 7, 11, 0, 0, 0, 0)) == 0
                        );
                        Assert.True(
                            a2.DueDate.CompareTo(new DateTime(2024, 7, 11, 0, 0, 0, 0)) == 0
                        );
                        Assert.True(
                            a3.DueDate.CompareTo(new DateTime(2024, 7, 11, 0, 0, 0, 0)) == 0
                        );
                        numberOfTemplatesChecked = +1;
                    }
                    catch
                    {
                        throw new Exception("Assignemnt Due Date was set Incorrectly: " + t.Title);
                    }
                    break;
                case "TestDays-":
                    try
                    {
                        Assert.True(
                            a1.DueDate.CompareTo(new DateTime(2024, 7, 12, 0, 0, 0, 0)) == 0
                        );
                        Assert.True(
                            a2.DueDate.CompareTo(new DateTime(2024, 7, 12, 0, 0, 0, 0)) == 0
                        );
                        Assert.True(
                            a3.DueDate.CompareTo(new DateTime(2024, 7, 12, 0, 0, 0, 0)) == 0
                        );
                        numberOfTemplatesChecked = +1;
                    }
                    catch
                    {
                        throw new Exception("Assignemnt Due Date was set Incorrectly: " + t.Title);
                    }
                    break;
                case "TestCombined":
                    try
                    {
                        Assert.True(
                            a1.DueDate.CompareTo(new DateTime(2024, 7, 12, 0, 0, 0, 0)) == 0
                        );
                        Assert.True(
                            a2.DueDate.CompareTo(new DateTime(2024, 7, 12, 0, 0, 0, 0)) == 0
                        );
                        Assert.True(
                            a3.DueDate.CompareTo(new DateTime(2024, 7, 12, 0, 0, 0, 0)) == 0
                        );
                        numberOfTemplatesChecked = +1;
                    }
                    catch
                    {
                        throw new Exception("Assignemnt Due Date was set Incorrectly: " + t.Title);
                    }
                    break;
            }
            try
            {
                Assert.True(numberOfTemplatesChecked == testAssignments.Count);
            }
            catch
            {
                throw new Exception("Not all Templates where converted to Assignment and Checked");
            }
        }
    }
}

//end codeownership Jan Pfluger
