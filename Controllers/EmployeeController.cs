using ManageEmployeeWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace ManageEmployeeWeb.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly string logPath = "logs.txt";

        public EmployeeController(AppDbContext context)
        {
            _context = context;
        }

        private void LogError(string message, Exception ex)
        {
            var error = new StringBuilder();
            error.AppendLine($"[{DateTime.Now}] {message}");
            error.AppendLine($"Exception: {ex.Message}");
            error.AppendLine(ex.StackTrace);
            error.AppendLine(new string('-', 50));
            System.IO.File.AppendAllText(logPath, error.ToString());
        }

        public IActionResult Index(string search, int page = 1)
        {
            int pageSize = 15;

            var employeesQuery = _context.Employees
                .Include(e => e.Salaries)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                employeesQuery = employeesQuery.Where(e =>
                    EF.Functions.Like(e.Name, $"%{search}%") ||
                    e.Salaries.Any(s => EF.Functions.Like(s.Title, $"%{search}%"))
                );
            }

            int totalRecords = employeesQuery.Count();

            var result = employeesQuery
                .OrderByDescending(e => e.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new EmployeeListViewModel
                {
                    Name = e.Name,
                    Title = e.Salaries
                        .OrderByDescending(s => s.FromDate)
                        .Select(s => s.Title)
                        .FirstOrDefault() ?? "N/A",
                    Salary = e.Salaries
                        .OrderByDescending(s => s.FromDate)
                        .Select(s => s.Salary)
                        .FirstOrDefault()
                })
                .ToList();

            ViewBag.TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
            ViewBag.PageSize = pageSize;
            ViewBag.CurrentPage = page;
            ViewBag.Search = search;

            return View(result);
        }



        public IActionResult Titles()
        {
            try
            {
                var titles = _context.EmployeeSalaries
                    .AsEnumerable()
                    .GroupBy(s => s.Title)
                    .Select(g => new TitleViewModel
                    {
                        Title = g.Key,
                        MinSalary = g.Min(s => s.Salary),
                        MaxSalary = g.Max(s => s.Salary)
                    })
                    .ToList();

                return View(titles);
            }
            catch (Exception ex)
            {
                LogError("Error in Titles action", ex);
                ViewBag.Error = "Unable to load title list.";
                return View(new List<TitleViewModel>());
            }
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(AddEmployeeViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (model.JoinDate <= model.DOB)
            {
                ModelState.AddModelError("JoinDate", "Join Date must be after Date of Birth.");
                return View(model);
            }

            try
            {
                var employee = new Employee
                {
                    Name = model.Name,
                    SSN = model.SSN,
                    DOB = model.DOB,
                    JoinDate = model.JoinDate,
                    Phone = model.Phone,
                    Address = model.Address,
                    City = model.City,
                    Zip = model.Zip,
                    State = model.State,
                };

                _context.Employees.Add(employee);
                await _context.SaveChangesAsync();

                var salary = new EmployeeSalary
                {
                    EmployeeId = employee.Id,
                    Title = model.Title,
                    Salary = model.Salary,
                    FromDate = model.JoinDate
                };

                _context.EmployeeSalaries.Add(salary);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                LogError("Error in Add POST action", ex);
                ViewBag.Error = "Unable to save employee. Please try again.";
                return View(model);
            }
        }
    }
}
