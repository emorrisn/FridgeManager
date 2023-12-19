using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FridgeManager.Models
{
    public class Report
    {
        public string UUID { get; set; }
        public string ReportType { get; set; }
        public DateTime DateGenerated { get; set; }
        public List<Item> FoodItems { get; set; } // food items, if on the agenda

        public List<string> Items { get; set; } // general items, if on the agenda

        public string Message { get; set; }

        public Report(string reportType, DateTime dateGenerated, string message, List<string> items = null, List<Item> foodItems = null, string uuid = "")
        {
            UUID = FridgeManager.setUUID(uuid);
            ReportType = reportType;
            FoodItems = foodItems;
            Items = items;
            DateGenerated = dateGenerated;
            Message = message;
        }
    }

}
