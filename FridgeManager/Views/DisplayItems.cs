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
using System.Security;
using System.Diagnostics;

namespace FridgeManager
{

    class DisplayItems : View
    {
        protected Stopwatch stopwatch = new Stopwatch();

        public void displayItemsMenu(string findItem = "")
        {
            Console.Title = "FridgeManager: Items";
            Console.Clear();

            stopwatch.Start();

            List<string> itemStrings = new List<string>(50);

            itemStrings.Add("Go Back");
            itemStrings.Add("Add Item");
            itemStrings.Add("Find Item");
            itemStrings.Add("");

            try
            {
                List<string> items = new List<string>(50);


                if (sf.Contents.Count() > 0)
                {
                    if (findItem != "")
                    {
                        items = sf.Contents
                            .Where(i => i.Name.ToLower().Contains(findItem.ToLower()))
                            .GroupBy(item => item.Category.Name)
                            .SelectMany(group =>
                            {
                                string categoryName = group.Key ?? "Uncategorized";
                                return group.Select(item => $"[{categoryName}] {item.Name}: {item.Quantity} - Expires: {item.ExpirationDate} | {item.UUID}");
                            })
                            .ToList();
                    }
                    else
                    {
                        items = sf.Contents
                        .GroupBy(item => item.Category.Name)
                        .SelectMany(group =>
                        {
                            string categoryName = group.Key ?? "Uncategorized";
                            return group.Select(item => $"[{categoryName}] {item.Name}: {item.Quantity} - Expires: {item.ExpirationDate} | {item.UUID}");
                        })
                        .ToList();
                    }
                }

                itemStrings.AddRange(items);
            } catch(NullReferenceException e)
            {
                Console.WriteLine("[!] Couldn't load items, please check your config file.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[!] Error loading fridge: {ex.Message}");
            }

            stopwatch.Stop();

            int totalQuantity = 0;
            DateTime earliestExpirationDate = DateTime.Now;
            DateTime latestExpirationDate = DateTime.Now;

            if (sf.Contents.Count() > 0)
            {
                totalQuantity = sf.Contents.Sum(item => item.Quantity);
                earliestExpirationDate = sf.Contents.Min(item => item.ExpirationDate);
                latestExpirationDate = sf.Contents.Max(item => item.ExpirationDate);
            }
            

            StringBuilder headerBuilder = new StringBuilder();
            headerBuilder.AppendLine("\n  Currently viewing fridge items");
            headerBuilder.AppendLine($"  Listing: {sf.Contents.Count()}");
            headerBuilder.AppendLine($"  Total Quantity: {totalQuantity}");
            headerBuilder.AppendLine($"  Earliest Expiration: {earliestExpirationDate}");
            headerBuilder.AppendLine($"  Latest Expiration: {latestExpirationDate}");
            headerBuilder.AppendLine($"  Elapsed Time: {stopwatch.ElapsedMilliseconds} ms");

            if(findItem != "")
            {
                headerBuilder.AppendLine($"  Search: {findItem}");
            }

            string header = headerBuilder.ToString();



            string selection = UI.Selector(itemStrings.ToArray(), header);

            switch (selection)
            {
                case string c when c.Contains("Expires: "):
                    int indexOfDash = selection.IndexOf('|');
                    string uuid = selection.Substring(indexOfDash + 1).Trim();
                    Item item = sf.Contents.FirstOrDefault(i => i.UUID == uuid);
                    displayItemDetailsMenu(item);
                    displayItemsMenu();
                    break;
                case string c when c.Contains("Add Item"):
                    addItem();
                    displayItemsMenu();
                    break;
                case "Find Item":
                    Console.Write("Enter the name of the item you want to find:");
                    string name = Console.ReadLine();
                    displayItemsMenu(name);
                    break;
                case "":
                    displayItemsMenu();
                    break;
                default:
                    break;
            }
        }

        private void displayItemDetailsMenu(Item item)
        {
            Console.Title = "FridgeManager: Item Details";
            Console.Clear();

            Dictionary<string, string> userInput = UI.Form(new List<FormField>(9) {
                    new FormField { ID = 0, Name = "Name", Label = "Name", Value = item.Name },
                    new FormField { ID = 1, Name = "Quantity", Label = "Quantity", Value = item.Quantity },
                    new FormField { ID = 2, Name = "Category", Label = "Category", Value = item.Category.Name },
                    new FormField { ID = 3, Name = "Barcode", Label = "Barcode", Value = item.Barcode },
                    new FormField { ID = 4, Name = "ExpirationDateDay", Label = "Expiration Day", Value = item.ExpirationDate.Day },
                    new FormField { ID = 5, Name = "ExpirationDateMonth", Label = "Expiration Month", Value = item.ExpirationDate.Month },
                    new FormField { ID = 6, Name = "ExpirationDateYear", Label = "Expiration Year", Value = item.ExpirationDate.Year },
                    new FormField { ID = 7, Name = "ExpirationDateHour", Label = "Expiration Hour", Value = item.ExpirationDate.Hour },
                    new FormField { ID = 8, Name = "ExpirationDateMin", Label = "Expiration Minute", Value = item.ExpirationDate.Minute },
                });


            if (Item.validateInput(userInput))
            {
                Item editedItem = new Item(userInput);

                sf.Contents.Remove(item);

                if (editedItem.Quantity > 0)
                {
                    sf.Contents.Add(editedItem);
                }
            }
            else
            {
                Console.WriteLine("[i] Skipping item, please try again...");
                Console.ReadLine();
            }

            sf.Save(sf.SaveLocation);
        }

        private void addItem()
        {
            Console.Title = "FridgeManager: Add Item";
            Console.Clear();

            Dictionary<string, string> userInput = UI.Form(new List<FormField>(9)
            {
                    new FormField { ID = 0, Name = "Name", Label = "Name" },
                    new FormField { ID = 1, Name = "Quantity", Label = "Quantity" },
                    new FormField { ID = 2, Name = "Category", Label = "Category" },
                    new FormField { ID = 3, Name = "Barcode", Label = "Barcode" },
                    new FormField { ID = 4, Name = "ExpirationDateDay", Label = "Expiration Day", Value = DateTime.Now.AddDays(7).Day },
                    new FormField { ID = 5, Name = "ExpirationDateMonth", Label = "Expiration Month", Value = DateTime.Now.AddDays(7).Month },
                    new FormField { ID = 6, Name = "ExpirationDateYear", Label = "Expiration Year", Value = DateTime.Now.AddDays(7).Year },
                    new FormField { ID = 7, Name = "ExpirationDateHour", Label = "Expiration Hour", Value = DateTime.Now.AddDays(7).Hour },
                    new FormField { ID = 8, Name = "ExpirationDateMin", Label = "Expiration Minute", Value = DateTime.Now.AddDays(7).Minute },
            });

            // Add validation here

            if (Item.validateInput(userInput))
            {
                // If validation passes, create the Item object
                Item item = new Item(userInput);

                // Add the item to the contents
                sf.Contents.Add(item);
            }
            else
            {
                Console.WriteLine("[i] Skipping item, please try again...");
                Console.ReadLine();
            }

            sf.Save(sf.SaveLocation);
        }

    }

}
