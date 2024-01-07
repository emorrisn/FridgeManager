using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FridgeManager.Models
{
    class User: Model
    {

        private string name;

        public string Name
        {
            get => name;
            set
            {
                // Validate that the name is not null or empty and contains only letters
                if (!string.IsNullOrEmpty(value) && IsLettersOnly(value))
                {
                    name = value;
                }
                else
                {
                    // Log or throw an exception for invalid name
                    Console.WriteLine("Invalid user name. Name should contain only letters.");
                }
            }
        }

        private string email;
        public string Email
        {
            get => email;
            set
            {
                // Validate that the email is not null or empty and has a valid format
                if (!string.IsNullOrEmpty(value) && IsValidEmail(value))
                {
                    email = value;
                }
                else
                {
                    // Log or throw an exception for invalid email
                    Console.WriteLine("Invalid email address.");
                }
            }
        }
        public string Role { get; set; }
        public DateTime LastActive { get; set; }

        public List<Notification> Notifications = new List<Notification>(10);
        public List<Item> ShoppingList { get; set; } = new List<Item>(100);

        [JsonConstructor]
        public User(string name, string email, string role, string uuid = "", List<Item> shoppingList = null)
        {
            UUID = setUUID(uuid);
            Name = name;
            Email = email;
            Role = role;
            LastActive = DateTime.Now;

            if (ShoppingList != null)
            {
                ShoppingList = shoppingList;
            }
        }

        public User(Dictionary<string, string> input, string uuid = "")
        {
            UUID = setUUID(uuid);
            Name = input.GetValueOrDefault("Name", "");
            Email = input.GetValueOrDefault("Email", "");
            Role = input.GetValueOrDefault("Role", "");
            LastActive = DateTime.Now;
        }

        // Copy constructor
        public User(User other)
        {
            // Copy the properties from the existing instance
            UUID = setUUID(other.UUID);
            Name = other.Name;
            Email = other.Email;
            Role = other.Role;
            LastActive = DateTime.Now; // Assuming you want to update LastActive for the new instance

            // Copy the ShoppingList
            ShoppingList = other.ShoppingList != null ? new List<Item>(other.ShoppingList) : null;
        }

        private string setUUID(string uuid)
        {
            if (uuid == "")
            {
                return Guid.NewGuid().ToString();
            }
            else
            {
                return uuid;
            }
        }

        public static bool validateInput(Dictionary<string, string> userInput)
        {
            if (string.IsNullOrEmpty(userInput["Name"]))
            {
                Console.WriteLine("\n{!} Name cannot be empty.");
                return false;
            }

            if (string.IsNullOrEmpty(userInput["Email"]))
            {
                Console.WriteLine("\n{!} Email cannot be empty.");
                return false;
            }

            if (string.IsNullOrEmpty(userInput["Role"]))
            {
                Console.WriteLine("\n{!} Role cannot be empty.");
                return false;
            }

            return true;
        }

    }
}
