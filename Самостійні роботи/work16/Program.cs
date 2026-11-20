using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace InvertedIndexDemo
{
    public class Document
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }

        public override string ToString() => $"[{Id}] {Title}";
    }

    public static class TextProcessor
    {
        private static readonly HashSet<string> StopWords = new HashSet<string>
        {
            "a", "an", "the", "is", "are", "was", "were",
            "in", "on", "at", "to", "for", "of", "and", "or"
        };

        public static IEnumerable<string> Tokenize(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return Enumerable.Empty<string>();
            }

            return Regex.Matches(text.ToLowerInvariant(), @"\w+")
                        .Select(m => m.Value);
        }

        public static IEnumerable<string> RemoveStopWords(IEnumerable<string> words)
        {
            return words.Where(w => !StopWords.Contains(w));
        }
    }

    public class InvertedIndex
    {
        private readonly Dictionary<string, HashSet<int>> _index;
        private readonly Dictionary<int, Document> _documents;

        public InvertedIndex()
        {
            _index = new Dictionary<string, HashSet<int>>(StringComparer.OrdinalIgnoreCase);
            _documents = new Dictionary<int, Document>();
        }

        public void AddDocument(Document doc)
        {
            if (doc == null) return;

            _documents[doc.Id] = doc;

            var tokens = TextProcessor.RemoveStopWords(TextProcessor.Tokenize(doc.Content));

            foreach (var word in tokens)
            {
                if (!_index.TryGetValue(word, out var docIds))
                {
                    docIds = new HashSet<int>();
                    _index[word] = docIds;
                }
                docIds.Add(doc.Id);
            }
        }

        public List<Document> SearchSingleWord(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                return new List<Document>();
            }

            string normalized = word.Trim().ToLowerInvariant();
            if (!_index.TryGetValue(normalized, out var docIds) || docIds.Count == 0)
            {
                return new List<Document>();
            }

            return docIds.Where(id => _documents.ContainsKey(id))
                         .Select(id => _documents[id])
                         .ToList();
        }

        public List<Document> SearchMultipleWords(params string[] words)
        {
            if (words == null || words.Length == 0)
            {
                return new List<Document>();
            }

            var normalizedWords = words
                .Where(w => !string.IsNullOrWhiteSpace(w))
                .Select(w => w.Trim().ToLowerInvariant())
                .Distinct()
                .ToList();

            if (normalizedWords.Count == 0)
            {
                return new List<Document>();
            }

            HashSet<int> resultSet = null;

            foreach (var word in normalizedWords)
            {
                if (!_index.TryGetValue(word, out var docIds))
                {
                    return new List<Document>();
                }

                if (resultSet == null)
                {
                    resultSet = new HashSet<int>(docIds);
                }
                else
                {
                    resultSet.IntersectWith(docIds);
                }

                if (resultSet.Count == 0)
                {
                    return new List<Document>();
                }
            }

            return resultSet.Where(id => _documents.ContainsKey(id))
                            .Select(id => _documents[id])
                            .ToList();
        }

        public List<Document> SearchAnyWord(params string[] words)
        {
            if (words == null || words.Length == 0)
            {
                return new List<Document>();
            }

            var unionSet = new HashSet<int>();

            foreach (var rawWord in words)
            {
                if (string.IsNullOrWhiteSpace(rawWord)) continue;

                string word = rawWord.Trim().ToLowerInvariant();
                if (_index.TryGetValue(word, out var docIds))
                {
                    unionSet.UnionWith(docIds);
                }
            }

            return unionSet.Where(id => _documents.ContainsKey(id))
                           .Select(id => _documents[id])
                           .ToList();
        }

        public int DocumentCount => _documents.Count;
        public int UniqueWords => _index.Keys.Count;
    }

    public static class PerformanceTest
    {
        public static List<Document> GenerateDocuments(List<Document> baseDocs, int targetCount)
        {
            var generated = new List<Document>(targetCount);
            string[] extraWords = { "collections", "generics", "index", "search", "performance", "dotnet", "algorithm" };
            var rng = new Random(42);

            for (int i = 0; i < targetCount; i++)
            {
                var template = baseDocs[i % baseDocs.Count];
                string extra = extraWords[rng.Next(extraWords.Length)];
                generated.Add(new Document
                {
                    Id = i + 1,
                    Title = $"{template.Title} #{i + 1}",
                    Content = $"{template.Content} {extra}."
                });
            }

            return generated;
        }

        public static void CompareSearchMethods(List<Document> documents, string searchWord)
        {
            string normalizedWord = searchWord.Trim().ToLowerInvariant();

            var sw = new Stopwatch();
            sw.Start();
            var linearResults = documents
                .Where(d => TextProcessor.Tokenize(d.Content).Contains(normalizedWord))
                .ToList();
            sw.Stop();
            long linearTicks = sw.ElapsedTicks;

            var index = new InvertedIndex();
            foreach (var doc in documents)
            {
                index.AddDocument(doc);
            }

            sw.Restart();
            var indexResults = index.SearchSingleWord(searchWord);
            sw.Stop();
            long indexTicks = sw.ElapsedTicks;

            Console.WriteLine($"Кількість документів: {documents.Count} | Слово: '{searchWord}'");
            Console.WriteLine($"  Linear search:  {linearTicks} ticks (знайдено: {linearResults.Count})");
            Console.WriteLine($"  Inverted index: {indexTicks} ticks (знайдено: {indexResults.Count})");
        }
    }

    class Program
    {
        static List<Document> GetBaseDocuments()
        {
            return new List<Document>
            {
                new Document
                {
                    Id = 1,
                    Title = "Introduction to C#",
                    Content = "C# is a modern object-oriented programming language developed by Microsoft."
                },
                new Document
                {
                    Id = 2,
                    Title = "Java Basics",
                    Content = "Java is a popular programming language known for its portability and performance."
                },
                new Document
                {
                    Id = 3,
                    Title = "Python Overview",
                    Content = "Python is a high-level programming language with simple syntax."
                },
                new Document
                {
                    Id = 4,
                    Title = "C# Advanced Features",
                    Content = "C# supports LINQ, async/await, and many modern programming paradigms."
                },
                new Document
                {
                    Id = 5,
                    Title = "Comparing Languages",
                    Content = "C#, Java, and Python are among the most popular programming languages today."
                }
            };
        }

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            var baseDocs = GetBaseDocuments();
            var index = new InvertedIndex();

            foreach (var doc in baseDocs)
            {
                index.AddDocument(doc);
            }

            Console.WriteLine($"Завантажено документів: {index.DocumentCount}");
            Console.WriteLine($"Унікальних слів в індексі: {index.UniqueWords}\n");

            Console.WriteLine("=== Пошук одного слова ('programming') ===");
            foreach (var d in index.SearchSingleWord("programming"))
            {
                Console.WriteLine($"  {d}");
            }

            Console.WriteLine("\n=== Пошук за ВСІМА словами (AND: 'c#', 'programming') ===");
            foreach (var d in index.SearchMultipleWords("c#", "programming"))
            {
                Console.WriteLine($"  {d}");
            }

            Console.WriteLine("\n=== Пошук за БУДЬ-ЯКИМ словом (OR: 'python', 'linq') ===");
            foreach (var d in index.SearchAnyWord("python", "linq"))
            {
                Console.WriteLine($"  {d}");
            }

            Console.WriteLine("\n=== Вимірювання продуктивності ===");
            var docs10 = PerformanceTest.GenerateDocuments(baseDocs, 10);
            var docs1000 = PerformanceTest.GenerateDocuments(baseDocs, 1000);

            PerformanceTest.CompareSearchMethods(docs10, "programming");
            PerformanceTest.CompareSearchMethods(docs1000, "programming");
        }
    }
}