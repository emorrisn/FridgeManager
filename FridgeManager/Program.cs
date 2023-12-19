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

            cliOptions(args);

            if (SelectedFridge == null || SelectedFridge.Users.Count() < 1)
            {
                SelectFridge selectFridge = new SelectFridge();

                SelectedFridge = selectFridge.SelectFridgeView();
            }

            if (SelectedUser == null)
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

            /// Everything is loaded and ready
            View view = new View();

            // Let's start the reporter
            Task.Run(() => Reporter.reporterAsync());

            // Now load the main menu view
            while (SelectedFridge != null && SelectedUser != null)
            {
                string choice = view.displayMainMenu();

                switch (choice)
                {
                    case string c when c.Contains("Reports"):
                        view.displayReportsMenu();
                        break;
                    case string c when c.Contains("Items"):
                        view.displayItemsMenu();
                        break;
                    case string c when c.Contains("Change fridge settings"):
                        view.displayFridgeSettingsMenu();
                        break;
                    case string c when c.Contains("Recipes"):
                        view.displayRecipesMenu();
                        break;
                    case string c when c.Contains("Shopping List"):
                        view.displayShoppingListMenu();
                        break;
                    case string c when c.Contains("Notifications"):
                        view.displayNotificationsMenu();
                        break;
                    case string c when c.Contains("Change user account settings"):
                        view.displayAccountMenu();
                        break;
                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                }
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

                // Check if -f (fridge) argument is present. e.g. prog.exe -f {/../fridge-2e63341a-e627-48ac-bb1a-9d56e2e9cc4f} 
                if (args[i] == "-f" && i + 1 < args.Length)
                {
                    FridgeFolder = args[i + 1];
                }

                // Check for -wf (working-folder) argument is present
                if (args[i] == "-wf" && i + 1 < args.Length)
                {
                    WorkingDirectory = args[i + 1];
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
            Console.WriteLine("Help unavailable at the moment");
        }
    }


}