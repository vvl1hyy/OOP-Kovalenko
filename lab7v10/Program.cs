using System;

namespace lab12v10
{
    class Program
    {
        static void Main(string[] args)
        {
            var repo = new InMemoryRepository<Book>();

            repo.Add(new Book { Id = 1, Title = "Кобзар", Author = "Тарас Шевченко", PublicationYear = 1840 });
            repo.Add(new Book { Id = 2, Title = "Захар Беркут", Author = "Іван Франко", PublicationYear = 1883 });

            Console.WriteLine("Усі книги в репозиторії");
            foreach (var book in repo.GetAll())
            {
                Console.WriteLine($"ID: {book.Id}, Назва: {book.Title}, Автор: {book.Author}, Рік: {book.PublicationYear}");
            }

            Console.WriteLine("\nОновлення книги з ID 1");
            repo.Update(new Book { Id = 1, Title = "Кобзар (Видання нове)", Author = "Тарас Шевченко", PublicationYear = 1860 });
            var updatedBook = repo.GetById(1);
            Console.WriteLine($"Оновлено: {updatedBook.Title}, Рік: {updatedBook.PublicationYear}");

            Console.WriteLine("\nВидалення книги з ID 2");
            repo.Delete(2);
            Console.WriteLine($"Кількість книг після видалення: {System.Linq.Enumerable.Count(repo.GetAll())}");
        }
    }
}