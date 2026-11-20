using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace IndependentWork11
{
    public class Sale
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public DateTime SaleDate { get; set; }
        public int CustomerId { get; set; }
    }

    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
    }

    public static class SalesAnalytics
    {
        public static void TopProductsByCategory(
            IEnumerable<Sale> sales,
            IEnumerable<Product> products)
        {
            var sw = Stopwatch.StartNew();

            var result = sales
                .GroupBy(s => s.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    TotalQuantity = g.Sum(s => s.Quantity)
                })
                .Join(
                    products,
                    pq => pq.ProductId,
                    p => p.Id,
                    (pq, p) => new
                    {
                        p.Category,
                        p.Name,
                        pq.TotalQuantity
                    })
                .GroupBy(x => x.Category)
                .Select(cg => new
                {
                    Category = cg.Key,
                    TopProducts = cg
                        .OrderByDescending(x => x.TotalQuantity)
                        .Take(3)
                        .ToList()
                })
                .OrderBy(c => c.Category)
                .ToList();

            sw.Stop();

            Console.WriteLine($"=== Пайплайн 1: Топ-3 популярні продукти за категоріями ({sw.ElapsedTicks} ticks) ===");
            foreach (var cat in result)
            {
                Console.WriteLine($"Категорія: {cat.Category}");
                foreach (var item in cat.TopProducts)
                {
                    Console.WriteLine($"  - {item.Name}: {item.TotalQuantity} шт.");
                }
            }
            Console.WriteLine();
        }

        public static void RevenueByCustomerCity(
            IEnumerable<Sale> sales,
            IEnumerable<Product> products,
            IEnumerable<Customer> customers)
        {
            var sw = Stopwatch.StartNew();

            var result = sales
                .Join(
                    products,
                    s => s.ProductId,
                    p => p.Id,
                    (s, p) => new
                    {
                        s.CustomerId,
                        Revenue = s.Quantity * p.UnitPrice
                    })
                .Join(
                    customers,
                    sr => sr.CustomerId,
                    c => c.Id,
                    (sr, c) => new
                    {
                        c.City,
                        sr.Revenue
                    })
                .GroupBy(x => x.City)
                .Select(g => new
                {
                    City = g.Key,
                    TotalRevenue = g.Sum(x => x.Revenue)
                })
                .OrderByDescending(x => x.TotalRevenue)
                .ToList();

            sw.Stop();

            Console.WriteLine($"=== Пайплайн 2: Загальний дохід за містами клієнтів ({sw.ElapsedTicks} ticks) ===");
            foreach (var row in result)
            {
                Console.WriteLine($"  - {row.City}: {row.TotalRevenue:F2} грн");
            }
            Console.WriteLine();
        }

        public static void SalesByYearAndMonth(
            IEnumerable<Sale> sales,
            IEnumerable<Product> products)
        {
            var sw = Stopwatch.StartNew();

            var result = sales
                .Join(
                    products,
                    s => s.ProductId,
                    p => p.Id,
                    (s, p) => new
                    {
                        s.SaleDate.Year,
                        s.SaleDate.Month,
                        s.Quantity,
                        Revenue = s.Quantity * p.UnitPrice
                    })
                .GroupBy(x => new { x.Year, x.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    TotalQuantity = g.Sum(x => x.Quantity),
                    TotalRevenue = g.Sum(x => x.Revenue)
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToList();

            sw.Stop();

            Console.WriteLine($"=== Пайплайн 3: Продажі за роками та місяцями ({sw.ElapsedTicks} ticks) ===");
            foreach (var row in result)
            {
                Console.WriteLine($"  - {row.Year}-{row.Month:D2}: {row.TotalQuantity} шт., дохід {row.TotalRevenue:F2} грн");
            }
            Console.WriteLine();
        }

        public static void HighValueCategoryStats(
            IEnumerable<Sale> sales,
            IEnumerable<Product> products)
        {
            var sw = Stopwatch.StartNew();

            var result = sales
                .Where(s => s.Quantity >= 2)
                .Join(
                    products,
                    s => s.ProductId,
                    p => p.Id,
                    (s, p) => new
                    {
                        p.Category,
                        OrderAmount = s.Quantity * p.UnitPrice
                    })
                .GroupBy(x => x.Category)
                .Select(g => new
                {
                    Category = g.Key,
                    OrdersCount = g.Count(),
                    AverageOrderAmount = g.Average(x => x.OrderAmount)
                })
                .OrderByDescending(x => x.AverageOrderAmount)
                .ToList();

            sw.Stop();

            Console.WriteLine($"=== Пайплайн 4 (додатковий): Середній чек великих замовлень за категоріями ({sw.ElapsedTicks} ticks) ===");
            foreach (var row in result)
            {
                Console.WriteLine($"  - {row.Category}: замовлень {row.OrdersCount}, сер. сума {row.AverageOrderAmount:F2} грн");
            }
            Console.WriteLine();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            var rng = new Random(101);

            string[] categories = { "Electronics", "Books", "Accessories", "Home" };
            string[] cities = { "Kyiv", "Lviv", "Rivne", "Odesa", "Kharkiv" };
            string[] firstNames = { "Andriy", "Olena", "Maksym", "Sofia", "Dmytro", "Kateryna", "Tar", "Iryna" };

            var products = Enumerable.Range(1, 60).Select(i => new Product
            {
                Id = i,
                Name = $"Product_{i}",
                Category = categories[i % categories.Length],
                UnitPrice = Math.Round((decimal)(rng.NextDouble() * 4500 + 100), 2)
            }).ToList();

            var customers = Enumerable.Range(1, 60).Select(i => new Customer
            {
                Id = i,
                Name = $"{firstNames[i % firstNames.Length]}_{i}",
                City = cities[rng.Next(cities.Length)]
            }).ToList();

            var startDate = new DateTime(2025, 1, 1);
            var sales = Enumerable.Range(1, 150).Select(i => new Sale
            {
                Id = i,
                ProductId = rng.Next(1, products.Count + 1),
                CustomerId = rng.Next(1, customers.Count + 1),
                Quantity = rng.Next(1, 10),
                SaleDate = startDate.AddDays(rng.Next(0, 540))
            }).ToList();

            SalesAnalytics.TopProductsByCategory(sales, products);
            SalesAnalytics.RevenueByCustomerCity(sales, products, customers);
            SalesAnalytics.SalesByYearAndMonth(sales, products);
            SalesAnalytics.HighValueCategoryStats(sales, products);
        }
    }
}