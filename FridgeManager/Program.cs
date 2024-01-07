// Needs to cover:
// Collections
// Algorithms
// CLI
// Encapsulation
// Construction
// Inheritance
// Polymorphism
// Serialisation
// Feature Binary files
// Use of Multi-processors
// Use of Profiling
// Needs to be:
// Robust
// High Performance


// TODO:
// Create classes for all possible types
// Develop a menu
// Create a flow chart which represents flow of program
// ...

using System.Diagnostics;
using System.Text;
using FridgeManager.Models;

namespace FridgeManager
{
    internal class FridgeManager
    {

        public static string ProgVersion = "1.0.0";
        public static string WorkingDirectory = Directory.GetCurrentDirectory();
        public static string FridgeFolder = "";

        public static Fridge SelectedFridge;
        public static User SelectedUser;

        static void Main(string[] args)
        {

            try
            {
                cliOptions(args);

                InitializeFridge();
                InitializeUser();

                /// Everything is loaded and ready, let's load the views.
                View view = new View();
                DisplayReports reports = new DisplayReports();
                DisplayItems items = new DisplayItems();
                DisplayShoppingList shoppingList = new DisplayShoppingList();
                DisplayNotifications notifications = new DisplayNotifications();

                // Let's start the reporter
                Task.Run(() => Reporter.reporterAsync());

                // Now load the main menu view
                while (SelectedFridge != null && SelectedUser != null)
                {
                    string choice = view.displayMainMenu();

                    switch (choice)
                    {
                        case string c when c.Contains("Reports"):
                            reports.displayReportsMenu();
                            break;
                        case string c when c.Contains("Items"):
                            items.displayItemsMenu();
                            break;
                        case string c when c.Contains("Change fridge settings"):
                            SelectedFridge = view.displayFridgeSettingsMenu();
                            SelectedFridge.Save(SelectedFridge.SaveLocation);
                            break;
                        case string c when c.Contains("Shopping List"):
                            shoppingList.displayShoppingListMenu();
                            break;
                        case string c when c.Contains("Notifications"):
                            notifications.displayNotificationsMenu();
                            break;
                        case string c when c.Contains("Change user account settings"):
                            SelectedFridge.Users.Remove(SelectedUser);
                            SelectedUser = view.displayAccountSettingsMenu();
                            SelectedFridge.Users.Add(SelectedUser);
                            SelectedFridge.Save(SelectedFridge.SaveLocation);
                            break;
                        case string c when c.Contains("Exit"):
                            SelectedFridge = null;
                            SelectedUser = null;
                            break;
                        default:
                            Console.ForegroundColor = ConsoleColor.Red;
                            break;
                    }
                }
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"[!] Argument error: {ex.Message}");
            }
            catch (IOException ex)
            {
                Console.WriteLine($"[!] I/O error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[!] An unexpected error occurred: {ex.Message}");
            }
        }

        static void cliOptions(string[] args)
        {
            // check if fridge folder is in CLI args
            for (int i = 0; i < args.Length; i++)
            {
                // Check for version 
                if (args[i] == "-v")
                {
                    Console.WriteLine("Version: " + ProgVersion);
                }

                // Check for help 
                if (args[i] == "-h")
                {
                    cliHelp();
                }

                string workingDirectory = WorkingDirectory;

                // Check if -wd (working directory) argument is present, overwrite current working directory
                if (args[i] == "-wd" && i + 1 < args.Length)
                {
                    workingDirectory = args[i + 1];
                }

                // Check if -f (fridge) argument is present
                if (args[i] == "-f" && i + 1 < args.Length)
                {
                    string fridgeName = args[i + 1];

                    // Check if fridge exists

                    string fridgeFolder = Path.Combine(workingDirectory, "fridges", (fridgeName));

                    if (!Directory.Exists(fridgeFolder))
                    {
                        Console.WriteLine("[!] No fridge exists or is selected, exiting...");
                        Console.ReadLine();
                        System.Environment.Exit(0);

                    } else
                    {
                        Fridge fridge = Fridge.Load(Path.Combine(fridgeFolder, "fridge.json"));

                        SelectedFridge = fridge;

                        Console.WriteLine("[i] Fridge selected: " + SelectedFridge.UUID);
                    }
                }

                // Check if -u (user) argument is present
                if (args[i] == "-u" && i + 1 < args.Length)
                {
                    string userName = args[i + 1];

                    if(SelectedFridge != null)
                    {

                        User user = SelectedFridge.Users.FirstOrDefault(u => u.Name == userName);

                        if (user != null)
                        {
                            SelectedUser = user;
                            Console.WriteLine("[i] User selected: " + SelectedUser.Name);
                        } else
                        {
                            Console.WriteLine("[!] No user exists or is selected, exiting...");
                            Console.ReadLine();
                            System.Environment.Exit(0);
                        }
                    } else
                    {
                        Console.WriteLine("[!] No fridge exists or is selected, please select one before choosing a user. Exiting...");
                        Console.ReadLine();
                        System.Environment.Exit(0);
                    }
                }

            }
        }

        private static void InitializeFridge()
        {
            if (SelectedFridge == null || SelectedFridge.Users.Count() < 1)
            {
                SelectFridge selectFridge = new SelectFridge();

                SelectedFridge = selectFridge.SelectFridgeView();

            }
        }

        private static void InitializeUser()
        {
            if (SelectedUser == null && SelectedFridge != null)
            {
                SelectUser selectUser = new SelectUser();

                if (SelectedFridge.Users == null || SelectedFridge.Users.Count() < 1)
                {

                    // Setup users for fridge
                    SelectedFridge.Users = selectUser.setupNewFridgeUsers();

                    // Save new users to fridge
                    SelectedFridge.Save(SelectedFridge.SaveLocation);
                }

                SelectedUser = selectUser.selectFridgeUser(SelectedFridge.Users);
            }
            else
            {
                if(SelectedFridge == null)
                {
                    Console.WriteLine("[!] No fridge selected, exiting...");
                    Console.ReadLine();
                    System.Environment.Exit(0);
                }
            }
        }

        public static string setUUID(string uuid)
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

        static void cliHelp()
        {
            Console.WriteLine("Commands: \n-h help\n-f fridge\n-wf working folder");

            StringBuilder headerBuilder = new StringBuilder();
            headerBuilder.AppendLine($"\nWelcome to Help!");
            headerBuilder.AppendLine($"General Commands: ");
            headerBuilder.AppendLine($"  -h  Help Command");
            headerBuilder.AppendLine($"  -v  Display version");
            headerBuilder.AppendLine($"  -wd  Select the working directory (location of the folder containing the /fridges/ folder)");
            headerBuilder.AppendLine($"Authentication Commands: ");
            headerBuilder.AppendLine($"  -f (fridge name) Select a fridge using a fridge's name.");
            headerBuilder.AppendLine($"  -u (user name) Select a user using a user's nam.e");
            headerBuilder.AppendLine($"  Usuage Example: ");
            headerBuilder.AppendLine($"    program.exe -f fridge-20c711c1 -u Ethan");
            headerBuilder.AppendLine($"    Expected Output: ");
            headerBuilder.AppendLine($"      [i] Fridge selected: 20c711c1-c0b1-49fc-b0c6-a56e53a4968c ");
            headerBuilder.AppendLine($"      [i] User selected: Ethan ");



            string header = headerBuilder.ToString();

            Console.WriteLine(header);
            System.Environment.Exit(0);
        }
    }


}