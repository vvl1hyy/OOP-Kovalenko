namespace lab1v10;

class Program
{
    static void Main(string[] args)
    {
        Computer pc1 = new Computer("Intel Core i5-12400F", 16, 512);
        Computer pc2 = new Computer("AMD Ryzen 7 7800X3D", 32, 1000);
        Computer pc3 = new Computer("Apple M2", 8, 256);

        pc1.RunBenchmark();
        pc2.RunBenchmark();
        pc3.RunBenchmark();
    }
}