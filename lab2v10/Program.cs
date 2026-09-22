using System;

namespace lab2v10
{
    public class Computer
    {
        private string _cpu = string.Empty;
        private int _ramGB;
        private int _storageGB;

        public string CPU
        {
            get => _cpu;
            set => _cpu = string.IsNullOrWhiteSpace(value) ? "Unknown CPU" : value;
        }

        public int RAMGB
        {
            get => _ramGB;
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException("Обсяг оперативної пам'яті повинен бути більшим за 0!");
                }
                _ramGB = value;
            }
        }

        public int StorageGB
        {
            get => _storageGB;
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException("Обсяг накопичувача повинен бути більшим за 0!");
                }
                _storageGB = value;
            }
        }

        public Computer(string cpu, int ramGB, int storageGB)
        {
            CPU = cpu;
            RAMGB = ramGB;
            StorageGB = storageGB;
        }

        public Computer() : this("Intel i5", 8, 256)
        {
        }

        ~Computer()
        {
            Console.WriteLine($"[Garbage Collector] Об'єкт ПК ({CPU}, {RAMGB}GB RAM) знищено з пам'яті.");
        }

        public void RunBenchmark()
        {
            int score = RAMGB * 100 + StorageGB * 10;
            Console.WriteLine($"Запуск бенчмарку для {CPU} ({RAMGB}GB RAM, {StorageGB}GB SSD)... Результат: {score} балів.");
        }

        public string GetInfo()
        {
            return $"ПК: CPU = {CPU}, RAM = {RAMGB}GB, Storage = {StorageGB}GB";
        }
    }

    class Program
    {
        static void TestObjects()
        {
            Console.WriteLine("=== Створення об'єктів ===");

            Computer? pc1 = new Computer();
            Console.WriteLine($"pc1 (конструктор за замовчуванням): {pc1.GetInfo()}");
            pc1.RunBenchmark();

            Console.WriteLine();

            Computer? pc2 = new Computer("AMD Ryzen 7 7800X3D", 32, 1024);
            Console.WriteLine($"pc2 (параметризований конструктор): {pc2.GetInfo()}");
            pc2.RunBenchmark();

            Console.WriteLine();

            try
            {
                Computer? pcInvalid = new Computer("Intel i3", -4, 0);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Помилка валідації: {ex.Message}");
            }
        }

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            TestObjects();

            Console.WriteLine();
            Console.WriteLine("=== Завершення роботи Main, підготовка до збору сміття (GC) ===");

            GC.Collect();
            GC.WaitForPendingFinalizers();

            Console.WriteLine("=== Програму завершено ===");
        }
    }
}