using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;

namespace lab18v10
{
    public static class RetryHelper
    {
        public static T ExecuteWithRetry<T>(Func<T> operation, int retryCount = 3, TimeSpan initialDelay = default, Func<Exception, bool> shouldRetry = null)
        {
            if (initialDelay == default)
            {
                initialDelay = TimeSpan.FromSeconds(1);
            }

            int attempt = 0;
            while (true)
            {
                try
                {
                    attempt++;
                    Console.WriteLine($"[Спроба {attempt}] Виконання операції...");
                    return operation();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Помилка] Спроба {attempt} завершилася невдачею: {ex.Message}");

                    bool canRetry = attempt <= retryCount && (shouldRetry == null || shouldRetry(ex));

                    if (!canRetry)
                    {
                        Console.WriteLine("[Невдача] Перевищено ліміт спроб або виняток не підлягає повторенню.");
                        throw;
                    }

                    TimeSpan delay = TimeSpan.FromTicks(initialDelay.Ticks * (long)Math.Pow(2, attempt - 1));
                    Console.WriteLine($"[Очікування] Повторна спроба через {delay.TotalSeconds} секунд(и)...");
                    Thread.Sleep(delay);
                }
            }
        }

        public static void ExecuteWithRetry(Action operation, int retryCount = 3, TimeSpan initialDelay = default, Func<Exception, bool> shouldRetry = null)
        {
            ExecuteWithRetry<object>(() =>
            {
                operation();
                return null;
            }, retryCount, initialDelay, shouldRetry);
        }
    }

    public class FileProcessor
    {
        private int _attemptsCount = 0;

        public string GetEmailTemplate(string path)
        {
            _attemptsCount++;
            if (_attemptsCount <= 3)
            {
                throw new IOException("Тимчасова помилка введення-виведення при читанні шаблону листа.");
            }
            return "Шаблон електронного листа: Привіт, {{Name}}!";
        }
    }

    public class NetworkClient
    {
        private int _attemptsCount = 0;

        public void SendEmail(string recipient, string subject, string body)
        {
            _attemptsCount++;
            if (_attemptsCount <= 2)
            {
                throw new HttpRequestException("Тимчасова мережева помилка при відправці листа.");
            }
            Console.WriteLine($"Лист успішно надіслано на адресу: {recipient}");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var fileProcessor = new FileProcessor();
            var networkClient = new NetworkClient();

            Func<Exception, bool> shouldRetryCondition = ex => ex is IOException || ex is HttpRequestException;

            Console.WriteLine("=== Тестування FileProcessor (GetEmailTemplate) ===");
            try
            {
                string template = RetryHelper.ExecuteWithRetry(
                    () => fileProcessor.GetEmailTemplate("template.txt"),
                    retryCount: 3,
                    initialDelay: TimeSpan.FromMilliseconds(500),
                    shouldRetry: shouldRetryCondition
                );
                Console.WriteLine($"Результат: {template}\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Операція остаточно провалилася: {ex.Message}\n");
            }

            Console.WriteLine("=== Тестування NetworkClient (SendEmail) ===");
            try
            {
                RetryHelper.ExecuteWithRetry(
                    () => networkClient.SendEmail("user@example.com", "Тема", "Тіло листа"),
                    retryCount: 3,
                    initialDelay: TimeSpan.FromMilliseconds(500),
                    shouldRetry: shouldRetryCondition
                );
                Console.WriteLine("Відправка завершена успішно.\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Операція остаточно провалилася: {ex.Message}\n");
            }
        }
    }
}