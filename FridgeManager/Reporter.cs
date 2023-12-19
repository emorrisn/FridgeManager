using FridgeManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FridgeManager
{
    internal class Reporter
    {
        public static Fridge SelectedFridge = FridgeManager.SelectedFridge;
        public static User SelectedUser = FridgeManager.SelectedUser;

        // Once the fridge has been selected and the program is running, a constant reporting system will run which runs periodically (asynchronously).
        public static async Task reporterAsync()
        {
            while (true)
            {

                // Generate expiry report based on the items in the fridge that are going to expire within the next {ExpiryReportDays} days.
                if (NeedsReport("Expiry Report", SelectedFridge.ReportFrequencies["Expiry"]))
                {
                    if (SelectedFridge.Contents.Count() > 0)
                    {
                        List<Item> expiringItems = SelectedFridge.Contents.Where(item => item.ExpirationDate < DateTime.Now.AddDays(SelectedFridge.ExpiryReportDays)).ToList();

                        SelectedFridge.Reports.Add(new Report(reportType: "Expiry Report", foodItems: expiringItems, dateGenerated: DateTime.Now, message: "These items are expiring soon!"));
                    }
                }

                // Generate items to buy based on every user's shopping list linked to the selected fridge, comparing what's alreay in the fridge.
                if (NeedsReport("Shopping Report", SelectedFridge.ReportFrequencies["Shopping"]))
                {
                    if (SelectedUser.ShoppingList.Count() > 0)
                    {
                        SelectedFridge.Reports.Add(new Report(reportType: "Shopping Report", foodItems: CalculateItemsToBuy(), dateGenerated: DateTime.Now, message: "You need to buy these items!"));
                    }
                }

                // Generate items to maintain based on the maintenance dates of the fridge.
                if (NeedsReport("Maintenance Report", SelectedFridge.ReportFrequencies["Maintenance"]))
                {
                    List<string> itemsToMaintain = new List<string>();

                    foreach (string maintenanceItem in SelectedFridge.MaintenanceDates.Keys)
                    {
                        DateTime lastCompleted = SelectedFridge.MaintenanceDates[maintenanceItem];
                        TimeSpan timeSinceCompletion = DateTime.Now - lastCompleted;

                        // Check if the maintenance task needs attention (e.g., not completed within the specified frequency)
                        if (timeSinceCompletion.Days >= SelectedFridge.MaintenanceReportDays)
                        {
                            itemsToMaintain.Add(maintenanceItem);
                        }
                    }

                    if (itemsToMaintain.Count > 0)
                    {
                        SelectedFridge.Reports.Add(new Report(reportType: "Maintenance Report", items: itemsToMaintain, dateGenerated: DateTime.Now, message: "Your fridge requires maintenance!"));

                        // Now save changes
                        SelectedFridge.Save(SelectedFridge.SaveLocation);
                    }
                }

                // Check every 2 minutes the program is open (when testing: .5)
                await Task.Delay(TimeSpan.FromMinutes(2)).ConfigureAwait(false);
            }
        }

        private static bool NeedsReport(string reportType, int days)
        {
            // Check if a report of the specified type already exists
            Report existingReport = SelectedFridge.Reports.FirstOrDefault(r => r.ReportType == reportType);

            if (existingReport == null)
            {
                // Report doesn't exist, so it needs to be generated
                return true;
            }
            else
            {
                // Check if the report was generated more than a week ago
                return DateTime.Now - existingReport.DateGenerated > TimeSpan.FromDays(days);
            }
        }

        private static List<Item> CalculateItemsToBuy()
        {
            // Get all the shopping lists, group them by name and then create a new item from the group which sums the quantities.

            List<Item> shoppingLists = SelectedFridge.Users.SelectMany(u => u.ShoppingList)
              .GroupBy(item => item.Name)
              .Select(group => new Item
              {
                  Name = group.Key,
                  Quantity = group.Sum(item => item.Quantity),
                  ExpirationDate = group.First().ExpirationDate,
                  Category = group.First().Category,
                  Barcode = group.First().Barcode
              }).ToList();

            List<Item> itemsToBuy = shoppingLists
              .Select<Item?, Item>(itb => {
                  Item? itemInFridge = SelectedFridge.Contents.FirstOrDefault(iif => iif.Name == itb.Name);

                  // Item is not in the fridge, then just return the item from the shopping list
                  if (itemInFridge == null)
                  {
                      return itb;
                  }

                  // Item is in the fridge, but the quantity is lower in the fridge than the shopping list, create a new item with the quantity from the shopping list
                  if (itemInFridge.Quantity < itb.Quantity)
                  {
                      return new Item
                      {
                          Name = itb.Name,
                          Quantity = itb.Quantity - itemInFridge.Quantity,
                          ExpirationDate = itb.ExpirationDate,
                          Category = itb.Category,
                          Barcode = itb.Barcode
                      };
                  }

                  // Item is in the fridge, and the quantity is higher in the fridge than the shopping list, return null
                  return null;
              })
              .Where(result => result != null)
              .ToList();

            return itemsToBuy;
        }
    }
}
