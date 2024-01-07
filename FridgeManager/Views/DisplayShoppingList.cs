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

    class DisplayShoppingList : View
    {
        public void displayShoppingListMenu()
        {
            Console.Title = "FridgeManager: Shopping List";
            Console.Clear();

            StringBuilder headerBuilder = new StringBuilder();
            headerBuilder.AppendLine("\n  Currently viewing reports");
            headerBuilder.AppendLine($"  Listing: {su.ShoppingList.Count()}\n");
            string header = headerBuilder.ToString();

            List<string> shoppingStrings = new List<string>(20);

            shoppingStrings.Add("Go Back");
            shoppingStrings.Add("Add Item");
            shoppingStrings.Add("Clear All");
            shoppingStrings.Add("");

            List<string> shoppingList = su.ShoppingList
                .GroupBy(item => item.Category.Name)
                .SelectMany(group =>
                {
                    string categoryName = group.Key ?? "Uncategorized";
                    return group.Select(item => $"[{categoryName}] {item.Name}: {item.Quantity} - {item.UUID}");
                })
                .ToList();

            shoppingStrings.AddRange(shoppingList);

            string selection = UI.Selector(shoppingStrings.ToArray(), header);

            switch (selection)
            {
                case string c when c.Contains("Clear All"):
                    su.ShoppingList.Clear();
                    sf.Save(sf.SaveLocation);
                    displayShoppingListMenu();
                    break;
                case string c when c.Contains(": "):
                    int indexOfDash = selection.IndexOf('-');
                    string uuid = selection.Substring(indexOfDash + 1).Trim();
                    Item item = su.ShoppingList.FirstOrDefault(i => i.UUID == uuid);
                    displayItemDetailsMenu(item);
                    sf.Save(sf.SaveLocation);
                    displayShoppingListMenu();
                    break;
                case string c when c.Contains("Add Item"):
                    AddItem();
                    break;
                case "":
                    displayShoppingListMenu();
                    break;
                default:
                    break;
            }
        }


        public void displayItemDetailsMenu(Item item)
        {
            Console.Title = "FridgeManager: Shopping List Item " + item.UUID;
            Console.Clear();

            List<string> detailsStrings = new List<string>(20);

            detailsStrings.Add("Go Back");
            detailsStrings.Add("Edit");
            detailsStrings.Add("Move to Fridge");
            detailsStrings.Add("Delete");

            StringBuilder headerBuilder = new StringBuilder();
            headerBuilder.AppendLine($"\n  {item.Name} ({item.Category.Name})");
            headerBuilder.AppendLine($"  Quantity: {item.Quantity}");
            headerBuilder.AppendLine($"  {item.UUID}");
            string header = headerBuilder.ToString();

            string selection = UI.Selector(detailsStrings.ToArray(), header);

            switch (selection)
            {
                case "Delete":
                    su.ShoppingList.Remove(item);
                    sf.Save(sf.SaveLocation);
                    break;
                case "Edit":
                    EditItem(item);
                    displayItemDetailsMenu(item);
                    break;
                case "Move to Fridge":
                    su.ShoppingList.Remove(item);
                    sf.Contents.Add(item);
                    sf.Save(sf.SaveLocation);
                    break;
                default:
                    break;
            }
        }

        private void AddItem()
        {
            Console.Title = "FridgeManager: Add Item to shopping list";
            Console.Clear();

            Dictionary<string, string> formResponse = UI.Form(new List<FormField>(9)
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

            if (Item.validateInput(formResponse))
            {
                Item item = new Item(formResponse);

                su.ShoppingList.Add(item);
            }
            else
            {
                Console.WriteLine("[i] Skipping item, please try again...");
                Console.ReadLine();
            }

            sf.Save(sf.SaveLocation);
        }

        private void EditItem(Item item)
        {
            Console.Title = "FridgeManager: Item Details in shopping list";
            Console.Clear();

            Dictionary<string, string> formResponse = UI.Form(new List<FormField>(9)
            {
                    new FormField { ID = 0, Name = "Name", Label = "Name", Value = item.Name },
                    new FormField { ID = 1, Name = "Quantity", Label = "Quantity", Value = item.Quantity },
                    new FormField { ID = 2, Name = "Category", Label = "Category", Value = item.Category },
                    new FormField { ID = 3, Name = "Barcode", Label = "Barcode", Value = item.Barcode },
                    new FormField { ID = 4, Name = "ExpirationDateDay", Label = "Expiration Day", Value = item.ExpirationDate.Day },
                    new FormField { ID = 5, Name = "ExpirationDateMonth", Label = "Expiration Month", Value = item.ExpirationDate.Month },
                    new FormField { ID = 6, Name = "ExpirationDateYear", Label = "Expiration Year", Value = item.ExpirationDate.Year },
                    new FormField { ID = 7, Name = "ExpirationDateHour", Label = "Expiration Hour", Value = item.ExpirationDate.Hour },
                    new FormField { ID = 8, Name = "ExpirationDateMin", Label = "Expiration Minute", Value = item.ExpirationDate.Minute },
            });

            if (Item.validateInput(formResponse))
            {
                Item editedItem = new Item(formResponse);

                su.ShoppingList.Remove(item);

                if (editedItem.Quantity > 0)
                {
                    su.ShoppingList.Add(editedItem);
                }
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
