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
using System.Net.Http.Headers;

namespace FridgeManager.Models
{

    public class Appliance: Model
    {
        public virtual string Brand { get; set; }
        public virtual string Model { get; set; }

        public virtual string DisplayInfo()
        {
            return $"Brand: {Brand} | Model: {Model}";
        }
    }

    class Fridge : Appliance
    {
        // Attributes
        public bool Powered { get; set; }
        public bool HasFridge { get; set; }
        public bool HasFreezer { get; set; }
        public string Room { get; set; }
        public string Color { get; set; }
        public override string Brand { get; set; }
        public override string Model { get; set; }
        public bool HasIceMaker { get; set; }
        public bool HasWaterDispenser { get; set; }

        public List<Report> Reports { get; set; } = new List<Report>(100);

        private int temperature;

        public int Temperature
        {
            get => temperature;
            set
            {
                // Validate that the temperature is within a reasonable range (e.g., -20 to 10 degrees Celsius)
                if (value >= -20 && value <= 10)
                {
                    temperature = value;
                }
                else
                {
                    // Log or throw an exception for invalid temperature
                    Console.WriteLine("[!] Invalid temperature. Temperature should be between -20 and 10 degrees Celsius.");
                }
            }
        }

        public override string DisplayInfo()
        {
            return $"{Brand} Fridge, Model: {Model}";
        }
        private int capacity;

        // Property with validation using IsValidCapacity method
        public int Capacity
        {
            get { return capacity; }
            set
            {
                if (IsValidCapacity(value))
                {
                    capacity = value;
                }
                else
                {
                    Console.WriteLine("Invalid capacity value. Capacity must be greater than 0.");
                    // You might want to throw an exception or handle the error accordingly
                }
            }
        }
        public int TemperatureSetting { get; set; }
        public List<Item> Contents { get; set; } = new List<Item>(100);

        public List<User> Users { get; set; } = new List<User>(10);
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
                  string brand = "unknown", string model = "base", bool hasIceMaker = false, bool hasWaterDispenser = false, int temperature = 0, int capacity = 1, int temperatureSetting = 0, string saveLocation = "", Dictionary<string, int> reportFrequencies = null, Dictionary<string, DateTime> maintenanceDates = null, int expiryReportDays = 3, int maintenanceReportDays = 3, List<Item> contents = null)
        {

            UUID = FridgeManager.setUUID(uuid);
            Powered = powered;
            HasFridge = hasFridge;
            HasFreezer = hasFreezer;
            Room = room;
            Color = color;
            Brand = brand;
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

            Contents = contents ?? new List<Item>(1);
        }

        public Fridge(Dictionary<string, string> input, string uuid = "")
        {
            UUID = FridgeManager.setUUID(uuid);
            Powered = bool.Parse(input.GetValueOrDefault("Powered", "false"));
            HasFridge = bool.Parse(input.GetValueOrDefault("HasFridge", "false"));
            HasFreezer = bool.Parse(input.GetValueOrDefault("HasFreezer", "false"));
            Room = input.GetValueOrDefault("Room", "kitchen");
            Color = input.GetValueOrDefault("Colour", "white");
            Brand = input.GetValueOrDefault("Brand", "base");
            Model = input.GetValueOrDefault("Model", "base");
            HasIceMaker = bool.Parse(input.GetValueOrDefault("HasIceMaker", "false"));
            HasWaterDispenser = bool.Parse(input.GetValueOrDefault("HasWaterDispenser", "false"));
            Temperature = int.Parse(input.GetValueOrDefault("Temperature", "0"));
            Capacity = int.Parse(input.GetValueOrDefault("Capacity", "1"));
            TemperatureSetting = int.Parse(input.GetValueOrDefault("Temperature_setting", "0"));
            SaveLocation = input.GetValueOrDefault("SaveLocation", ".");
            ExpiryReportDays = int.Parse(input.GetValueOrDefault("ExpiryReportDays", "3"));
            MaintenanceReportDays = int.Parse(input.GetValueOrDefault("MaintenanceReportDays", "30"));
            ReportFrequencies["Shopping"] = int.Parse(input.GetValueOrDefault("ReportFrequencyShopping", "1"));
            ReportFrequencies["Maintenance"] = int.Parse(input.GetValueOrDefault("ReportFrequencyMaintenance", "7"));
            ReportFrequencies["Expiry"] = int.Parse(input.GetValueOrDefault("ReportFrequencyExpiry", "7"));
        }

        public Fridge(Fridge other)
        {
            // Copy the properties from the existing instance
            UUID = other.UUID;
            Powered = other.Powered;
            HasFridge = other.HasFridge;
            HasFreezer = other.HasFreezer;
            Room = other.Room;
            Color = other.Color;
            Brand = other.Brand;
            Model = other.Model;
            HasIceMaker = other.HasIceMaker;
            HasWaterDispenser = other.HasWaterDispenser;
            Temperature = other.Temperature;
            Capacity = other.Capacity;
            TemperatureSetting = other.TemperatureSetting;
            SaveLocation = other.SaveLocation;
            ExpiryReportDays = other.ExpiryReportDays;

            // Copy ReportFrequencies dictionary
            if (other.ReportFrequencies != null)
            {
                ReportFrequencies = new Dictionary<string, int>(other.ReportFrequencies);
            }
            else
            {
                ReportFrequencies = null;
            }

            MaintenanceReportDays = other.MaintenanceReportDays;

            // Copy MaintenanceDates dictionary
            if (other.MaintenanceDates != null)
            {
                MaintenanceDates = new Dictionary<string, DateTime>(other.MaintenanceDates);
            }
            else
            {
                MaintenanceDates = null;
            }

            // Copy the contents list
            if (other.Contents != null)
            {
                Contents = new List<Item>(other.Contents);
            }
            else
            {
                Contents = null;
            }
        }

        public static Fridge Load(string path)
        {
            try
            {
                string json = File.ReadAllText(path);
                return JsonSerializer.Deserialize<Fridge>(json);
            }
            catch (NotSupportedException e) {
                Console.WriteLine("[!] Error loading fridge: Config file is corrupted or needs fixing.");
                Console.WriteLine(e.Message);
            } catch(Exception ex)
            {
                Console.WriteLine($"[!] Error loading fridge: {ex.Message}");
            }
            return null;
        }

        public void Save(string path)
        {
            try
            {
                string json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(path, json);
                Console.WriteLine("[i] Fridge saved successfully.");
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"[!] Unauthorized access: {ex.Message}");
            }
            catch (IOException ex)
            {
                Console.WriteLine($"[!] I/O error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[!] Error saving fridge: {ex.Message}");
            }
        }

        public static bool validateInput(Dictionary<string, string> userInput)
        {
            if (string.IsNullOrEmpty(userInput["Room"]))
            {
                Console.WriteLine("\n{!} Room cannot be empty.");
                return false;
            }

            if (string.IsNullOrEmpty(userInput["Brand"]))
            {
                Console.WriteLine("\n{!} Brand cannot be empty.");
                return false;
            }

            if (string.IsNullOrEmpty(userInput["Model"]))
            {
                Console.WriteLine("\n{!} Model cannot be empty.");
                return false;
            }

            if (string.IsNullOrEmpty(userInput["Colour"]))
            {
                Console.WriteLine("\n{!} Colour cannot be empty.");
                return false;
            }

            if (!int.TryParse(userInput["Capacity"], out _))
            {
                Console.WriteLine("\n{!} Capacity must be a valid integer.");
                return false;
            }

            return true;
        }
    }
}
