using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using Polly;
using Polly.CircuitBreaker;
using Polly.Fallback;
using Polly.Retry;
using Polly.Timeout;

namespace IndependentWork19
{
    public class InsufficientFundsException : Exception
    {
        public decimal RequestedAmount { get; }
        public decimal CurrentBalance { get; }

        public InsufficientFundsException()
        {
        }

        public InsufficientFundsException(string message) : base(message)
        {
        }

        public InsufficientFundsException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public InsufficientFundsException(string message, decimal requestedAmount, decimal currentBalance)
            : base(message)
        {
            RequestedAmount = requestedAmount;
            CurrentBalance = currentBalance;
        }
    }

    public sealed class Result<T>
    {
        public bool IsSuccess { get; }
        public T Value { get; }
        public string Error { get; }

        private Result(bool isSuccess, T value, string error)
        {
            IsSuccess = isSuccess;
            Value = value;
            Error = error;
        }

        public static Result<T> Success(T value) => new Result<T>(true, value, string.Empty);
        public static Result<T> Failure(string error) => new Result<T>(false, default!, error);
    }

    public static class BankAccountDemo
    {
        public static Result<decimal> WithdrawSafe(decimal balance, decimal amount)
        {
            if (amount <= 0)
            {
                return Result<decimal>.Failure("Сума зняття має бути більшою за нуль.");
            }

            if (amount > balance)
            {
                return Result<decimal>.Failure($"Недостатньо коштів: запитано {amount}, доступно {balance}.");
            }

            return Result<decimal>.Success(balance - amount);
        }

        public static void WithdrawOrThrow(decimal balance, decimal amount)
        {
            if (balance < 0)
            {
                throw new InvalidOperationException("Рахунок заблоковано через від'ємний баланс.");
            }

            if (amount > balance)
            {
                throw new InsufficientFundsException(
                    "Недостатньо коштів на рахунку для проведення операції.",
                    amount,
                    balance);
            }
        }

        public static void Run()
        {
            Console.WriteLine("=== Частина 1: Власний виняток та try-catch-finally ===");

            var res = WithdrawSafe(500m, 800m);
            Console.WriteLine($"Result підхід: IsSuccess={res.IsSuccess}, Error={res.Error}");

            try
            {
                WithdrawOrThrow(500m, 800m);
            }
            catch (InsufficientFundsException ex)
            {
                Console.WriteLine($"Перехоплено {nameof(InsufficientFundsException)}: {ex.Message}");
                Console.WriteLine($"  Запитано: {ex.RequestedAmount} грн, Баланс: {ex.CurrentBalance} грн");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Загальна помилка: {ex.Message}");
            }
            finally
            {
                Console.WriteLine("  [finally] Лог транзакції зафіксовано.\n");
            }
        }
    }

