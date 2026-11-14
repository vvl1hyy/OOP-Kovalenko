using System;
using System.Collections.Generic;

namespace lab19v10
{
    public class ProductOutOfStockException : Exception
    {
        public int ProductId { get; }
        public int RequestedQuantity { get; }

        public ProductOutOfStockException(int productId, int requestedQuantity) 
            : base($"Товар з ID {productId} у кількості {requestedQuantity} відсутній на склади або його недостатньо.")
        {
            ProductId = productId;
            RequestedQuantity = requestedQuantity;
        }

        public ProductOutOfStockException(string message) : base(message) { }

        public ProductOutOfStockException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class Result<T>
    {
        public bool IsSuccess { get; }
        public T Value { get; }
        public string Error { get; }

        private Result(T value, bool isSuccess, string error)
        {
            Value = value;
            IsSuccess = isSuccess;
            Error = error;
        }

        public static Result<T> Success(T value)
        {
            return new Result<T>(value, true, string.Empty);
        }

        public static Result<T> Failure(string error)
        {
            return new Result<T>(default, false, error);
        }

        public static Result<T> Failure(Exception ex)
        {
            return new Result<T>(default, false, ex.Message);
        }
    }

    public class InventoryService
    {
        private Dictionary<int, int> _inventory = new Dictionary<int, int>
        {
            { 1, 10 },
            { 2, 5 },
            { 3, 0 }
        };

        public void DecreaseStock(int productId, int quantity)
        {
            if (!_inventory.ContainsKey(productId))
            {
                throw new KeyNotFoundException($"Товар з ID {productId} не знайдено.");
            }

            if (_inventory[productId] < quantity)
            {
                throw new ProductOutOfStockException(productId, quantity);
            }

            _inventory[productId] -= quantity;
            Console.WriteLine($"[InventoryService] Успішно списано {quantity} од. товару ID {productId}. Залишок: {_inventory[productId]}");
        }

        public Result<bool> AddStock(int productId, int quantity)
        {
            if (quantity <= 0)
            {
                return Result<bool>.Failure("Кількість товару для додавання має бути більшою за нуль.");
            }

            if (!_inventory.ContainsKey(productId))
            {
                _inventory[productId] = 0;
            }

            _inventory[productId] += quantity;
            Console.WriteLine($"[InventoryService] Успішно додано {quantity} од. товару ID {productId}. Залишок: {_inventory[productId]}");
            
            return Result<bool>.Success(true);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var inventoryService = new InventoryService();

            Console.WriteLine("=== Тестування винятку (DecreaseStock) ===");
            try
            {
                inventoryService.DecreaseStock(1, 3);
                inventoryService.DecreaseStock(2, 10);
            }
            catch (ProductOutOfStockException ex)
            {
                Console.WriteLine($"[Перехоплено власний виняток] Помилка: {ex.Message} (ID товару: {ex.ProductId}, Запит: {ex.RequestedQuantity})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Перехоплено інший виняток] {ex.Message}");
            }

            Console.WriteLine("\n=== Тестування патерну Result (AddStock) ===");
            
            var successResult = inventoryService.AddStock(1, 5);
            if (successResult.IsSuccess)
            {
                Console.WriteLine("Результат операції: Успіх!");
            }
            else
            {
                Console.WriteLine($"Результат операції: Помилка -> {successResult.Error}");
            }

            Console.WriteLine();

            var failureResult = inventoryService.AddStock(2, -3);
            if (failureResult.IsSuccess)
            {
                Console.WriteLine("Результат операції: Успіх!");
            }
            else
            {
                Console.WriteLine($"Результат операції: Помилка -> {failureResult.Error}");
            }
        }
    }
}