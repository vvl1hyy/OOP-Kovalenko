using System;
using System.Collections.Generic;

namespace IndependentWork7
{
    public class Cache<TKey, TValue>
    {
        private readonly int _capacity;
        private readonly Dictionary<TKey, TValue> _cache;
        private readonly Queue<TKey> _keysOrder;

        public Cache(int capacity)
        {
            if (capacity <= 0)
            {
                throw new ArgumentException("Ємність кешу має бути більшою за нуль.", nameof(capacity));
            }

            _capacity = capacity;
            _cache = new Dictionary<TKey, TValue>();
            _keysOrder = new Queue<TKey>();
        }

        public int Count => _cache.Count;

        public void Add(TKey key, TValue value)
        {
            if (_cache.ContainsKey(key))
            {
                _cache[key] = value;
                return;
            }

            if (_cache.Count >= _capacity)
            {
                if (_keysOrder.Count > 0)
                {
                    TKey oldestKey = _keysOrder.Dequeue();
                    _cache.Remove(oldestKey);
                }
            }

            _cache[key] = value;
            _keysOrder.Enqueue(key);
        }

        public TValue Get(TKey key)
        {
            if (!_cache.TryGetValue(key, out TValue value))
            {
                throw new KeyNotFoundException($"Ключ '{key}' не знайдено в кеші.");
            }
            return value;
        }

        public bool ContainsKey(TKey key)
        {
            return _cache.ContainsKey(key);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var cache = new Cache<int, string>(3);

            cache.Add(1, "Один");
            cache.Add(2, "Два");
            cache.Add(3, "Три");

            Console.WriteLine($"Кількість елементів у кеші: {cache.Count}");
            Console.WriteLine($"Містить ключ 1: {cache.ContainsKey(1)}");
            Console.WriteLine($"Отримано ключ 2: {cache.Get(2)}");

            Console.WriteLine("Додаємо елемент 4 (перевищення ємність, має видалитися найстаріший - ключ 1)...");
            cache.Add(4, "Чотири");

            Console.WriteLine($"Містить ключ 1 після переповнення: {cache.ContainsKey(1)}");
            Console.WriteLine($"Містить ключ 4: {cache.ContainsKey(4)}");
            Console.WriteLine($"Поточна кількість елементів: {cache.Count}");
        }
    }
}