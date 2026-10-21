using System;

class Building
{
    public string Address { get; set; }
    public int YearBuilt { get; set; }

    public Building(string address, int yearBuilt)
    {
        Address = address;
        YearBuilt = yearBuilt;
    }

    public virtual void GetInfo()
    {
        Console.WriteLine($"Building at {Address}, built in {YearBuilt}.");
    }

    public string GetBuildingType()
    {
        return "Generic Building";
    }
}

class House : Building
{
    public int NumFloors { get; set; }

    public House(string address, int yearBuilt, int numFloors) : base(address, yearBuilt)
    {
        NumFloors = numFloors;
    }

    public override void GetInfo()
    {
        Console.WriteLine($"House at {Address}, built in {YearBuilt}, with {NumFloors} floors.");
    }

    public void OpenDoor()
    {
        Console.WriteLine("The front door of the house is now open.");
    }

    public new string GetBuildingType()
    {
        return "House";
    }
}

class Skyscraper : Building
{
    public int NumElevators { get; set; }

    public Skyscraper(string address, int yearBuilt, int numElevators) : base(address, yearBuilt)
    {
        NumElevators = numElevators;
    }

    public override void GetInfo()
    {
        Console.WriteLine($"Skyscraper at {Address}, built in {YearBuilt}, featuring {NumElevators} elevators.");
    }

    public void GoToRoof()
    {
        Console.WriteLine("Taking the elevator to the skyscraper's roof.");
    }
}

class Program
{
    static void Main()
    {
        House myHouse = new House("Вулиця Соборна, 15", 2010, 2);
        Skyscraper mySkyscraper = new Skyscraper("Бізнес-центр", 2024, 8);

        Building houseAsBuilding = myHouse;
        Building skyscraperAsBuilding = mySkyscraper;

        myHouse.GetInfo();
        houseAsBuilding.GetInfo();
        
        mySkyscraper.GetInfo();
        skyscraperAsBuilding.GetInfo();

        myHouse.OpenDoor();
        mySkyscraper.GoToRoof();

        Console.WriteLine(myHouse.GetBuildingType());
        Console.WriteLine(houseAsBuilding.GetBuildingType());
    }
}