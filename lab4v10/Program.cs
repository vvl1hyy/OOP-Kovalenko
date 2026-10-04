using System;

namespace lab4v10
{
    public class Building
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
            Console.WriteLine($"Будівля за адресою: {Address}, рік побудови: {YearBuilt}");
        }

        public string GetBuildingType()
        {
            return "Базовий тип: Будівля";
        }
    }

    public class House : Building
    {
        public int NumFloors { get; set; }

        public House(string address, int yearBuilt, int numFloors) : base(address, yearBuilt)
        {
            NumFloors = numFloors;
        }

        public override void GetInfo()
        {
            Console.WriteLine($"Будинок за адресою: {Address}, рік побудови: {YearBuilt}, поверхів: {NumFloors}");
        }

        public void OpenDoor()
        {
            Console.WriteLine("Двері будинку відчинено.");
        }

        public new string GetBuildingType()
        {
            return "Похідний тип: Житловий будинок";
        }
    }

    public class Skyscraper : Building
    {
        public int NumElevators { get; set; }

        public Skyscraper(string address, int yearBuilt, int numElevators) : base(address, yearBuilt)
        {
            NumElevators = numElevators;
        }

        public override void GetInfo()
        {
            Console.WriteLine($"Хмарочос за адресою: {Address}, рік побудови: {YearBuilt}, ліфтів: {NumElevators}");
        }

        public void GoToRoof()
        {
            Console.WriteLine("Підйом на дах хмарочоса.");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Building b = new Building("вул. Київська, 1", 1995);
            House h = new House("вул. Соборна, 15", 2010, 9);
            Skyscraper s = new Skyscraper("вул. Центральна, 100", 2022, 6);

            Console.WriteLine("Поліморфні виклики GetInfo():");
            Building ref1 = h;
            Building ref2 = s;

            b.GetInfo();
            ref1.GetInfo();
            ref2.GetInfo();

            Console.WriteLine("\nДемонстрація override та new:");
            Console.WriteLine(h.GetBuildingType());
            Building ref3 = h;
            Console.WriteLine(ref3.GetBuildingType());

            Console.WriteLine("\nВиклик унікальних методів:");
            h.OpenDoor();
            s.GoToRoof();
        }
    }
}