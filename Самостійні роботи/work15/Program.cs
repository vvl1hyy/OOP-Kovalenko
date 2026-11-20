using System;
using System.Collections.Generic;
using System.Text;

namespace Lab9Self
{
    public class Product
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
    }

    public static class CollectionUtils
    {
        public static IEnumerable<TResult> Map<TSource, TResult>(
            IEnumerable<TSource> collection,
            Func<TSource, TResult> mapper)
        {
            var result = new List<TResult>();
            foreach (var item in collection)
            {
                result.Add(mapper(item));
            }
            return result;
        }

        public static T MaxBy<T, TKey>(
            IEnumerable<T> collection,
            Func<T, TKey> keySelector)
            where TKey : IComparable<TKey>
        {
            T maxItem = default(T);
            TKey maxKey = default(TKey);
            bool first = true;

            foreach (var item in collection)
            {
                TKey key = keySelector(item);
                if (first || key.CompareTo(maxKey) > 0)
                {
                    maxKey = key;
                    maxItem = item;
                    first = false;
                }
            }

            return maxItem;
        }

        public static IEnumerable<T> Filter<T>(
            IEnumerable<T> collection,
            Predicate<T> predicate)
        {
            var result = new List<T>();
            foreach (var item in collection)
            {
                if (predicate(item))
                {
                    result.Add(item);
                }
            }
            return result;
        }

        public static void ForEach<T>(
            IEnumerable<T> collection,
            Action<T> action)
        {
            foreach (var item in collection)
            {
                action(item);
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            var numbers = new List<int> { 3, 18, 7, 42, 10, 25 };
            var words = new List<string> { "cat", "keyboard", "sun", "programming", "code" };
            var products = new List<Product>
            {
                new Product { Name = "Laptop", Price = 25000 },
                new Product { Name = "Mouse", Price = 500 },
                new Product { Name = "Keyboard", Price = 1500 },
                new Product { Name = "Monitor", Price = 9000 }
            };

            var squared = CollectionUtils.Map(numbers, n => n * n);
            Console.WriteLine("Квадрати чисел: " + string.Join(", ", squared));

            var evenNumbers = CollectionUtils.Filter(numbers, n => n % 2 == 0);
            Console.WriteLine("Парні числа:");
            CollectionUtils.ForEach(evenNumbers, n => Console.WriteLine($"  - {n}"));

            var longestWord = CollectionUtils.MaxBy(words, w => w.Length);
            Console.WriteLine($"Найдовше слово: {longestWord}");

            var shortUpperWords = CollectionUtils.Map(
                CollectionUtils.Filter(words, w => w.Length <= 4),
                w => w.ToUpper()
            );
            Console.WriteLine("Короткі слова капсом: " + string.Join(", ", shortUpperWords));

            var maxPriceProduct = CollectionUtils.MaxBy(products, p => p.Price);
            Console.WriteLine($"Найдорожчий товар: {maxPriceProduct.Name} ({maxPriceProduct.Price} грн)");

            var midProducts = CollectionUtils.Filter(products, p => p.Price >= 1000 && p.Price <= 10000);
            Console.WriteLine("Товари від 1000 до 10000 грн:");
            CollectionUtils.ForEach(midProducts, p => Console.WriteLine($"  - {p.Name}: {p.Price} грн"));
        }
    }
}