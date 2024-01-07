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
using System.ComponentModel.Design;

namespace FridgeManager
{
    class View
    {
        // Generate shorthand variables for the selected fridge and user
        protected Fridge sf = FridgeManager.SelectedFridge;
        protected User su = FridgeManager.SelectedUser;

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

            StringBuilder headerBuilder = new StringBuilder();
            headerBuilder.AppendLine($"\n  Welcome {su.Name} ({su.Role}),");
            headerBuilder.AppendLine($"  Currently viewing {sf.DisplayInfo()}");
            headerBuilder.AppendLine($"  {sf.Temperature}°C | {DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm")}");
            string header = headerBuilder.ToString();

            return UI.Selector(new string[]
            {
                    $"Items ({sf.Contents.Count()}) - View & manage items inside the fridge",
                    $"Reports ({sf.Reports.Count()}) - View & manage reports generated from the fridge",
                    "Fridge - Change fridge settings",
                    "",
                    $"Shopping List ({su.ShoppingList.Count()}) - View & manage the shopping list linked to your account",
                    $"Notifications ({su.Notifications.Count()}) - View & manage recipes linked to your account",
                    "Account - Change user account settings",
                    "Exit - Exit the fridge manager application",
            }, header);
        }

        public Fridge displayFridgeSettingsMenu()
        {
            Console.Title = "FridgeManager: Fridge Settings";
            Console.Clear();

            StringBuilder headerBuilder = new StringBuilder();
            headerBuilder.AppendLine($"\n   Currently Editing {sf.Brand}");
            headerBuilder.AppendLine($"  {sf.Model} in {sf.Room}.");
            headerBuilder.AppendLine($"  Please note: You will need to restart the program for changes to take effect.");
            string header = headerBuilder.ToString();


            Dictionary<string, string> formResponse = UI.Form(new List<FormField>(18)
            {
                new FormField { ID = 0, Name = "Powered", Label = "Powered", Value = sf.Powered },
                new FormField { ID = 1, Name = "HasFridge", Label = "Fridge", Value = sf.HasFridge},
                new FormField { ID = 2, Name = "HasFreezer", Label = "Freezer", Value = sf.HasFreezer },
                new FormField { ID = 3, Name = "HasIceMaker", Label = "Has Ice Maker", Value = sf.HasIceMaker },
                new FormField { ID = 4, Name = "HasWaterDispenser", Label = "Has Water Dispenser", Value = sf.HasWaterDispenser },
                new FormField { ID = 5, Name = "Room", Label = "Room", Value = sf.Room },
                new FormField { ID = 6, Name = "Brand", Label = "Brand", Value = sf.Brand },
                new FormField { ID = 7, Name = "Model", Label = "Model", Value = sf.Model },
                new FormField { ID = 8, Name = "Colour", Label = "Colour", Value = sf.Color },
                new FormField { ID = 9, Name = "Capacity", Label = "Capacity", Value = sf.Capacity },
                new FormField { ID = 10, Name = "Temperature", Label = "Temperature", Value = sf.Temperature },
                new FormField { ID = 11, Name = "Temperature_setting", Label = "Temperature Setting", Value = sf.TemperatureSetting },
                new FormField { ID = 12, Name = "ExpiryReportDays", Label = "[Reports] Expiry Buffer (Days)", Value = sf.ExpiryReportDays },
                new FormField { ID = 13, Name = "MaintenanceReportDays", Label = "[Reports] Maintence Reminder Frequency (Days)", Value = sf.MaintenanceReportDays },
                new FormField { ID = 14, Name = "ReportFrequencyShopping", Label = "[Reports] Shopping Report Expiry (Days)", Value = sf.ReportFrequencies["Shopping"] },
                new FormField { ID = 15, Name = "ReportFrequencyMaintenance", Label = "[Reports] Maintenance Report Expiry (Days)", Value = sf.ReportFrequencies["Maintenance"] },
                new FormField { ID = 16, Name = "ReportFrequencyExpiry", Label = "[Reports] Food Expiry Report Expiry (Days)", Value = sf.ReportFrequencies["Expiry"] },
                new FormField { ID = 17, Name = "SaveLocation", Label = "Save Location", Value = sf.SaveLocation },

            });

            if (Fridge.validateInput(formResponse))
            {
                Fridge fridge = new Fridge(formResponse, sf.UUID);

                fridge.MaintenanceDates = sf.MaintenanceDates;
                fridge.Users = sf.Users;
                fridge.Contents = sf.Contents;
                fridge.Reports = sf.Reports;

                return fridge;
            }
            else
            {
                Console.WriteLine("[i] Skipping changes, please try again...");
                Console.ReadLine();
            }

            return sf;

        }

        public User displayAccountSettingsMenu()
        {
            Console.Title = "FridgeManager: Account Settings";
            Console.Clear();

            StringBuilder headerBuilder = new StringBuilder();
            headerBuilder.AppendLine($"\n   Currently Editing {su.Name},");
            headerBuilder.AppendLine($"  Currently Editing {su.Name},");
            headerBuilder.AppendLine($"  Please note: You will need to restart the program for changes to take effect.");
            string header = headerBuilder.ToString();

            HashSet<string> roles = new HashSet<string>
            {
                "Owner",
                "User",
                "Guest"
            };


            Dictionary<string, string> formResponse = UI.Form(new List<FormField>(3) {
                    new FormField { ID = 0, Name = "Name", Label = "Name", Value = su.Name },
                    new FormField { ID = 1, Name = "Email", Label = "Email", Value = su.Email },
                    new FormField { ID = 2, Name = "Role", Label = "Role", Options = roles, Value = su.Role },
            }, header: header);

            if (User.validateInput(formResponse))
            {
                User user = new User(formResponse);

                user.ShoppingList = su.ShoppingList;
                user.Notifications = su.Notifications;

                // Return the modified user
                return user;
            }
            else
            {
                Console.WriteLine("[i] Skipping changes, please try again...");
                Console.ReadLine();
            }

            // Return the default user
            return su;
        }

    }

    public class FormField
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }
        public HashSet<string> Options { get; set; }
        public object Value { get; set; }
    }

    class UI : View
    {
        public static string Selector(string[] options, string prompt = "", bool vertical = false, bool showSplitter = true)
        {
            // Initialize the selected index to the first option
            int selectedIndex = 0;

            try
            {
                // Display options initially
                DisplayOptions(options, selectedIndex, vertical, prompt, showSplitter);

                // Continue capturing input until the Enter key is pressed
                while (true)
                {
                    // Read the key pressed by the user
                    ConsoleKeyInfo key = Console.ReadKey(true);

                    // Break the loop if the Enter key is pressed
                    if (key.Key == ConsoleKey.Enter)
                    {
                        break;
                    }

                    // Handle arrow keys to navigate through options
                    HandleArrowKeys(options.Length, ref selectedIndex, key);

                    // Redisplay options after handling input
                    DisplayOptions(options, selectedIndex, vertical, prompt, showSplitter);
                }
            }
            catch (FormatException ex)
            {
                // Handle FormatException (e.g., if there's an issue parsing user input)
                Console.WriteLine($"Format error: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                // Handle InvalidOperationException (e.g., if an operation is not valid)
                Console.WriteLine($"Invalid operation: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

            // Return the selected option
            return options[selectedIndex];
        }



        private static void DisplayOptions(string[] options, int selectedIndex, bool vertical, string prompt = "", bool showSplitter = true)
        {
            // Clear the console for a clean display
            Console.Clear();

            // Display optional prompt if provided
            if (!string.IsNullOrEmpty(prompt))
            {
                Console.WriteLine(prompt);
            }

            // Iterate through options and display them
            for (int i = 0; i < options.Length; i++)
            {
                // Initialize prefix and option variables
                string prefix;
                string currentOption = options[i];

                // Set prefix and adjust option for the selected item
                if (i == selectedIndex)
                {
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.BackgroundColor = ConsoleColor.White;

                    // Set prefix for the selected item
                    prefix = "* ";
                }
                else
                {
                    // Set default prefix
                    prefix = "  ";

                    // Modify option if the splitter is enabled
                    if (showSplitter)
                    {
                        currentOption = options[i].Split('-')[0];
                    }
                }

                // Display options either vertically or horizontally
                if (vertical)
                {
                    Console.Write($"{prefix}{currentOption}  ");
                }
                else
                {
                    Console.WriteLine($"{prefix}{currentOption}");
                }

                // Reset console colors for the next iteration
                ResetConsoleColors();
            }

            // Display additional instructions
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

        public static Dictionary<string, string> Form(List<FormField> form, string header = "")
        {
            // Dictionary to store the final result of the form
            Dictionary<string, string> formData = new Dictionary<string, string>();

            try
            {
                // Index to keep track of the currently selected item in the form
                int selectedIndex = 0;

                // Console key to capture user input
                ConsoleKeyInfo userInput;

                // Display the form options initially
                DisplayFormOptions(form, selectedIndex, header: header);

                // Continue capturing input until the user decides to exit
                while (true)
                {

                    // Read the key pressed by the user
                    userInput = Console.ReadKey(true);

                    // Handle arrow keys to navigate through form options
                    HandleArrowKeys(form.Count, ref selectedIndex, userInput);

                    // Process user input when Enter key is pressed or when other keys are pressed
                    if (userInput.Key == ConsoleKey.Enter || (userInput.Key != ConsoleKey.UpArrow && userInput.Key != ConsoleKey.DownArrow && userInput.Key != ConsoleKey.Escape))
                    {
                        ProcessFormInput(form, form.FirstOrDefault(field => field.ID == selectedIndex), ref selectedIndex);
                    }

                    // Exit the loop if the user presses the Escape key
                    if (userInput.Key == ConsoleKey.Escape)
                    {
                        break;
                    }

                    // Redisplay form options after handling input
                    DisplayFormOptions(form, selectedIndex, header: header);
                }

                // Process the inputs stored in the clonedFormFields dictionary
                ProcessFormOutput(ref form, ref formData);
            }
            catch (FormatException ex)
            {
                // Handle FormatException (e.g., if there's an issue with format)
                Console.WriteLine($"Format error: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                // Handle InvalidOperationException (e.g., if an operation is not valid)
                Console.WriteLine($"Invalid operation: {ex.Message}");
            }
            catch (KeyNotFoundException ex)
            {
                // Handle KeyNotFoundException (e.g., if a key is not found in the dictionary)
                Console.WriteLine($"Key not found: {ex.Message}");
            }
            catch (IndexOutOfRangeException ex)
            {
                // Handle IndexOutOfRangeException (e.g., if an index is out of range)
                Console.WriteLine($"Index out of range: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

            // Return the final form data
            return formData;
        }

        private static void ProcessFormOutput(ref List<FormField> form, ref Dictionary<string, string> formData)
        {
            foreach (FormField field in form)
            {

                // If the actual value is null, set it to an empty string
                if (field.Value == null)
                {
                    field.Value = "";
                }

                // Add the result to the formData dictionary
                formData.Add(field.Name, field.Value.ToString());
            }
        }


        private static void ProcessFormInput(List<FormField> form, FormField field, ref int selectedIndex)
        {
            // Handle boolean form inputs
            if (field.Value != null && field.Value.GetType() == typeof(bool))
            {
                // Toggle the boolean value
                field.Value = !(bool)field.Value;
            }

            // Handle string/int form inputs
            if (field.Options == null && (field.Value == null || field.Value.GetType() == typeof(string) || field.Value.GetType() == typeof(int)))
            {
                // Display form options with a prompt for user input
                DisplayFormOptions(form, selectedIndex, true);

                Console.WriteLine("\n[!] Type and press enter to confirm changes, leave blank and enter to revert changes.");
                Console.Write("\n" + field.Label + ": ");

                // Get user input
                string userInput = Console.ReadLine();

                // Update the value if user input is provided
                if (!string.IsNullOrEmpty(userInput))
                {
                    field.Value = userInput;
                }

                // Move to the next form field
                selectedIndex++;

                // Wrap around to the first field if at the end of the form
                if (selectedIndex == form.Count)
                {
                    selectedIndex = 0;
                }

                // Clear the console for a clean display
                Console.Clear();
            }

            // Handle string[] form inputs
            if (field.Options != null)
            {
                field.Value = Selector(field.Options.ToArray(), "", true);

                // Clear the console for a clean display
                Console.Clear();
            }
        }

        private static void DisplayFormOptions(List<FormField> form, int selectedIndex, bool typing = false, string header = "")
        {
            // Clear the console for a clean display
            Console.Clear();

            // Display optional header if provided
            if (!string.IsNullOrEmpty(header))
            {
                Console.WriteLine(header);
            }

            // Iterate through form fields and display options

            
            foreach (FormField field in form)
            {
                string prefix;

                // Set prefix and console colors based on whether the field is selected
                if (field.ID == selectedIndex)
                {
                    prefix = "* ";
                    Console.ForegroundColor = ConsoleColor.Black;

                    // Set background color for selected field during typing or regular display
                    Console.BackgroundColor = typing ? ConsoleColor.Blue : ConsoleColor.White;
                }
                else
                {
                    prefix = "  ";
                }

                // Display the field name and value
                Console.Write(prefix + field.Label + ": ");

                // Adjust console colors for boolean values
                if (field.Value != null)
                {
                    if (field.Value.GetType() == typeof(bool))
                    {
                        Console.ForegroundColor = (bool)field.Value ? ConsoleColor.Green : ConsoleColor.Red;
                    }

                    // Display the value
                    Console.WriteLine(field.Value);
                }
                else
                {
                    Console.WriteLine("");
                }

                // Reset console colors for the next iteration
                ResetConsoleColors();
            }
            
            // Display additional instructions if not in typing mode
            if (!typing)
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
