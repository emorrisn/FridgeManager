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
using System.Text.Json;
using JsonConstructorAttribute = System.Text.Json.Serialization.JsonConstructorAttribute;
using JsonIgnoreAttribute = System.Text.Json.Serialization.JsonIgnoreAttribute;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace FridgeManager.Models
{
    class Fridge
    {
        // Attributes
        public string UUID { get; set; }
        public bool Powered { get; set; }
        public bool HasFridge { get; set; }
        public bool HasFreezer { get; set; }
        public string Room { get; set; }
        public string Color { get; set; }
        public string Model { get; set; }
        public bool HasIceMaker { get; set; }
        public bool HasWaterDispenser { get; set; }

        public List<Report> Reports { get; set; } = new List<Report>();

        private int _temperature;
        public int Temperature
        {
            get { return _temperature; }
            set
            {
                if (value >= -10 && value <= 10) // Validation
                {
                    _temperature = value;
                }
                else
                {
                    Console.WriteLine("[!] Invalid temperature value");
                }
            }
        }
        public int Capacity { get; set; }
        public int TemperatureSetting { get; set; }
        // public List<Item> Contents { get; } = new List<Item>();

        // Testing data:
        public List<Item> Contents = new List<Item>
                    {
                        new Item { Name = "Milk", Quantity = 1, ExpirationDate = DateTime.Now.AddDays(2), Category = new ItemCategory { Name = "Dairy" } },
                        new Item { Name = "Eggs", Quantity = 12, ExpirationDate = DateTime.Now.AddDays(5), Category = new ItemCategory { Name = "Produce" } },
                    };

        public List<User> Users { get; set; } = new List<User>();
        public string SaveLocation { get; set; }
        
        // Amount of days until an expiry warning
        public int ExpiryReportDays { get; set; } = 3;

        // Frequency of report items (e.g. each maintenance item needs to be 30 days apart)
        public int MaintenanceReportDays { get; set; } = 30;

        public Dictionary<string, int> ReportFrequencies { get; set; } = new Dictionary<string, int>
        {
            { "Shopping", 1 },
            { "Maintenance", 7 },
            { "Expiry", 7 }
        };

        public Dictionary<string, DateTime> MaintenanceDates { get; set; } = new Dictionary<string, DateTime>
        {
            { "Cleaned", DateTime.Now },
            { "Defrosted", DateTime.Now },
            { "Filter Change", DateTime.Now },
            { "Condenser Coil Cleaned", DateTime.Now },
            { "Compressor Check", DateTime.Now },
            { "DoorSeal Check", DateTime.Now },
            { "LightBulb Change", DateTime.Now },
            { "Temperature Calibration", DateTime.Now },
            { "FanCheck", DateTime.Now },
            { "Water Filter Change", DateTime.Now },
            { "IceMakerService", DateTime.Now },
            { "FreezerDrawer Cleaning", DateTime.Now },
            { "Refrigerator Drawer Cleaning", DateTime.Now },
            { "Power Cord Check", DateTime.Now },
            { "Exterior Cleaning", DateTime.Now },
            { "Energy Efficiency Audit", DateTime.Now }
        };

        [JsonConstructor]
        public Fridge(string uuid = "", bool powered = false, bool hasFridge = false, bool hasFreezer = false, string room = "kitchen", string color = "white",
                  string model = "base", bool hasIceMaker = false, bool hasWaterDispenser = false, int temperature = 0, int capacity = 1, int temperatureSetting = 0, string saveLocation = "", Dictionary<string, int> reportFrequencies = null, Dictionary<string, DateTime> maintenanceDates = null, int expiryReportDays = 3, int maintenanceReportDays = 3)
        {

            UUID = FridgeManager.setUUID(uuid);
            Powered = powered;
            HasFridge = hasFridge;
            HasFreezer = hasFreezer;
            Room = room;
            Color = color;
            Model = model;
            HasIceMaker = hasIceMaker;
            HasWaterDispenser = hasWaterDispenser;
            Temperature = temperature;
            Capacity = capacity;
            TemperatureSetting = temperatureSetting;
            SaveLocation = saveLocation;
            ExpiryReportDays = expiryReportDays;
            ReportFrequencies = reportFrequencies;
            MaintenanceReportDays = maintenanceReportDays;
            MaintenanceDates = maintenanceDates;
        }

        public Fridge(Dictionary<string, string> input, string uuid = "")
        {
            UUID = FridgeManager.setUUID(uuid);
            Powered = bool.Parse(input.GetValueOrDefault("Powered", "false"));
            HasFridge = bool.Parse(input.GetValueOrDefault("HasFridge", "false"));
            HasFreezer = bool.Parse(input.GetValueOrDefault("HasFreezer", "false"));
            Room = input.GetValueOrDefault("Room", "kitchen");
            Color = input.GetValueOrDefault("Colour", "white");
            Model = input.GetValueOrDefault("Brand", "base");
            HasIceMaker = bool.Parse(input.GetValueOrDefault("HasIceMaker", "false"));
            HasWaterDispenser = bool.Parse(input.GetValueOrDefault("HasWaterDispenser", "false"));
            Temperature = int.Parse(input.GetValueOrDefault("Temperature", "0"));
            Capacity = int.Parse(input.GetValueOrDefault("Capacity", "1"));
            TemperatureSetting = int.Parse(input.GetValueOrDefault("Temperature_setting", "0"));
            SaveLocation = input.GetValueOrDefault("SaveLocation", ".");
        }

        public static Fridge Load(string path)
        {
            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<Fridge>(json);
        }

        public static void AddUser()
        {
            // TODO: Add user to list
        }

        public static void RemoveUser()
        {
            // TODO: Add user to list
        }

        public void Save(string path)
        {
            try
            {
                string json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(path, json);
                Console.WriteLine("[i] Fridge saved successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[!] Error saving fridge: {ex.Message}");
            }
        }

        public void AddItem(Item item)
        {
            Contents.Add(item);
        }

        public void RemoveItem(Item item)
        {
            Contents.Remove(item);
        }

        public bool HasCapacityFor(Item item)
        {
            return Contents.Count < Capacity;
        }

        public void PowerOn()
        {
            Powered = true;
        }

        public void PowerOff()
        {
            Powered = false;
        }
    }
}
