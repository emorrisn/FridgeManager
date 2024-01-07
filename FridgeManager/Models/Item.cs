using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FridgeManager.Models
{
    public class ItemCategory
    {
        public string Name { get; set; }
    }

    public class Item: Model
    {
        private string name;

        public string Name
        {
            get => name;
            set
            {
                // Validate that the name is not null or empty and contains only letters
                if (!string.IsNullOrEmpty(value) && IsLettersOnly(value))
                {
                    name = value;
                }
                else
                {
                    // Log or throw an exception for invalid name
                    Console.WriteLine("Invalid name. Name should contain only letters.");
                }
            }
        }

        public int Quantity { get; set; }
        public DateTime ExpirationDate { get; set; }
        public ItemCategory Category { get; set; }
        public string Barcode { get; set; }

        [JsonConstructor]
        public Item(string name, int quantity, DateTime expirationDate, ItemCategory category, string barcode = "", string uuid = "")
        {
            UUID = FridgeManager.setUUID(uuid);
            Name = name;
            Quantity = quantity;
            ExpirationDate = expirationDate;
            Category = category;
            Barcode = barcode;
        }

        public Item(Dictionary<string, string> input, string uuid = "")
        {
            Item newItem = null;

            try
            {
                // Set UUID using the FridgeManager.setUUID method
                UUID = FridgeManager.setUUID(uuid);

                // Set Name from input or default to an empty string
                Name = input.GetValueOrDefault("Name", "");

                // Parse Quantity from input or default to 0
                Quantity = int.Parse(input.GetValueOrDefault("Quantity", "0"));

                // Create a new ItemCategory and set its Name from input or default to an empty string
                Category = new ItemCategory() { Name = input.GetValueOrDefault("Category", "") };

                // Set Barcode from input or default to an empty string
                Barcode = input.GetValueOrDefault("Barcode", "");

                // Parse and set ExpirationDate components from input
                int expirationDateDay = Convert.ToInt32(input.GetValueOrDefault("ExpirationDateDay", ""));
                int expirationDateMonth = Convert.ToInt32(input.GetValueOrDefault("ExpirationDateMonth", ""));
                int expirationDateYear = Convert.ToInt32(input.GetValueOrDefault("ExpirationDateYear", ""));
                int expirationDateHour = Convert.ToInt32(input.GetValueOrDefault("ExpirationDateHour", ""));
                int expirationDateMinute = Convert.ToInt32(input.GetValueOrDefault("ExpirationDateMin", ""));

                ExpirationDate = new DateTime(
                    expirationDateYear,
                    expirationDateMonth,
                    expirationDateDay,
                    expirationDateHour,
                    expirationDateMinute,
                    0
                );
            }
            catch (FormatException ex)
            {
                // Handle FormatException (e.g., if there's an issue with parsing)
                Console.WriteLine($"Error parsing input: {ex.Message}");
                Console.ReadLine(); 
            }
            catch (OverflowException ex)
            {
                // Handle OverflowException (e.g., if the parsed value is outside the range of the target type)
                Console.WriteLine($"Overflow error: {ex.Message}");
                Console.ReadLine();
            }
            catch (ArgumentNullException ex)
            {
                // Handle ArgumentNullException (e.g., if a required input is null)
                Console.WriteLine($"Input is null: {ex.Message}");
                Console.ReadLine();
            }
            catch (KeyNotFoundException ex)
            {
                // Handle KeyNotFoundException (e.g., if a key is not found in the dictionary)
                Console.WriteLine($"Key not found: {ex.Message}");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                Console.ReadLine();
            }
        }

        // Copy constructor
        public Item(Item other)
        {
            // Copy the properties from the existing instance
            UUID = FridgeManager.setUUID(other.UUID);
            Name = other.Name;
            Quantity = other.Quantity;
            ExpirationDate = other.ExpirationDate;
            Category = new ItemCategory() { Name = other.Category.Name};
            Barcode = other.Barcode;
        }

        public static bool validateInput(Dictionary<string, string> userInput)
        {
            if (string.IsNullOrEmpty(userInput["Name"]))
            {
                Console.WriteLine("\n{!} Name cannot be empty.");
                return false;
            }

            if (string.IsNullOrEmpty(userInput["Barcode"]))
            {
                Console.WriteLine("\n{!} Barcode cannot be empty.");
                return false;
            }

            if (string.IsNullOrEmpty(userInput["Category"]))
            {
                Console.WriteLine("\n{!} Category cannot be empty.");
                return false;
            }

            return true;
        }


    }
}
