using System;
using System.Collections.Generic;
using System.Linq;

namespace lab12v10
{
    public class InMemoryRepository<T> : IRepository<T> where T : IEntity
    {
        private readonly List<T> _items = new List<T>();

        public T GetById(int id)
        {
            var item = _items.FirstOrDefault(x => x.Id == id);
            if (item == null)
            {
                throw new KeyNotFoundException($"Елемент з ID {id} не знайдено.");
            }
            return item;
        }

        public IEnumerable<T> GetAll()
        {
            return _items.ToList();
        }

        public void Add(T item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }
            if (_items.Any(x => x.Id == item.Id))
            {
                throw new InvalidOperationException($"Елемент з ID {item.Id} вже існує.");
            }
            _items.Add(item);
        }

        public void Update(T item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            var existingIndex = _items.FindIndex(x => x.Id == item.Id);
            if (existingIndex == -1)
            {
                throw new KeyNotFoundException($"Елемент з ID {item.Id} для оновлення не знайдено.");
            }

            _items[existingIndex] = item;
        }

        public void Delete(int id)
        {
            var item = _items.FirstOrDefault(x => x.Id == id);
            if (item == null)
            {
                throw new KeyNotFoundException($"Елемент з ID {id} для видалення не знайдено.");
            }
            _items.Remove(item);
        }
    }
}