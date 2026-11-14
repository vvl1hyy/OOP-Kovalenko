using System.Collections.Generic;
using Xunit;
using lab12v10;

namespace lab12v10.Tests
{
    public class BookRepositoryTests
    {
        [Fact]
        public void Add_ShouldAddItem_WhenItemIsValid()
        {
            var repo = new InMemoryRepository<Book>();
            var book = new Book { Id = 1, Title = "Тіні забутих предків", Author = "Михайло Коцюбинський", PublicationYear = 1912 };

            repo.Add(book);

            var retrieved = repo.GetById(1);
            Assert.NotNull(retrieved);
            Assert.Equal("Тіні забутих предків", retrieved.Title);
        }

        [Fact]
        public void GetById_ShouldThrowKeyNotFoundException_WhenIdDoesNotExist()
        {
            var repo = new InMemoryRepository<Book>();

            Assert.Throws<KeyNotFoundException>(() => repo.GetById(999));
        }

        [Fact]
        public void GetAll_ShouldReturnAllAddedItems()
        {
            var repo = new InMemoryRepository<Book>();
            repo.Add(new Book { Id = 1, Title = "Книга 1", Author = "Автор 1", PublicationYear = 2000 });
            repo.Add(new Book { Id = 2, Title = "Книга 2", Author = "Автор 2", PublicationYear = 2005 });

            var allBooks = repo.GetAll();

            Assert.Collection(allBooks,
                b => Assert.Equal(1, b.Id),
                b => Assert.Equal(2, b.Id)
            );
        }

        [Fact]
        public void Update_ShouldModifyExistingItem()
        {
            var repo = new InMemoryRepository<Book>();
            repo.Add(new Book { Id = 1, Title = "Стара назва", Author = "Автор", PublicationYear = 1990 });

            var updatedBook = new Book { Id = 1, Title = "Нова назва", Author = "Автор", PublicationYear = 1995 };
            repo.Update(updatedBook);

            var retrieved = repo.GetById(1);
            Assert.Equal("Нова назва", retrieved.Title);
            Assert.Equal(1995, retrieved.PublicationYear);
        }

        [Fact]
        public void Update_ShouldThrowKeyNotFoundException_WhenItemDoesNotExist()
        {
            var repo = new InMemoryRepository<Book>();
            var nonExistentBook = new Book { Id = 100, Title = "Тест", Author = "Тест", PublicationYear = 2020 };

            Assert.Throws<KeyNotFoundException>(() => repo.Update(nonExistentBook));
        }

        [Fact]
        public void Delete_ShouldRemoveItem_WhenIdExists()
        {
            var repo = new InMemoryRepository<Book>();
            repo.Add(new Book { Id = 1, Title = "Книга для видалення", Author = "Автор", PublicationYear = 2010 });

            repo.Delete(1);

            Assert.Throws<KeyNotFoundException>(() => repo.GetById(1));
        }

        [Fact]
        public void Delete_ShouldThrowKeyNotFoundException_WhenIdDoesNotExist()
        {
            var repo = new InMemoryRepository<Book>();

            Assert.Throws<KeyNotFoundException>(() => repo.Delete(404));
        }
    }
}