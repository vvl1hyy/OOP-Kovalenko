using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace lab16v10
{
    public class Location
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Warehouse { get; set; }
    }

    public class Item
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public int LocationId { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var locations = new List<Location>
            {
                new Location { Id = 1, Name = "Zone A", Warehouse = "Main Warehouse" },
                new Location { Id = 2, Name = "Zone B", Warehouse = "Main Warehouse" },
                new Location { Id = 3, Name = "Zone C", Warehouse = "Secondary Warehouse" },
                new Location { Id = 4, Name = "Zone D", Warehouse = "Main Warehouse" }
            };

            var items = new List<Item>
            {
                new Item { Id = 1, Name = "Laptop", Quantity = 5, LocationId = 1 },
                new Item { Id = 2, Name = "Mouse", Quantity = 15, LocationId = 1 },
                new Item { Id = 3, Name = "Keyboard", Quantity = 8, LocationId = 2 },
                new Item { Id = 4, Name = "Monitor", Quantity = 3, LocationId = 2 },
                new Item { Id = 5, Name = "Printer", Quantity = 12, LocationId = 3 },
                new Item { Id = 6, Name = "Scanner", Quantity = 2, LocationId = 3 },
                new Item { Id = 7, Name = "Tablet", Quantity = 7, LocationId = 4 },
                new Item { Id = 8, Name = "Phone", Quantity = 25, LocationId = 4 },
                new Item { Id = 9, Name = "Router", Quantity = 4, LocationId = 1 },
                new Item { Id = 10, Name = "Switch", Quantity = 11, LocationId = 2 },
                new Item { Id = 11, Name = "Cables", Quantity = 50, LocationId = 3 },
                new Item { Id = 12, Name = "Server", Quantity = 1, LocationId = 4 }
            };

            var stopwatch = new Stopwatch();

            Console.WriteLine("--- Запит 1: Предмети з кількістю < 10 на Main Warehouse ---");
            stopwatch.Start();
            var q1 = (from item in items
                      join loc in locations on item.LocationId equals loc.Id
                      where item.Quantity < 10 && loc.Warehouse == "Main Warehouse"
                      select item).ToList();
            stopwatch.Stop();
            Console.WriteLine($"Query syntax time: {stopwatch.ElapsedTicks} ticks");

            stopwatch.Restart();
            var m1 = items.Join(locations, i => i.LocationId, l => l.Id, (i, l) => new { i, l })
                          .Where(x => x.i.Quantity < 10 && x.l.Warehouse == "Main Warehouse")
                          .Select(x => x.i)
                          .ToList();
            stopwatch.Stop();
            Console.WriteLine($"Method syntax time: {stopwatch.ElapsedTicks} ticks");


            Console.WriteLine("\n--- Запит 2: Об'єднання предметів з локаціями ---");
            stopwatch.Restart();
            var q2 = (from item in items
                      join loc in locations on item.LocationId equals loc.Id
                      select new { item.Name, LocationName = loc.Name }).ToList();
            stopwatch.Stop();
            Console.WriteLine($"Query syntax time: {stopwatch.ElapsedTicks} ticks");

            stopwatch.Restart();
            var m2 = items.Join(locations, i => i.LocationId, l => l.Id, (i, l) => new { i.Name, LocationName = l.Name }).ToList();
            stopwatch.Stop();
            Console.WriteLine($"Method syntax time: {stopwatch.ElapsedTicks} ticks");


            Console.WriteLine("\n--- Запит 3: Загальна кількість предметів у кожній локації ---");
            stopwatch.Restart();
            var q3 = (from item in items
                      join loc in locations on item.LocationId equals loc.Id
                      group item by loc.Name into g
                      select new { Location = g.Key, Total = g.Sum(x => x.Quantity) }).ToList();
            stopwatch.Stop();
            Console.WriteLine($"Query syntax time: {stopwatch.ElapsedTicks} ticks");

            stopwatch.Restart();
            var m3 = items.Join(locations, i => i.LocationId, l => l.Id, (i, l) => new { l.Name, i.Quantity })
                          .GroupBy(x => x.Name)
                          .Select(g => new { Location = g.Key, Total = g.Sum(x => x.Quantity) })
                          .ToList();
            stopwatch.Stop();
            Console.WriteLine($"Method syntax time: {stopwatch.ElapsedTicks} ticks");


            Console.WriteLine("\n--- Запит 4: Предмет з максимальною кількістю ---");
            stopwatch.Restart();
            var q4 = (from item in items
                      orderby item.Quantity descending
                      select item).FirstOrDefault();
            stopwatch.Stop();
            Console.WriteLine($"Query syntax time: {stopwatch.ElapsedTicks} ticks");

            stopwatch.Restart();
            var m4 = items.OrderByDescending(x => x.Quantity).FirstOrDefault();
            stopwatch.Stop();
            Console.WriteLine($"Method syntax time: {stopwatch.ElapsedTicks} ticks");
        }
    }
}