using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FridgeManager.Models
{
    // This class will be used for things like validation or functions which only apply to models
    public class Model
    {
        public string UUID { get; set; }

        public bool IsLettersOnly(string value)
        {
            // Implement validation logic to check if the string contains only letters
            return value.All(char.IsLetter);
        }

        // Email validation
        public bool IsValidEmail(string email)
        {
            string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

            // Use Regex.IsMatch to check if the email matches the pattern
            return Regex.IsMatch(email, emailPattern);
        }

        // Capacity validation
        public bool IsValidCapacity(int capacity)
        {
            // Implement validation logic to check if the capacity is within a reasonable range
            return capacity > 0 && capacity <= 1000; // Adjust the range as needed
        }

    }
}
