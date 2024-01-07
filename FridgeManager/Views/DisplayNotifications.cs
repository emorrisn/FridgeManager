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

    class DisplayNotifications : View
    {
        protected Stopwatch stopwatch = new Stopwatch();

        public void displayNotificationsMenu()
        {
            Console.Title = "FridgeManager: Notifications";
            Console.Clear();

            stopwatch.Start();

            List<string> notificationStrings = new List<string>(20);

            notificationStrings.Add("Go Back");
            notificationStrings.Add("Clear All");
            notificationStrings.Add("");

            notificationStrings.AddRange(su.Notifications.Select(notification => $"{(DateTime.Now - (notification.DateGenerated)).Days} days ago: {notification.Message} - {notification.UUID}").ToList());

            stopwatch.Stop();

            StringBuilder headerBuilder = new StringBuilder();
            headerBuilder.AppendLine("\n  Currently viewing notifications");
            headerBuilder.AppendLine($"  You have: {su.Notifications.Count()}");
            headerBuilder.AppendLine($"  Elapsed Time: {stopwatch.ElapsedMilliseconds} ms");
            string header = headerBuilder.ToString();

            string selection = UI.Selector(notificationStrings.ToArray(), header);

            switch (selection)
            {
                case string c when c.Contains("Clear All"):
                    su.Notifications.Clear();
                    sf.Save(sf.SaveLocation);
                    displayNotificationsMenu();
                    break;
                case string c when c.Contains("days ago: "):
                    int indexOfDash = selection.IndexOf('-');
                    string uuid = selection.Substring(indexOfDash + 1).Trim();
                    Notification notification = su.Notifications.FirstOrDefault(n => n.UUID == uuid);
                    displayNotificationDetailsMenu(notification);
                    sf.Save(sf.SaveLocation);
                    displayNotificationsMenu();
                    break;
                case "":
                    displayNotificationsMenu();
                    break;
                default:
                    break;
            }
        }

        public void displayNotificationDetailsMenu(Notification notification)
        {
            Console.Title = "FridgeManager: Notification " + notification.UUID;
            Console.Clear();

            List<string> detailsStrings = new List<string>(20);

            detailsStrings.Add("Go Back");
            detailsStrings.Add("Delete");

            StringBuilder foodItems = new StringBuilder("");

            string header = @$"
  Notification: {notification.UUID}
  Message: {notification.Message}
  ";

            string selection = UI.Selector(detailsStrings.ToArray(), header);

            switch (selection)
            {
                case "Delete":
                    su.Notifications.Remove(notification);
                    break;
                default:
                    break;
            }
        }

    }

}
