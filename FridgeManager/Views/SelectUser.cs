using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json.Serialization;
using System.ComponentModel;
using System.Net.Http.Json;
using System.Xml;
using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;
using System.ComponentModel.DataAnnotations;
using System.Reflection.PortableExecutable;
using FridgeManager.Models;
using System.Reflection.Emit;

namespace FridgeManager
{
    class SelectUser : View
    {
        public List<User> setupNewFridgeUsers(bool skipDialogue = false)
        {
            Console.Title = "FridgeManager: Setup New Users For Your Fridge";
            Console.Clear();

            Fridge fridge = FridgeManager.SelectedFridge;
            List<User> newUsers = new List<User>(5);

            if (skipDialogue == false)
            {
                if (UI.Selector(new string[] { "Yes", "No (Save & Exit)" }, "It appears the selected fridge has no users, would you like to add some?", true) == "No (Save & Exit)")
                {
                    fridge.Save(fridge.SaveLocation);
                    System.Environment.Exit(0);
                }
            }

            bool createUser = true;

            while (createUser == true)
            {
                // Create user here
                Console.Title = "FridgeManager: Setup New User (" + (FridgeManager.SelectedFridge.Users.Count + 1) + ") for your Fridge";
                Console.Clear();

                HashSet<string> roles = new HashSet<string>();

                roles.Add("Owner");
                roles.Add("User");
                roles.Add("Guest");

                Dictionary<string, string> formResponse = UI.Form(new List<FormField>(3) {
                    new FormField { ID = 0, Name = "Name", Label = "Name" },
                    new FormField { ID = 1, Name = "Email", Label = "Email" },
                    new FormField { ID = 2, Name = "Role", Label = "Role", Options = roles },
                });

                if (User.validateInput(formResponse))
                {
                    User user = new User(formResponse);
                    newUsers.Add(user);
                } else
                {
                    Console.WriteLine("[i] Skipping user, please try again...");
                    Console.ReadLine();
                }

                string selection = UI.Selector(new string[] { "Yes", "No" }, "Continue adding users?", true);

                if (selection == "No")
                {
                    createUser = false;
                }
            }

            return newUsers;
        }

        public User selectFridgeUser(List<User> users)
        {
            bool selecting = true;
            User user = users.First(); // Default to first user

            while (selecting == true)
            {
                Console.Title = "FridgeManager: Select User";
                Console.Clear();

                string selection = UI.Selector(users.Select(user => user.Name).Concat(new string[] { "(+) New User" }).ToArray(), "Available users:");

                if (selection.Contains("(+) New User"))
                {
                    FridgeManager.SelectedFridge.Users.AddRange(setupNewFridgeUsers(true));
                    FridgeManager.SelectedFridge.Save(FridgeManager.SelectedFridge.SaveLocation);
                }
                else
                {
                    selecting = false;
                    user = users.FirstOrDefault(user => user.Name == selection);
                }
            }

            return user;
        }

    }

}
