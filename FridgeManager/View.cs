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

namespace FridgeManager
{
    class View
    {

        protected bool CreateDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);

                return true;
            }

            return false;
        }

        public string displayMainMenu()
        {
            Console.Title = "FridgeManager: Main Menu";
            Console.Clear();

            Fridge sf = FridgeManager.SelectedFridge;
            User su = FridgeManager.SelectedUser;

            string header = @$"
  Welcome {su.Name} ({su.Role}),
  Currently viewing {sf.Model} in {sf.Room}
  {sf.Temperature}°C | {DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm")}
  ";
            return UI.Selector(new string[]
            {
                    $"Items ({sf.Contents.Count()}) - View & manage items inside the fridge",
                    $"Reports ({sf.Reports.Count()}) - View & manage reports generated from the fridge",
                    "Fridge - Change fridge settings",
                    "",
                    $"Shopping List ({su.ShoppingList.Count()}) - View & manage the shopping list linked to your account",
                    $"Notifications ({su.Notifications.Count()}) - View & manage recipes linked to your account",
                    "Account - Change user account settings",
            }, header);
        }

        public void displayReportsMenu()
        {
            Console.Title = "FridgeManager: Reports";
            Console.Clear();

            Fridge sf = FridgeManager.SelectedFridge;
            User su = FridgeManager.SelectedUser;

            string header = @$"
  Currently viewing reports
  Listing: {sf.Reports.Count()}
  ";

            List<string> reportStrings = new List<string>();

            reportStrings.Add("Go Back");
            reportStrings.Add("Clear All");
            reportStrings.Add("");

            reportStrings.AddRange(sf.Reports.Select(report => $"{report.DateGenerated.ToString()} {report.ReportType} - {report.UUID}").ToList());

            string selection = UI.Selector(reportStrings.ToArray(), header);

            switch (selection)
            {
                case string c when c.Contains("Clear All"):
                    sf.Reports.Clear();
                    sf.Save(sf.SaveLocation);
                    displayReportsMenu();
                    break;
                case string c when c.Contains("Report"):
                    int indexOfDash = selection.IndexOf('-');
                    string uuid = selection.Substring(indexOfDash + 1).Trim();
                    Report report = sf.Reports.FirstOrDefault(r => r.UUID == uuid);
                    displayReportDetailsMenu(report);
                    sf.Save(sf.SaveLocation);
                    displayReportsMenu();
                    break;
                case "":
                    displayReportsMenu();
                    break;
                default:
                    break;
            }
        }

        public void displayReportDetailsMenu(Report report)
        {
            Console.Title = "FridgeManager: Report " + report.UUID;
            Console.Clear();

            List<string> detailsStrings = new List<string>();

            if (report.ReportType == "Maintenance Report")
            {
                detailsStrings.Add("Mark as Complete");
            }

            detailsStrings.Add("Go Back");
            detailsStrings.Add("Delete");

            StringBuilder foodItems = new StringBuilder("");

            if (report.FoodItems != null)
            {
                foodItems = new StringBuilder("Foods:\n");

                foreach (Item item in report.FoodItems)
                {
                    if(report.ReportType == "Expiry Report")
                    {
                        var timeLeft = item.ExpirationDate - DateTime.Now;

                        foodItems.AppendLine($"  * {item.Name} ({item.Quantity}) - Expires: {item.ExpirationDate} ({(int)timeLeft.TotalDays} days left)");
                    }else if(report.ReportType == "Shopping Report")
                    {
                        foodItems.AppendLine($"  * {item.Name} ({item.Quantity} left)");
                    } else
                    {
                        foodItems.AppendLine($"  * {item.Name} ({item.Quantity}) - {item.Barcode} : {item.Category.Name}");
                    }
                }
            }

            StringBuilder items = new StringBuilder("");

            if (report.Items != null)
            {
                items = new StringBuilder("Items:\n");

                foreach (string item in report.Items)
                {
                    if (report.ReportType == "Maintenance Report")
                    {
                        DateTime completed = FridgeManager.SelectedFridge.MaintenanceDates[item];
                        TimeSpan timeSinceCompletion = DateTime.Now - completed;

                        items.AppendLine($"  * {item} was last completed: {completed} - This was {timeSinceCompletion.Days} days ago");
                    } else
                    {
                        items.AppendLine("  * " + item);
                    }
                }
            }

            string header = @$"
  Report: {report.UUID} ({report.ReportType})
  {report.Message}
  {items}{foodItems}
  ";

            string selection = UI.Selector(detailsStrings.ToArray(), header);

            switch (selection)
            {
                case "Delete":
                    FridgeManager.SelectedFridge.Reports.Remove(report);
                    break;
                case "Mark as Complete":
                    markMaintenanceItemsAsComplete(report.Items);
                    FridgeManager.SelectedFridge.Reports.Remove(report);
                    break;
                default:
                    break;
            }
        }

        private void markMaintenanceItemsAsComplete(List<string> Items)
        {
            foreach (string item in Items)
            {
                FridgeManager.SelectedFridge.MaintenanceDates[item] = DateTime.Now;
            }
        }

        public void displayItemsMenu()
        {
            Console.Title = "FridgeManager: Items";
            Console.Clear();
            Console.ReadLine();
            // TODO: ...
        }

        public void displayFridgeSettingsMenu()
        {
            Console.Title = "FridgeManager: Fridge Settings";
            Console.Clear();
            Console.ReadLine();
            // TODO: ...
        }

        public void displayRecipesMenu()
        {
            Console.Title = "FridgeManager: Recipes";
            Console.Clear();
            Console.ReadLine();
            // TODO: ...
        }
        public void displayShoppingListMenu()
        {
            Console.Title = "FridgeManager: Shopping List";
            Console.Clear();
            Console.ReadLine();

            // TODO: ...
        }
        public void displayNotificationsMenu()
        {
            Console.Title = "FridgeManager: Notifications";
            Console.Clear();
            Console.ReadLine();

            // TODO: ...
        }
        public void displayAccountMenu()
        {
            Console.Title = "FridgeManager: Account";
            Console.Clear();
            Console.ReadLine();

            // TODO: ...
        }

    }

    class UI : View
    {

        public static string Selector(string[] options, string prompt = "", bool vertical = false, bool splitter = true)
        {
            int selectedIndex = 0;

            DisplayOptions(options, selectedIndex, vertical, prompt, splitter);

            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);

                if (key.Key == ConsoleKey.Enter)
                {
                    break;
                }

                HandleArrowKeys(options.Length, ref selectedIndex, key);
                DisplayOptions(options, selectedIndex, vertical, prompt, splitter);
            }

            return options[selectedIndex];
        }

        private static void DisplayOptions(string[] options, int selectedIndex, bool vertical, string prompt = "", bool splitter = true)
        {
            Console.Clear();

            if (!string.IsNullOrEmpty(prompt))
            {
                Console.WriteLine(prompt);
            }

            for (int i = 0; i < options.Length; i++)
            {
                string prefix;
                string option = options[i];

                if (i == selectedIndex)
                {
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.BackgroundColor = ConsoleColor.White;

                    prefix = "* ";
                }
                else
                {
                    prefix = "  ";

                    if (splitter == true)
                    {
                        option = options[i].Split('-')[0];
                    }
                }

                if (vertical)
                {
                    Console.Write($"{prefix}{option}  ");
                }
                else
                {
                    Console.WriteLine($"{prefix}{option}");
                }

                ResetConsoleColors();
            }

            Console.WriteLine("\n[i] Use the arrow keys to move and select to enter.");
        }

        private static void HandleArrowKeys(int optionsLength, ref int selectedIndex, ConsoleKeyInfo key)
        {
            if (key.Key == ConsoleKey.DownArrow || key.Key == ConsoleKey.RightArrow)
            {
                selectedIndex = (selectedIndex + 1) % optionsLength;
            }
            else if (key.Key == ConsoleKey.UpArrow || key.Key == ConsoleKey.LeftArrow)
            {
                selectedIndex = (selectedIndex - 1 + optionsLength) % optionsLength;
            }
        }

        public static Dictionary<string, string> Form(Dictionary<string, object> input)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            Dictionary<string, object> formFields = input.ToDictionary(entry => entry.Key, entry => entry.Value);

            int selectedIndex = 0;

            ConsoleKeyInfo key;

            DisplayFormOptions(formFields, selectedIndex);

            while (true)
            {
                key = Console.ReadKey(true);

                HandleArrowKeys(formFields.Values.Count, ref selectedIndex, key);

                if (key.Key == ConsoleKey.Enter || key.Key != ConsoleKey.UpArrow && key.Key != ConsoleKey.DownArrow && key.Key != ConsoleKey.Escape)
                {
                    ProcessFormInput(formFields, ref selectedIndex);
                }

                if (key.Key == ConsoleKey.Escape)
                {
                    break;
                }

                DisplayFormOptions(formFields, selectedIndex);
            }

            // Process the inputs stored from the formFields

            foreach (KeyValuePair<string, object> item in formFields)
            {
                string _key = item.Key.ToString().Split(":")[1];

                object[] _value = (object[])item.Value;

                object? actualValue = _value[1];

                if (actualValue.GetType() == typeof(string[]))
                {
                    string[] _actualValue = (string[])actualValue;
                    actualValue = _actualValue[_actualValue.Length - 1];
                }

                if (_value[1] == null)
                {
                    actualValue = "";
                }

                result.Add(_key, actualValue.ToString());
            }

            return result;
        }

        private static void ProcessFormInput(Dictionary<string, object> formFields, ref int selectedIndex)
        {
            KeyValuePair<string, object> selectedKvp = formFields.ElementAt(selectedIndex);

            object[] selectedValue = (object[])selectedKvp.Value;

            // Handle boolean form inputs
            if (selectedValue[1].GetType() == typeof(bool))
            {
                selectedValue[1] = !(bool)selectedValue[1]; // Toggle the boolean value
            }

            // Handle string/int form inputs
            if (selectedValue[1].GetType() == typeof(string) || selectedValue[1].GetType() == typeof(int))
            {
                DisplayFormOptions(formFields, selectedIndex, true);

                Console.WriteLine("\n[!] Type and press enter to confirm changes, leave blank and enter to revert changes.");
                Console.Write("\n" + selectedValue[0] + ": ");

                string userInput = Console.ReadLine();

                if (userInput != "")
                {
                    selectedValue[1] = userInput;
                }

                selectedIndex++;

                if (selectedIndex == formFields.Values.Count)
                {
                    selectedIndex = 0;
                }

                Console.Clear();
            }

            // Handle string[] form inputs
            if (selectedValue[1].GetType() == typeof(string[]))
            {
                string[] options = (string[])selectedValue[1];
                List<string> optionsList = new List<string>(options);

                optionsList.Add(Selector(optionsList.Distinct().ToArray(), "", true));
                options = optionsList.ToArray();

                selectedValue[1] = options;

                Console.Clear();
            }
        }

        private static void DisplayFormOptions(Dictionary<string, object> input, int selectedIndex, bool typing = false)
        {
            Console.Clear();
            foreach (object i in input)
            {
                KeyValuePair<string, object> kvp = (KeyValuePair<string, object>)i;

                string name = kvp.Key.Split(":")[1]; // name that will be saved

                object[] value = (object[])kvp.Value;

                int index = Convert.ToInt32(kvp.Key.Split(":")[0]);

                string prefix;

                if (index == selectedIndex)
                {
                    prefix = "* ";
                    Console.ForegroundColor = ConsoleColor.Black;
                    if (typing == true)
                    {
                        Console.BackgroundColor = ConsoleColor.Blue;
                    }
                    else
                    {
                        Console.BackgroundColor = ConsoleColor.White;
                    }
                }
                else
                {
                    prefix = "  ";
                }

                Console.Write(prefix + value[0].ToString() + ": ");

                if (value[1].GetType() == typeof(bool))
                {
                    if ((bool)value[1] == false)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                    }
                }

                if (value[1].GetType() == typeof(string[]))
                {
                    string[] dispVal = (string[])value[1];
                    Console.WriteLine(dispVal[dispVal.Length - 1].ToString());
                }
                else
                {
                    Console.WriteLine(value[1]);
                }

                ResetConsoleColors();
            }

            if (typing == false)
            {
                Console.WriteLine("\n[i] Press enter to select choice.");
                Console.WriteLine("[i] Press ESC to save and continue.");
            }
        }

        private static void ResetConsoleColors()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;
        }
    }
}
