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

            string selection = UI.Selector(GetFridgeOptions(), "Available fridges:");

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

            Dictionary<string, string> formResponse = UI.Form(new Dictionary<string, object>()
            {
                { "0:Powered", new object[] { "Powered", false } },
                { "1:HasFridge", new object[] { "Fridge", false } },
                { "2:HasFreezer", new object[] { "Freezer", false } },
                { "3:HasIceMaker", new object[] { "Has Ice Maker", false } },
                { "4:HasWaterDispenser ", new object[] { "Has Water Dispenser", false } },
                { "5:Room", new object[] { "Room", "" } },
                { "6:Brand", new object[] { "Brand", "" } },
                { "7:Model", new object[] { "Model", "" } },
                { "8:Colour", new object[] { "Colour", "White" } },
                { "9:Capacity", new object[] { "Capacity (L)", 0 } },
                { "10:Temperature", new object[] { "Temperature (C)", 0 } },
                { "11:Temperature_setting", new object[] { "Temperature Setting", 0 } },
                { "12:SaveLocation", new object[] { "Save Location", folderlocation } },
            });

            Fridge fridge = new Fridge(formResponse, uuid);

            CreateDirectory(Path.Combine(path, "fridges", ("fridge-" + uuid.Split("-")[0])));

            return fridge;

        }


    }

}
