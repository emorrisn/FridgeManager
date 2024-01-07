using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FridgeManager.Models
{
    public class Report : Model
    {
       
        public string ReportType { get; set; }
        public DateTime DateGenerated { get; set; }
        public List<Item> FoodItems { get; set; } // food items, if on the agenda

        public List<string> Items { get; set; } // general items, if on the agenda

        public string Message { get; set; }

        [JsonConstructor]
        public Report(string reportType, DateTime dateGenerated, string message, List<string> items = null, List<Item> foodItems = null, string uuid = "")
        {
            UUID = FridgeManager.setUUID(uuid);
            ReportType = reportType;
            FoodItems = foodItems;
            Items = items;
            DateGenerated = dateGenerated;
            Message = message;
        }

        // Copy constructor
        public Report(Report other)
        {
            // Copy the properties from the existing instance
            UUID = FridgeManager.setUUID(other.UUID);
            ReportType = other.ReportType;
            FoodItems = other.FoodItems != null ? new List<Item>(other.FoodItems) : null;
            Items = other.Items != null ? new List<string>(other.Items) : null;
            DateGenerated = other.DateGenerated;
            Message = other.Message;
        }
    }

}
