using Bogus;
using ManageEmployeeWeb.Models;

namespace ManageEmployeeWeb.Seeders
{
    public static class DataSeeder
    {
        public static void SeedData(AppDbContext context)
        {
            try
            {
                if (context.Employees.Any())
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Data already seeded.");
                    Console.ResetColor();
                    return;
                }

                var faker = new Faker("en");
                var titles = new[] { "Developer", "Manager", "QA", "DevOps Engineer", "Business Analyst", "BDM", "HR" };
                var rnd = new Random();
                var employees = new List<Employee>();

                for (int i = 0; i < 100; i++)
                {
                    var dob = faker.Date.Past(rnd.Next(22, 65), DateTime.Today.AddYears(-22));
                    var joinDate = faker.Date.Between(dob.AddYears(22), DateTime.Today);

                    var employee = new Employee
                    {
                        Name = faker.Name.FullName(),
                        SSN = faker.Random.Replace("###-##-####"),
                        DOB = dob,
                        JoinDate = joinDate,
                        Phone = faker.Phone.PhoneNumber("##########"),
                        Address = faker.Address.StreetAddress(),
                        City = faker.Address.City(),
                        Zip = faker.Address.ZipCode(),
                        State = faker.Address.StateAbbr(),                        
                    };

                    var salary = new EmployeeSalary
                    {
                        Title = faker.PickRandom(titles),
                        Salary = faker.Random.Decimal(30000, 120000),
                        FromDate = joinDate
                    };

                    employee.Salaries.Add(salary);
                    employees.Add(employee);
                }

                context.Employees.AddRange(employees);
                context.SaveChanges();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Seeded 100 employees with salaries.");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                var error = $"[{DateTime.Now}] Data Seeding Error: {ex.Message}\n{ex.StackTrace}\n";
                File.AppendAllText("logs.txt", error);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Failed to seed data. See logs.txt for details.");
                Console.ResetColor();
            }
        }
    }
}
