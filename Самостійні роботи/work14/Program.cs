using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace IndependentWork14
{
    public class FileMetadata
    {
        public string FileName { get; set; }
        public long Size { get; set; }
        public DateTime LastModified { get; set; }
        public string Hash { get; set; }

        public override string ToString()
        {
            return $"{FileName} ({Size} bytes, Modified: {LastModified:yyyy-MM-dd HH:mm}, Hash: {Hash})";
        }
    }

    public class FileMetadataCache
    {
        private readonly Dictionary<string, FileMetadata> _cache;

        public FileMetadataCache()
        {
            _cache = new Dictionary<string, FileMetadata>(StringComparer.OrdinalIgnoreCase);
        }

        public void Add(string path, FileMetadata metadata)
        {
            _cache[path] = metadata;
        }

        public FileMetadata Get(string path)
        {
            return _cache.TryGetValue(path, out var metadata) ? metadata : null;
        }

        public bool Remove(string path)
        {
            return _cache.Remove(path);
        }

        public bool Contains(string path)
        {
            return _cache.ContainsKey(path);
        }

        public int Count => _cache.Count;
    }

    public class TagManager
    {
        private readonly Dictionary<string, HashSet<string>> _articleTags;

        public TagManager()
        {
            _articleTags = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        }

        public void AddTag(string articleId, string tag)
        {
            if (!_articleTags.ContainsKey(articleId))
            {
                _articleTags[articleId] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }
            _articleTags[articleId].Add(tag);
        }

        public void RemoveTag(string articleId, string tag)
        {
            if (_articleTags.TryGetValue(articleId, out var tags))
            {
                tags.Remove(tag);
                if (tags.Count == 0)
                {
                    _articleTags.Remove(articleId);
                }
            }
        }

        public HashSet<string> GetAllUniqueTags()
        {
            var allTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var tags in _articleTags.Values)
            {
                allTags.UnionWith(tags);
            }
            return allTags;
        }

        public HashSet<string> FindCommonTags(string articleId1, string articleId2)
        {
            if (!_articleTags.TryGetValue(articleId1, out var tags1) ||
                !_articleTags.TryGetValue(articleId2, out var tags2))
            {
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }

            var common = new HashSet<string>(tags1, StringComparer.OrdinalIgnoreCase);
            common.IntersectWith(tags2);
            return common;
        }

        public HashSet<string> FindUniqueTagsForFirst(string articleId1, string articleId2)
        {
            if (!_articleTags.TryGetValue(articleId1, out var tags1))
            {
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }

            var diff = new HashSet<string>(tags1, StringComparer.OrdinalIgnoreCase);
            if (_articleTags.TryGetValue(articleId2, out var tags2))
            {
                diff.ExceptWith(tags2);
            }
            return diff;
        }
    }

    public class SearchIndex
    {
        private readonly Dictionary<string, HashSet<int>> _invertedIndex;

        public SearchIndex()
        {
            _invertedIndex = new Dictionary<string, HashSet<int>>(StringComparer.OrdinalIgnoreCase);
        }

        public void AddDocument(int docId, string content)
        {
            var words = Regex.Split(content.ToLowerInvariant(), @"\W+")
                             .Where(w => !string.IsNullOrWhiteSpace(w));

            foreach (var word in words)
            {
                if (!_invertedIndex.ContainsKey(word))
                {
                    _invertedIndex[word] = new HashSet<int>();
                }
                _invertedIndex[word].Add(docId);
            }
        }

        public HashSet<int> Search(string word)
        {
            if (_invertedIndex.TryGetValue(word, out var docIds))
            {
                return new HashSet<int>(docIds);
            }
            return new HashSet<int>();
        }

        public HashSet<int> SearchMultiple(params string[] words)
        {
            if (words == null || words.Length == 0)
            {
                return new HashSet<int>();
            }

            HashSet<int> result = null;

            foreach (var word in words)
            {
                if (!_invertedIndex.TryGetValue(word, out var docIds))
                {
                    return new HashSet<int>();
                }

                if (result == null)
                {
                    result = new HashSet<int>(docIds);
                }
                else
                {
                    result.IntersectWith(docIds);
                }
            }

            return result ?? new HashSet<int>();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            Console.WriteLine("=== Сценарій 1: FileMetadataCache (Dictionary) ===");
            var cache = new FileMetadataCache();
            cache.Add("C:/projects/app.cs", new FileMetadata
            {
                FileName = "app.cs",
                Size = 2048,
                LastModified = DateTime.Now.AddHours(-2),
                Hash = "A1B2C3D4"
            });
            cache.Add("C:/projects/readme.md", new FileMetadata
            {
                FileName = "readme.md",
                Size = 512,
                LastModified = DateTime.Now,
                Hash = "E5F6G7H8"
            });

            Console.WriteLine($"Чи є в кеші C:/PROJECTS/APP.CS: {cache.Contains("C:/PROJECTS/APP.CS")}");
            Console.WriteLine($"Отримано: {cache.Get("c:/projects/app.cs")}");
            cache.Remove("C:/projects/readme.md");
            Console.WriteLine($"Кількість елементів після видалення: {cache.Count}");

            Console.WriteLine("\n=== Сценарій 2: TagManager (Dictionary + HashSet) ===");
            var tagManager = new TagManager();
            tagManager.AddTag("post-1", "C#");
            tagManager.AddTag("post-1", "dotnet");
            tagManager.AddTag("post-1", "collections");
            tagManager.AddTag("post-1", "c#");

            tagManager.AddTag("post-2", "dotnet");
            tagManager.AddTag("post-2", "performance");
            tagManager.AddTag("post-2", "collections");

            Console.WriteLine($"Усі унікальні теги: {string.Join(", ", tagManager.GetAllUniqueTags())}");
            Console.WriteLine($"Спільні теги (post-1 та post-2): {string.Join(", ", tagManager.FindCommonTags("post-1", "post-2"))}");
            Console.WriteLine($"Унікальні для post-1 (ExceptWith): {string.Join(", ", tagManager.FindUniqueTagsForFirst("post-1", "post-2"))}");

            tagManager.RemoveTag("post-1", "collections");
            Console.WriteLine($"Спільні теги після видалення 'collections' з post-1: {string.Join(", ", tagManager.FindCommonTags("post-1", "post-2"))}");

            Console.WriteLine("\n=== Сценарій 3: SearchIndex (Інвертований індекс) ===");
            var searchIndex = new SearchIndex();
            searchIndex.AddDocument(1, "C# Dictionary and HashSet provide O(1) lookup.");
            searchIndex.AddDocument(2, "HashSet stores unique elements in .NET collections.");
            searchIndex.AddDocument(3, "Dictionary maps keys to values in C#.");

            Console.WriteLine($"Документи зі словом 'C#': [{string.Join(", ", searchIndex.Search("C#"))}]");
            Console.WriteLine($"Документи зі словами 'HashSet' та 'collections': [{string.Join(", ", searchIndex.SearchMultiple("HashSet", "collections"))}]");
            Console.WriteLine($"Документи зі словами 'C#' та 'HashSet': [{string.Join(", ", searchIndex.SearchMultiple("C#", "HashSet"))}]");
        }
    }
}