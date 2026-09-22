namespace lab1v10;

public class Computer
{
    private string cpu;
    private int ram;

    public int Storage { get; set; }

    public Computer(string cpu, int ram, int storage)
    {
        this.cpu = cpu;
        this.ram = ram;
        Storage = storage;
    }

    public void RunBenchmark()
    {
        int score = ram * 100 + Storage * 2;
        Console.WriteLine($"[Benchmark] CPU: {cpu} | RAM: {ram}GB | Storage: {Storage}GB -> Performance Score: {score} pts");
    }
}