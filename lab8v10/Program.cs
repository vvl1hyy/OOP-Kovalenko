using System;
using System.Collections.Generic;

namespace lab14v10
{
    public static class CollectionHelper<T>
    {
        public static T? FirstOrDefault(IEnumerable<T> collection, Predicate<T> predicate)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            foreach (var item in collection)
            {
                if (predicate(item))
                {
                    return item;
                }
            }

            return default(T);
        }
    }

    public class Product
    {
        public string Name { get; set; }
        public decimal Price { get; set; }

        public Product(string name, decimal price)
        {
            Name = name;
            Price = price;
        }

        public override string ToString()
        {
            return $"{Name} (Ціна: {Price} грн)";
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("--- Тестування для List<string> ---");
            List<string> words = new List<string> { "Яблуко", "Банан", "Апельсин", "Ананас", "Груша" };
            
            string? firstAWord = CollectionHelper<string>.FirstOrDefault(words, w => w.StartsWith("А"));
            Console.WriteLine($"Перше слово на 'А': {firstAWord ?? "Не знайдено"}");

            string? firstKWord = CollectionHelper<string>.FirstOrDefault(words, w => w.StartsWith("К"));
            Console.WriteLine($"Перше слово на 'К': {firstKWord ?? "Не знайдено"}");


            Console.WriteLine("\n--- Тестування для List<Product> ---");
            List<Product> products = new List<Product>
            {
                new Product("Хліб", 25.50m),
                new Product("Молоко", 42.00m),
                new Product("Сир", 120.00m),
                new Product("Ковбаса", 250.00m)
            };

            Product? expensiveProduct = CollectionHelper<Product>.FirstOrDefault(products, p => p.Price > 100);
            Console.WriteLine($"Перший продукт дорожчий за 100 грн: {expensiveProduct?.ToString() ?? "Не знайдено"}");

            Product? veryExpensiveProduct = CollectionHelper<Product>.FirstOrDefault(products, p => p.Price > 500);
            Console.WriteLine($"Перший продукт дорожчий за 500 грн: {veryExpensiveProduct?.ToString() ?? "Не знайдено"}");
        }
    }
}