﻿namespace ManageEmployeeWeb.Models
{
    public class EmployeeListViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public decimal Salary { get; set; }
    }

}
