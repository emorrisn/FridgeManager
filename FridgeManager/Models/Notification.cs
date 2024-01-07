using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FridgeManager.Models
{
    class Notification: Model
    {
        public string Message { get; set; }
        public DateTime DateGenerated { get; set; }

        [JsonConstructor]
        public Notification(string message, DateTime dateGenerated, string uuid = "")
        {
            Message = message;
            UUID = FridgeManager.setUUID(uuid);
            DateGenerated = dateGenerated;
        }

        // Copy constructor
        public Notification(Notification other)
        {
            // Copy the properties from the existing instance
            Message = other.Message;
            UUID = FridgeManager.setUUID(other.UUID);
            DateGenerated = other.DateGenerated;
        }
    }
}
