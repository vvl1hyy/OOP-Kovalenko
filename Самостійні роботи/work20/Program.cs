using System;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace IndependentWork14
{
    public class RetryPolicy
    {
        private readonly int _maxRetries;
        private readonly TimeSpan _initialDelay;
        private readonly double _jitterFactor;
        private readonly Random _random;

        public RetryPolicy(int maxRetries, TimeSpan initialDelay, double jitterFactor = 0.2)
        {
            if (maxRetries < 0)
                throw new ArgumentOutOfRangeException(nameof(maxRetries));
            if (_initialDelay < TimeSpan.Zero && initialDelay < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(initialDelay));
            if (jitterFactor < 0 || jitterFactor >= 1.0)
                throw new ArgumentOutOfRangeException(nameof(jitterFactor));

            _maxRetries = maxRetries;
            _initialDelay = initialDelay;
            _jitterFactor = jitterFactor;
            _random = new Random();
        }

        public T Execute<T>(Func<T> operation, Func<Exception, bool> shouldRetry = null)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            int attempt = 0;

            while (true)
            {
                try
                {
                    Console.WriteLine($"  [Спроба #{attempt + 1}] Виконання операції...");
                    T result = operation();
                    Console.WriteLine($"  [Спроба #{attempt + 1}] Успішно.");
                    return result;
                }
                catch (Exception ex)
                {
                    bool canRetry = attempt < _maxRetries && (shouldRetry == null || shouldRetry(ex));
                    Console.WriteLine($"  [Спроба #{attempt + 1}] Невдача. Причина: {ex.GetType().Name} - {ex.Message}");

                    if (!canRetry)
                    {
                        Console.WriteLine("  [Політика Retry] Повторні спроби вичерпані або не дозволені. Прокидаємо виняток.");
                        throw;
                    }

                    double baseDelayMs = _initialDelay.TotalMilliseconds * Math.Pow(2, attempt);
                    double jitterMultiplier = 1.0 + _random.NextDouble() * _jitterFactor * 2.0 - _jitterFactor;
                    double finalDelayMs = Math.Max(0, baseDelayMs * jitterMultiplier);
                    TimeSpan delay = TimeSpan.FromMilliseconds(finalDelayMs);

                    Console.WriteLine($"  [Очікування] Затримка перед повтором #{attempt + 2}: {delay.TotalMilliseconds:F1} мс");
                    Thread.Sleep(delay);
                    attempt++;
                }
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            var policy = new RetryPolicy(
                maxRetries: 3,
                initialDelay: TimeSpan.FromMilliseconds(300),
                jitterFactor: 0.2);

            Func<Exception, bool> isTransientError = ex =>
                ex is HttpRequestException || ex is TimeoutException;

            Console.WriteLine("=== Сценарій 1: Операція проходить після кількох невдалих спроб ===");
            int flakyAttempt = 0;
            try
            {
                string response = policy.Execute(() =>
                {
                    flakyAttempt++;
                    if (flakyAttempt < 3)
                    {
                        throw new HttpRequestException($"Тимчасовий збій мережі на виклику {flakyAttempt}");
                    }
                    return "DATA_OK_200";
                }, isTransientError);

                Console.WriteLine($"Результат сценарію 1: {response}\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Сценарій 1 завершився помилкою: {ex.Message}\n");
            }

            Console.WriteLine("=== Сценарій 2: Операція завжди завершується невдачею ===");
            try
            {
                int alwaysFail = policy.Execute<int>(() =>
                {
                    throw new TimeoutException("Сервіс не відповідає в межах таймауту");
                }, isTransientError);

                Console.WriteLine($"Результат сценарію 2: {alwaysFail}\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Спіймано фінальний виняток після всіх спроб: {ex.GetType().Name} - {ex.Message}\n");
            }
        }
    }
}