using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceStation
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ServiceStation serviceStation = new ServiceStation();
            serviceStation.Work();
        }
    }

    class ServiceStation
    {
        private Stockroom _stockroom = new Stockroom();
        private Queue<Car> _cars = new Queue<Car>();
        private int _carsCount = 10;
        private int _money = 10000;
        private int _penalty = 5000;

        public ServiceStation()
        {
            for (int i = 0; i < _carsCount; i++)
            {
                _cars.Enqueue(new Car());
            }
        }

        public void Work()
        {
            bool isWork = true;

            while (isWork)
            {
                Console.Clear();
                RepairCar();

                if (_cars.Count == 0 || _money <=0)
                {
                    isWork = false;
                }   
            }
        }

        private void RepairCar()
        {
            Car car = _cars.Dequeue();
            bool isContinue = false;

            while (isContinue == false)
            {
                ShowInfo(car);                
                int index = GetIndex();

                if (_stockroom.ReduceDetailNumber(index))
                {
                    СhangeDitail(car, index);
                }
                else
                {
                    Console.WriteLine($"Детали закончились. Вы платите штраф {_penalty} руб.");
                    car.OutOfStock(index);
                    _money -= _penalty;
                }

                if (car.IsAllWorking() || IsMoneyRanOut())
                {
                    isContinue = true;
                }  

                Console.ReadKey(true);
            }
        }

        private void СhangeDitail(Car car, int index)
        {
            if (car.СhangeDitail(index))
            {
                _money += _stockroom.GetWorkPrice(index);
            }
            else
            {
                Console.WriteLine($"Вы установили не ту деталь. " +
                    $"В качестве компенсации вы возвращаете старые детали на место и платите штраф {_penalty} руб.");
                _stockroom.IncreaseDetailNumber(index);
                _money -= _penalty;
            }
        }

        private bool IsMoneyRanOut()
        {
            if (_money < 0)
            {
                Console.Clear();
                Console.WriteLine("Вы обанкротились");
                return true;
            }
            else
            {
                return false;
            }
        }

        private void ShowInfo(Car car)
        {
            Console.Clear();
            Console.WriteLine($"На счету автосервиса {_money} руб.");
            car.ShowInfo();
            _stockroom.ShowInfo();
        }

        private int GetIndex()
        {
            int index = 0;
            bool success = false;
            bool isCorrect = false;

            while (isCorrect == false)
            {
                Console.WriteLine("\nВыберете номер детали которую вы хотите установить");
                string userInput = Console.ReadLine();
                success = int.TryParse(userInput, out index);

                if (success && index > 0 && index <= _stockroom.DetailsCount)
                {
                    isCorrect = true;
                }
                else
                {
                    Console.WriteLine("Такой детали нет");
                }
            }

            return index - 1;
        }
    }



    class Range
    {
        protected static Random Random = new Random();
        protected List<Detail> Details = new List<Detail>();

        public int DetailsCount { get { return Details.Count; } }
        public Range()
        {
            Details.Add(new Detail("Масляный фильтр", 400, 500));
            Details.Add(new Detail("Топливный фильтр", 500, 500));
            Details.Add(new Detail("Ремень ГРМ", 500, 500));
            Details.Add(new Detail("Тормозные колодки", 1500, 1000));
            Details.Add(new Detail("Тормозной диск", 1600, 1000));
            Details.Add(new Detail("Лобовое стекло", 7000, 1000));
            Details.Add(new Detail("Комплект сцепления", 5000, 3000));
            Details.Add(new Detail("Генератор в сборе", 8000, 3000));
            Details.Add(new Detail("Комплект покрышек", 20000, 3000));
        }
    }

    class Car : Range
    {
        private int numberFaultsMin = 1;
        private int numberFaultsMax = 5;

        public Car()
        {
            int faultsCount = Random.Next(numberFaultsMin, numberFaultsMax);

            for (int i = 0; i < faultsCount; i++)
            {
                int indexFault = Random.Next(Details.Count);
                Details[indexFault].SetWorkingStatus(false);
            }
        }

        public bool СhangeDitail(int index)
        {
            bool isRight = !Details[index].IsWorking;
            Details[index].SetWorkingStatus(true);
            return isRight;
        }

        public bool IsAllWorking()
        {
            for (int i = 0; i < Details.Count; i++)
            {
                if (Details[i].IsWorking == false && Details[i].IsInStock == true)
                {
                    return false;
                }
            }

            return true;
        }

        public void OutOfStock(int index)
        {
            Details[index].IsOutOfStock();
        }

        public void ShowInfo()
        {
            int positionX = 0;
            int positionY = 2;
            Console.SetCursorPosition(positionX, positionY);
            Console.WriteLine("На обслуживание поступил автомобиль:");

            for (int i = 0; i < Details.Count; i++)
            {
                if (Details[i].IsWorking == false && Details[i].IsInStock == true)
                {
                    Console.WriteLine($"Требует замены {Details[i].Name}.");
                }

                if (Details[i].IsInStock == false)
                {
                    Console.WriteLine($"Нет на складе {Details[i].Name}");
                }
            }
        }
    }

    class Stockroom : Range
    {
        private int _minDetailCount = 1;
        private int _maxDetailCount = 10;
        private List<Stack> _stacks = new List<Stack>();

        public Stockroom()
        {
            foreach (Detail detail in Details)
            {
                _stacks.Add(new Stack(detail, Random.Next(_minDetailCount, _maxDetailCount + 1)));
            }
        }

        public int GetWorkPrice(int index)
        {
            Console.WriteLine($"Вы получили с клиента {Details[index].Price} руб. за деталь " +
                $"и {Details[index].InstallationPrice} руб. за работу");
            return Details[index].Price + Details[index].InstallationPrice;
        }

        public bool ReduceDetailNumber(int index)
        {
            if (_stacks[index].Quantity == 0)
            {
                return false;
            }
            else
            {
                _stacks[index].ReduceQuantity();
                return true;
            }
        }

        public void IncreaseDetailNumber(int index)
        {
            _stacks[index].IncreaseQuantity();
        }

        public void ShowInfo()
        {
            int positionX = 38;
            int positionY = 2;
            Console.SetCursorPosition(positionX, positionY++);
            Console.WriteLine("Сейчас на складе:");

            for (int i = 0; i < Details.Count; i++)
            {
                Console.SetCursorPosition(positionX, positionY++);
                Console.WriteLine($"{i + 1}){Details[i].Name}. Цена {Details[i].Price}. " +
                    $"Стоимоить установки {Details[i].InstallationPrice}. На складе {_stacks[i].Quantity} шт.");
            }
        }
    }

    class Stack
    {
        private Detail _detail;

        public int Quantity { get; private set; }

        public Stack(Detail detail, int quantity)
        {
            _detail = detail;
            Quantity = quantity;
        }

        public void ReduceQuantity()
        {
            Quantity--;
        }
        public void IncreaseQuantity()
        {
            Quantity++;
        }        
    }

    class Detail
    {
        public string Name { get; private set; }
        public int Price { get; private set; }
        public int InstallationPrice { get; private set; }
        public bool IsWorking { get; private set; }
        public bool IsInStock { get; private set; }

        public Detail(string name, int price, int installationPrice)
        {
            Name = name;
            Price = price;
            InstallationPrice = installationPrice;
            IsWorking = true;
            IsInStock = true;
        }

        public void SetWorkingStatus(bool isWorking)
        {
            IsWorking = isWorking;
        }

        public void IsOutOfStock()
        {
            IsInStock = false;
        }
    }
}
