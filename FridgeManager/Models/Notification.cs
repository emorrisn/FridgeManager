using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FridgeManager.Models
{
    class Notification
    {
        public string NotificationMessage { get; set; }
        public User Recipient { get; set; }
    }
}
