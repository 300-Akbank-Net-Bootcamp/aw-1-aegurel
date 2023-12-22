using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VbApi.Controllers
{
    public class Employee
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }
        
        public string Email { get; set; }
        
        public string Phone { get; set; }
        
        public double HourlySalary { get; set; }
    }

    public class EmployeeValidator : AbstractValidator<Employee>
    {
        public EmployeeValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MinimumLength(10)
                .MaximumLength(250)
                .WithMessage("Invalid Name");

            RuleFor(x => x.DateOfBirth)
                .Must(BeAValidBirthDate)
                .WithMessage("Birthdate is not valid.");

            RuleFor(x => x.Email)
                .EmailAddress()
                .When(x => !string.IsNullOrEmpty(x.Email))
                .WithMessage("Email address is not valid.");

            RuleFor(x => x.Phone)
                .Must(BeAValidPhoneNumber).When(x => !string.IsNullOrEmpty(x.Phone))
                .WithMessage("Phone is not valid.");

            RuleFor(x => x.HourlySalary)
                .InclusiveBetween(50, 400)
                .WithMessage("Hourly salary does not fall within allowed range.")
                .Must((employee, hourlySalary) => BeValidSalary(employee, hourlySalary))
                .WithMessage("Minimum hourly salary is not valid.");
        }

        private bool BeAValidBirthDate(DateTime dateOfBirth)
        {
            var minAllowedBirthDate = DateTime.Today.AddYears(-65);
            return minAllowedBirthDate <= dateOfBirth;
        }

        private bool BeValidSalary(Employee employee, double hourlySalary)
        {
            var dateBeforeThirtyYears = DateTime.Today.AddYears(-30);
            var isOlderThanThirtyYears = employee.DateOfBirth <= dateBeforeThirtyYears;

            return isOlderThanThirtyYears ? hourlySalary >= 200 : hourlySalary >= 50;
        }
        private bool BeAValidPhoneNumber(string? phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber))
                return true;

            return new PhoneAttribute().IsValid(phoneNumber);
        }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly IValidator<Employee> _validator;

        public EmployeeController(IValidator<Employee> validator)
        {
            _validator = validator;
        }

        [HttpPost]
        public IActionResult Post([FromBody] Employee value)
        {
            var validationResult = _validator.Validate(value);

            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }
            
            return Ok(value);
        }
    }
}
