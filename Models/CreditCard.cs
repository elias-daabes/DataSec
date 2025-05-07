using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace DataSec.Models
{
    public class CreditCard : IValidatableObject
    {
        public int AccountId { get; set; }

        [Required(ErrorMessage = "Card number is required.")]
        [StringLength(16, MinimumLength = 16, ErrorMessage = "Card number must be 16 digits.")]
        [RegularExpression(@"^\d+$", ErrorMessage = "Card number must contain only digits.")]
        public string CardNumber { get; set; }

        [Required(ErrorMessage = "First Name is required.")]
        [StringLength(50, ErrorMessage = "First name must not exceed 50 characters.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required.")]
        [StringLength(50, ErrorMessage = "Last name must not exceed 50 characters.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "ID number is required.")]
        [StringLength(10)]
        [RegularExpression(@"^\d+$", ErrorMessage = "ID number must contain only digits.")]
        public string IDNumber { get; set; }

        [Required(ErrorMessage = "Expiration date is required.")]
        public string ValidDate { get; set; }


        [Required(ErrorMessage = "CVV is required.")]
        [RegularExpression(@"^\d{3,4}$", ErrorMessage = "CVV must be 3 or 4 digits.")]
        public string CVC { get; set; }




        // Validation logic for ExpiryDate without yield
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationResults = new List<ValidationResult>();

            if (!string.IsNullOrWhiteSpace(ValidDate))
            {
                // Validate format: MM/YY
                if (!Regex.IsMatch(ValidDate, @"^(0[1-9]|1[0-2])\/\d{2}$"))
                {
                    validationResults.Add(new ValidationResult("Invalid date format.", new[] { nameof(ValidDate) }));
                }
                else
                {
                    try
                    {
                        var dateParts = ValidDate.Split('/');
                        int month = int.Parse(dateParts[0]);
                        int year = int.Parse(dateParts[1]) + 2000; // Convert YY to YYYY

                        // Create a DateTime object for the last day of the expiration month
                        DateTime expiry = new DateTime(year, month, DateTime.DaysInMonth(year, month));

                        // Check if the expiration date is in the future
                        if (expiry < DateTime.Now)
                        {
                            validationResults.Add(new ValidationResult("Expiration date cannot be in the past.", new[] { nameof(ValidDate) }));
                        }
                    }
                    catch
                    {
                        validationResults.Add(new ValidationResult("Invalid expiration date.", new[] { nameof(ValidDate) }));
                    }
                }
            }

            return validationResults;
        }
    }
}