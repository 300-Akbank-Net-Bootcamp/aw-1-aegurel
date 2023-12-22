using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace VbApi.Controllers
{
    public class Staff
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public decimal? HourlySalary { get; set; }
    }

    public class StaffValidator : AbstractValidator<Staff>
    {
        public StaffValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .Length(10, 250).WithMessage("Name length must be between 10 and 250 characters.");

            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("Email address is not valid.");

            RuleFor(x => x.Phone)
                .Must(BeAValidPhoneNumber).When(x => !string.IsNullOrEmpty(x.Phone))
                .WithMessage("Phone is not valid.");

            RuleFor(x => x.HourlySalary)
                .NotNull().WithMessage("Hourly salary is required.")
                .InclusiveBetween(30, 400).WithMessage("Hourly salary must be between 30 and 400.");
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
    public class StaffController : ControllerBase
    {
        private readonly IValidator<Staff> _validator;

        public StaffController(IValidator<Staff> validator)
        {
            _validator = validator;
        }

        [HttpPost]
        public IActionResult Post([FromBody] Staff value)
        {
            var validationResult = _validator.Validate(value);

            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            // Eğer doğrulama başarılı ise burada gerekli işlemleri yapabilirsiniz.
            return Ok(value);
        }
    }
}