    public static class PollyScenariosDemo
    {
        public static void RunExternalApiRetryWithTimeout()
        {
            Console.WriteLine("=== Сценарій 1: Зовнішній API (Retry + Timeout) ===");

            var pipeline = new ResiliencePipelineBuilder<string>()
                .AddTimeout(TimeSpan.FromSeconds(2))
                .AddRetry(new RetryStrategyOptions<string>
                {
                    ShouldHandle = args =>
                        new ValueTask<bool>(args.Outcome.Exception is HttpRequestException or TimeoutRejectedException),
                    MaxRetryAttempts = 3,
                    Delay = TimeSpan.FromMilliseconds(300),
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true,
                    OnRetry = args =>
                    {
                        Console.WriteLine(
                            $"  [Retry #{args.AttemptNumber + 1}] Затримка {args.RetryDelay.TotalMilliseconds:F0} мс. Причина: {args.Outcome.Exception?.GetType().Name}");
                        return default;
                    }
                })
                .Build();

            int attempt = 0;

            try
            {
                pipeline.Execute(_ =>
                {
                    attempt++;
                    Console.WriteLine($"  Виклик зовнішнього API (спроба #{attempt})...");

                    if (attempt == 1)
                    {
                        throw new HttpRequestException("Тимчасова помилка 503 Service Unavailable.");
                    }

                    if (attempt == 2)
                    {
                        Thread.Sleep(2500);
                    }

                    Console.WriteLine("  Успіх: відповідь від зовнішнього API отримано.");
                    return "OK";
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Фінальна помилка після спроб: {ex.GetType().Name} - {ex.Message}");
            }

            Console.WriteLine();
        }

        public static void RunPaymentGatewayCircuitBreaker()
        {
            Console.WriteLine("=== Сценарій 2: Платіжний шлюз (Circuit Breaker) ===");

            var breakerOptions = new CircuitBreakerStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<HttpRequestException>(),
                FailureRatio = 0.5,
                MinimumThroughput = 2,
                SamplingDuration = TimeSpan.FromSeconds(10),
                BreakDuration = TimeSpan.FromSeconds(2),
                OnOpened = args =>
                {
                    Console.WriteLine($"  [Circuit Breaker -> OPEN] Шлюз заблоковано на {args.BreakDuration.TotalSeconds}с через збої.");
                    return default;
                },
                OnClosed = _ =>
                {
                    Console.WriteLine("  [Circuit Breaker -> CLOSED] Роботу шлюзу відновлено.");
                    return default;
                },
                OnHalfOpened = _ =>
                {
                    Console.WriteLine("  [Circuit Breaker -> HALF-OPEN] Перевірочний запит до шлюзу...");
                    return default;
                }
            };

            var pipeline = new ResiliencePipelineBuilder()
                .AddCircuitBreaker(breakerOptions)
                .Build();

            for (int i = 1; i <= 5; i++)
            {
                try
                {
                    Console.WriteLine($"  Запит #{i} до платіжного шлюзу...");
                    pipeline.Execute(_ =>
                    {
                        if (i <= 2)
                        {
                            throw new HttpRequestException("Платіжний шлюз не відповідає.");
                        }

                        Console.WriteLine($"  Запит #{i} виконано успішно.");
                    });
                }
                catch (BrokenCircuitException)
                {
                    Console.WriteLine($"  Запит #{i} відхилено миттєво: коло розімкнене (BrokenCircuitException).");
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"  Запит #{i} завершився помилкою: {ex.Message}");
                }

                if (i == 3)
                {
                    Console.WriteLine("  Очікування 2.1с для переходу Circuit Breaker у Half-Open...");
                    Thread.Sleep(2100);
                }
            }

            Console.WriteLine();
        }

        public static void RunQueueFallbackScenario()
        {
            Console.WriteLine("=== Сценарій 3: Черга повідомлень (Retry + Fallback) ===");

            var fallbackMessages = new Queue<string>();

            var pipeline = new ResiliencePipelineBuilder<string>()
                .AddFallback(new FallbackStrategyOptions<string>
                {
                    ShouldHandle = new PredicateBuilder<string>().Handle<InvalidOperationException>(),
                    FallbackAction = args =>
                    {
                        string fallbackMsg = "QUEUED_LOCALLY_FALLBACK";
                        fallbackMessages.Enqueue(fallbackMsg);
                        Console.WriteLine($"  [Fallback] Черга перевантажена. Повідомлення збережено локально ({fallbackMsg}).");
                        return Outcome.FromResultAsValueTask(fallbackMsg);
                    }
                })
                .AddRetry(new RetryStrategyOptions<string>
                {
                    ShouldHandle = new PredicateBuilder<string>().Handle<InvalidOperationException>(),
                    MaxRetryAttempts = 2,
                    Delay = TimeSpan.FromMilliseconds(200),
                    OnRetry = args =>
                    {
                        Console.WriteLine($"  [Retry #{args.AttemptNumber + 1}] Повторна спроба відправки в чергу...");
                        return default;
                    }
                })
                .Build();

            string status = pipeline.Execute<string>(_ =>
            {
                throw new InvalidOperationException("Брокер черги перевантажений (429 Too Many Requests).");
            });

            Console.WriteLine($"  Підсумковий статус операції: {status} (у локальній черзі: {fallbackMessages.Count})\n");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            BankAccountDemo.Run();
            PollyScenariosDemo.RunExternalApiRetryWithTimeout();
            PollyScenariosDemo.RunPaymentGatewayCircuitBreaker();
            PollyScenariosDemo.RunQueueFallbackScenario();
        }
    }
}