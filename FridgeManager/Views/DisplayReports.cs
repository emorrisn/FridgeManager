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
using System.Diagnostics;

namespace FridgeManager
{

    class DisplayReports : View
    {
        protected Stopwatch stopwatch = new Stopwatch();
        public void displayReportsMenu()
        {
            Console.Title = "FridgeManager: Reports";
            Console.Clear();

            stopwatch.Start();

            List<string> reportStrings = new List<string>(20);

            reportStrings.Add("Go Back");
            reportStrings.Add("Clear All");
            reportStrings.Add("");

            reportStrings.AddRange(sf.Reports.Select(report => $"{report.DateGenerated.ToString()} {report.ReportType} - {report.UUID}").ToList());

            stopwatch.Stop();

            StringBuilder headerBuilder = new StringBuilder();
            headerBuilder.AppendLine("\n  Currently viewing reports");
            headerBuilder.AppendLine($"  Listing: {sf.Reports.Count()}");
            headerBuilder.AppendLine($"  Elapsed Time: {stopwatch.ElapsedMilliseconds} ms");
            string header = headerBuilder.ToString();

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

            List<string> detailsStrings = new List<string>(20);

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
                    if (report.ReportType == "Expiry Report")
                    {
                        var timeLeft = item.ExpirationDate - DateTime.Now;

                        foodItems.AppendLine($"  * {item.Name} ({item.Quantity}) - Expires: {item.ExpirationDate} ({(int)timeLeft.TotalDays} days left)");
                    }
                    else if (report.ReportType == "Shopping Report")
                    {
                        foodItems.AppendLine($"  * {item.Name} ({item.Quantity} left)");
                    }
                    else
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
                        DateTime completed = sf.MaintenanceDates[item];
                        TimeSpan timeSinceCompletion = DateTime.Now - completed;

                        items.AppendLine($"  * {item} was last completed: {completed} - This was {timeSinceCompletion.Days} days ago");
                    }
                    else
                    {
                        items.AppendLine("  * " + item);
                    }
                }
            }

            StringBuilder headerBuilder = new StringBuilder();
            headerBuilder.AppendLine($"\n  Report: {report.UUID} ({report.ReportType})");
            headerBuilder.AppendLine($"  {report.Message}");
            headerBuilder.AppendLine($"  {items}{foodItems}");
            string header = headerBuilder.ToString();

            string selection = UI.Selector(detailsStrings.ToArray(), header);

            switch (selection)
            {
                case "Delete":
                    sf.Reports.Remove(report);
                    break;
                case "Mark as Complete":
                    markMaintenanceItemsAsComplete(report.Items);
                    sf.Reports.Remove(report);
                    break;
                default:
                    break;
            }
        }

        private void markMaintenanceItemsAsComplete(List<string> Items)
        {
            foreach (string item in Items)
            {
                sf.MaintenanceDates[item] = DateTime.Now;
            }
        }

    }

}
