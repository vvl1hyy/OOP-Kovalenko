using System;
using System.Collections.Generic;
using System.Linq;

namespace lab11v10
{
    public class Option
    {
        public int Id { get; set; }
        public string Value { get; set; } = string.Empty;
    }

    public class Feature
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<Option> Options { get; set; } = new();
    }

    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<Feature> Features { get; set; } = new();
    }

    public static class Extensions
    {
        public static bool HasFeature(this Product product, string featureName)
        {
            return product.Features.Any(f => f.Name.Equals(featureName, StringComparison.OrdinalIgnoreCase));
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var products = new List<Product>
            {
                new Product
                {
                    Id = 1,
                    Name = "Ноутбук Dell",
                    Features = new List<Feature>
                    {
                        new Feature { Id = 1, Name = "Процесор", Options = new List<Option> { new Option { Id = 1, Value = "Intel Core i7" } } },
                        new Feature { Id = 2, Name = "Пам'ять", Options = new List<Option> { new Option { Id = 2, Value = "16GB RAM" }, new Option { Id = 3, Value = "512GB SSD" } } }
                    }
                },
                new Product
                {
                    Id = 2,
                    Name = "Смартфон Samsung",
                    Features = new List<Feature>
                    {
                        new Feature { Id = 3, Name = "Екран", Options = new List<Option> { new Option { Id = 4, Value = "OLED 6.5" } } },
                        new Feature { Id = 4, Name = "Батарея", Options = new List<Option> { new Option { Id = 5, Value = "5000 mAh" } } }
                    }
                },
                new Product
                {
                    Id = 3,
                    Name = "Монітор LG",
                    Features = new List<Feature>
                    {
                        new Feature { Id = 5, Name = "Роздільна здатність", Options = new List<Option> { new Option { Id = 6, Value = "4K" } } }
                    }
                },
                new Product
                {
                    Id = 4,
                    Name = "Клавіатура Keychron",
                    Features = new List<Feature>
                    {
                        new Feature { Id = 6, Name = "Тип", Options = new List<Option> { new Option { Id = 7, Value = "Механічна" } } }
                    }
                }
            };

            Console.WriteLine("=== SelectMany: Усі опції всіх характеристик ===");
            var allOptions = products.SelectMany(p => p.Features).SelectMany(f => f.Options).ToList();
            foreach (var option in allOptions)
            {
                Console.WriteLine($"Опція: {option.Value}");
            }

            Console.WriteLine("\n=== Custom Extension: HasFeature ===");
            string searchFeature = "Екран";
            bool hasScreen = products[1].HasFeature(searchFeature);
            Console.WriteLine($"Продукт '{products[1].Name}' має характеристику '{searchFeature}': {hasScreen}");
            
            string searchFeature2 = "Камера";
            bool hasCamera = products[1].HasFeature(searchFeature2);
            Console.WriteLine($"Продукт '{products[1].Name}' має характеристику '{searchFeature2}': {hasCamera}");

            Console.WriteLine("\n=== Пагінація: Продукти по 3 на сторінку ===");
            int pageSize = 3;
            int totalPages = (int)Math.Ceiling(products.Count / (double)pageSize);

            for (int page = 1; page <= totalPages; page++)
            {
                Console.WriteLine($"--- Сторінка {page} ---");
                var pageProducts = products.Skip((page - 1) * pageSize).Take(pageSize).ToList();
                foreach (var product in pageProducts)
                {
                    Console.WriteLine($"Продукт: {product.Name}");
                }
            }
        }
    }
}