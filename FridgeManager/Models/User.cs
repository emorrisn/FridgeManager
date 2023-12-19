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
    class User
    {
        public string UUID { get; set; }
        public string Name { get; set; }
        public string Password { get; set; } // Store securely (hashed and salted).
        public string Email { get; set; }
        public string Role { get; set; }
        public DateTime LastActive { get; set; }

        public List<Notification> Notifications = new List<Notification>();
        public List<Item> ShoppingList { get; set; } = new List<Item>();

        [JsonConstructor]
        public User(string name, string password, string email, string role, string uuid = "", List<Item> shoppingList = null)
        {
            UUID = setUUID(uuid);
            Name = name;
            Password = password;
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
            Password = input.GetValueOrDefault("Password", "");
            Email = input.GetValueOrDefault("Email", "");
            Role = input.GetValueOrDefault("Role", "");
            LastActive = DateTime.Now;
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

    }
}
