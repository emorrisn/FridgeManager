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
    class SelectFridge : View
    {

        public Fridge SelectFridgeView()
        {
            Console.Title = "FridgeManager: Select Fridge";

            CreateDirectory(Path.Combine(FridgeManager.WorkingDirectory, "fridges"));

            StringBuilder header = new StringBuilder();

            string[] headerText = {
                "\r\n",
                "  ███████ ██████  ██ ██████   ██████  ███████     ███    ███  █████  ███    ██  █████   ██████  ███████ ██████  ",
                "  ██      ██   ██ ██ ██   ██ ██       ██          ████  ████ ██   ██ ████   ██ ██   ██ ██       ██      ██   ██ ",
                "  █████   ██████  ██ ██   ██ ██   ███ █████       ██ ████ ██ ███████ ██ ██  ██ ███████ ██   ███ █████   ██████  ",
                "  ██      ██   ██ ██ ██   ██ ██    ██ ██          ██  ██  ██ ██   ██ ██  ██ ██ ██   ██ ██    ██ ██      ██   ██ ",
                "  ██      ██   ██ ██ ██████   ██████  ███████     ██      ██ ██   ██ ██   ████ ██   ██  ██████  ███████ ██   ██ ",
                "\n\nAvailable Fridges:"
            };

            foreach (string line in headerText)
            {
                header.AppendLine(line);
            }

            string selection = UI.Selector(GetFridgeOptions(), header.ToString());

            return SetupFridge(selection);
        }

        private string[] GetFridgeOptions()
        {
            return Directory.GetDirectories(FridgeManager.WorkingDirectory + "/fridges")
                .Select(Path.GetFileName)
                .Concat(new string[] { "(+) New Fridge" })
                .ToArray();
        }

        private Fridge SetupFridge(string selection)
        {
            if (selection.Contains("(+) New Fridge"))
            {
                return SetupNewFridge();
            }

            return SetupExistingFridge(selection);
        }

        public Fridge SetupExistingFridge(string selection)
        {
            Console.Title = "FridgeManager: Loading Existing Fridge";
            Console.Clear();

            string fridgeFolder = Path.Combine(FridgeManager.WorkingDirectory, "fridges", (selection));

            if (!Directory.Exists(fridgeFolder))
            {
                Console.WriteLine("Fridge doesn't exist!");
                return null;
            }

            Fridge fridge = Fridge.Load(Path.Combine(fridgeFolder, "fridge.json"));

            return fridge;
        }

        public Fridge SetupNewFridge()
        {
            Console.Title = "FridgeManager: Setup New Fridge";
            Console.Clear();

            string uuid = Guid.NewGuid().ToString();
            string path = Directory.GetCurrentDirectory();

            string folderlocation = Path.Combine(path, "fridges", ("fridge-" + uuid.Split("-")[0]), "fridge.json");

            Dictionary<string, string> formResponse = UI.Form(new List<FormField>(13)
            {
                new FormField { ID = 0, Name = "Powered", Label = "Powered", Value = false },
                new FormField { ID = 1, Name = "HasFridge", Label = "Fridge", Value = false },
                new FormField { ID = 2, Name = "HasFreezer", Label = "Freezer", Value = false },
                new FormField { ID = 3, Name = "HasIceMaker", Label = "Has Ice Maker", Value = false },
                new FormField { ID = 4, Name = "HasWaterDispenser", Label = "Has Water Dispenser", Value = false },
                new FormField { ID = 5, Name = "Room", Label = "Room" },
                new FormField { ID = 6, Name = "Brand", Label = "Brand" },
                new FormField { ID = 7, Name = "Model", Label = "Model" },
                new FormField { ID = 8, Name = "Colour", Label = "Colour", Value = "White" },
                new FormField { ID = 9, Name = "Capacity", Label = "Capacity", Value = 0 },
                new FormField { ID = 10, Name = "Temperature", Label = "Temperature", Value = 0 },
                new FormField { ID = 11, Name = "Temperature_setting", Label = "Temperature Setting", Value = 0 },
                new FormField { ID = 12, Name = "SaveLocation", Label = "Save Location", Value = folderlocation }
            });

            if (Fridge.validateInput(formResponse))
            {
                Fridge fridge = new Fridge(formResponse, uuid);

                CreateDirectory(Path.Combine(path, "fridges", ("fridge-" + uuid.Split("-")[0])));

                return fridge;
            }

            return null;

        }
    }

}
