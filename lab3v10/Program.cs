using System;

namespace lab3v10
{
    public class ImageBuffer : IDisposable
    {
        private bool _disposed = false;
        private int _width;
        private int _height;
        private bool _isAllocated;

        public int Width => _width;
        public int Height => _height;

        public ImageBuffer(int width, int height)
        {
            _width = width;
            _height = height;
            _isAllocated = true;
            Console.WriteLine($"Буфер зображення {_width}x{_height} виділено в пам'яті.");
        }

        public void DrawPixel(int x, int y)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ImageBuffer));
            }

            if (!_isAllocated)
            {
                Console.WriteLine("Буфер не виділено.");
                return;
            }

            Console.WriteLine($"Намальовано піксель на координатах ({x}, {y}).");
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Console.WriteLine("Звільнення керованих ресурсів буфера зображення.");
                }

                if (_isAllocated)
                {
                    Console.WriteLine("Звільнення некерованого буфера зображення.");
                    _isAllocated = false;
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~ImageBuffer()
        {
            Dispose(false);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Сценарій 1: Використання через using");
            using (var buffer1 = new ImageBuffer(1920, 1080))
            {
                buffer1.DrawPixel(100, 200);
            }
            Console.WriteLine();

            Console.WriteLine("Сценарій 2: Явний виклик Dispose");
            var buffer2 = new ImageBuffer(800, 600);
            buffer2.DrawPixel(50, 50);
            buffer2.Dispose();
            Console.WriteLine();

            Console.WriteLine("Сценарій 3: Робота деструктора через GC");
            CreateAndAbandonBuffer();

            Console.WriteLine("Очікування збирання сміття...");
            GC.Collect();
            GC.WaitForPendingFinalizers();

            Console.WriteLine("Кінець програми.");
        }

        static void CreateAndAbandonBuffer()
        {
            var buffer3 = new ImageBuffer(640, 480);
            buffer3.DrawPixel(10, 10);
        }
    }
}