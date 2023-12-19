﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FridgeManager.Models
{
    public class ItemCategory
    {
        public string Name { get; set; }
    }

    public class Item
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
        public DateTime ExpirationDate { get; set; }
        public ItemCategory Category { get; set; }
        public string Barcode { get; set; }
    }
}
