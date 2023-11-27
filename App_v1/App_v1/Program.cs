using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App_v1
{
    public class Client
    {
        //public int ID;
        public TaskSystem ts { get; set; }
        public Warehouse warehouse;

        // УСЛОВНО ИСЧЕРПЫВАЮЩАЯ ИНФОРМАЦИЯ О КЛИЕНТЕ
        public string name;
        public string address;
    }

    public class Seller : Client
    {
        // Придумать логику возврата товара, чтобы при отрпавке/возврате товар пропадал/возвращался
        public Product productToSell;

        // Адресс склада

        public int sellerId { get; set; }

        public Seller(Product product, TaskSystem ts, Warehouse warehouse, string name, string address)
        {
            this.productToSell = product;
            this.name = name;
            this.ts = ts;
            this.warehouse = warehouse;
            this.address = address;
        }

        public void GetApprovement()
        {
            // Согласует поставку с менеджером
            ts.AddApprTask(new ApprovementTask(productToSell));

            Console.WriteLine("Seller " + name + " создает заявку на согласование поставки " + productToSell.productName);

        }

        public void SellProduct()
        {
            // Если товар соотвествует требованиям хранения (проверяют рабочие) и у него есть ID
            if (productToSell.isAccepted == true)
            {
                // Продовец создает заявку, где указывает свои данные и продукт, который он хочет разместить на основном складе
                ts.AddSellerTask(new SellerTask(this.name, this.address, this.productToSell.ProductId));
                Console.WriteLine("Seller " + name + " создает заявку на продажу товара с ID " + productToSell.ProductId);

                // Удаляем продукт у продовца, теперь товар оффициально на временном складе
                Console.WriteLine("Seller " + name + " остался без товара " + productToSell.productName);
                productToSell = null;
            }
        }

        public void TransportProduct()
        {
            // Если поставка согласован (менеджером), то можно привозить товар на временный склад
            if (productToSell.isApproved == true) {

                // Продовец привзоит товар на временный склад
                warehouse.AddTempProduct(productToSell);
                Console.WriteLine("Seller " + name + " привозит " + productToSell.productName + " на временный склад");

                // Рабочие тут же проверяют поступивший товар и присваивают ему ID
                ts.AddCheckTask(new CheckTask(productToSell));
                Console.WriteLine("Seller " + name + " создает заявку на проверку товара" + productToSell.productName);
            }
        }
    }

    public class Buyer : Client
    {

    }


    public class Invoice
    {
        // Свойство для хранения уникального ID накладной
        public int invId { get; set; } 

        public int sellerId;
        public int productId;

        public Invoice(int sellerId, int productId, int invId)
        {
            this.sellerId = sellerId;
            this.productId = productId;
        }
    }

    public class Product
    {
        public string productName;
        public bool meetRequirements;

        // Свойство для хранения уникального ID
        public int ProductId { get; set; } 

        // Перед тем, как привезти твар, ему нужно присвоить уникальный ID
        public string sellerID;

        // Товар согласован для хранения
        public bool isApproved = false;

        // Товар принят на хранение
        public bool isAccepted = false;

        //public string sellerName;
        //public string sellerAddress;

        public Product(bool mR, string name)
        {
            this.meetRequirements = mR;
            this.productName = name;
        }
    }

    public class Warehouse // Костыль
    {
        // Временный склад
        Product[] tempProducts = new Product[10];

        // Основной склад
        Product[] mainProducts = new Product[10];


        public void AddTempProduct(Product product)
        {
            tempProducts[0] = product;
        }

        // Получить товар со временного склада по ID товара
        public Product GetTempProduct(int id)
        {
            return tempProducts[0];
        }

        // Переместить товар с временного склада в основной по ID
        public void TransportToMain(int id)
        {
            mainProducts[0] = tempProducts[0];
        }
    }

    public class Staff
    {
        //public int id { set; get; }
        public string name { set; get; }
        public TaskSystem ts { set; get; }
    }

    public class Manager : Staff
    {
        public ClientTask clientTask = null;
        public SellerTask sellerTask = null;
        public ApprovementTask apprTask = null;

        private static int nextId = 1; // Статическая переменная для отслеживания следующего уникального ID

        public Manager(string name, TaskSystem ts)
        {
            this.name = name;
            this.ts = ts;
        }

        public void GetClientTask()
        {
            this.clientTask = ts.GetManagerClientTask();
        }

        public void GetSellerTask()
        {
            this.sellerTask = ts.GetManagerSellerTask();
            Console.WriteLine("Менеджер " + name + " взял в исполнение заявку по продаже товара c ID " + sellerTask.productID);
        }

        public void GetApprTask()
        {
            this.apprTask = ts.GetApprTask();
        }


        public void SolveSellerTask()
        {
            // Создает задачу для Keeper, чтобы он  проверил и разместил товар на складе
            ts.AddKeeperTask(new KeeperTask(sellerTask.productID));
            Console.WriteLine("Менеджер " + name + " создал заявку на размещение товара с ID " + sellerTask.productID + " на основной склад");

            // Менеджер помечает задачу как выполненную и может брать новую
            this.sellerTask = null;
            //Console.WriteLine("Менеджер " + name + " выполнил sellerTask");
        }

        // Менеджер может согласовать товар на поступление
        public void SolveApprTask()
        {
            apprTask.product.isApproved = true;
            Console.WriteLine("Manager " + name + " согласовал для поставки товар " + apprTask.product.productName);

        }

        // По накладной, хранящейся в базе данных?
        public void ReturnProductToSeller()
        {

        }
    }

    public class Worker : Staff
    {
        CheckTask checkTask = null;
        TransportTask transportTask = null;
        Warehouse warehouse;

        private static int nextId = 1; // Статическая переменная для отслеживания следующего уникального ID

        public Worker(string name, TaskSystem ts, Warehouse warehouse)
        {
            this.name = name;
            this.ts = ts;
            this.warehouse = warehouse;
        }

        public void GetCheckTask()
        {
            this.checkTask = ts.GetCheckTask();
            Console.WriteLine("Worker " + name + " взял в исполнение задачу на проверку товара " + checkTask.product.productName);
        }

        public void SolveCheckTask()
        {
            checkTask.product.isAccepted = true;
            Console.WriteLine("Worker " + name + " присвоил товару " + checkTask.product.productName + " статус accepted");

            checkTask.product.ProductId = nextId++;
            Console.WriteLine("Worker " + name + " присвоил товару " + checkTask.product.productName + " ID " + checkTask.product.ProductId);
        }

        public void GetTransportTask()
        {
            this.transportTask = ts.GetTransportTask();
            Console.WriteLine("Worker " + name + " взял в исполнение задачу на перевозку товара с ID " + transportTask.productID + " на основной склад.");
        }

        public void SolveTransportTask()
        {
            warehouse.TransportToMain(transportTask.productID);
            Console.WriteLine("Worker " + name + " перенес товар с ID " + transportTask.productID + " на основной склад");
        }
    }

    public class Keepper : Staff
    {
        public KeeperTask keeperTask = null;
        public Warehouse warehouse;

        public Keepper(string name, TaskSystem ts, Warehouse warehouse)
        {
            this.name = name;
            this.ts = ts;
            this.warehouse = warehouse;
        }

        public void GetKeeperTask()
        {
            this.keeperTask = ts.GetKeeperTask();
            Console.WriteLine("Keeper " + name + " взял в исполнение заявку по размещению товара с ID " + keeperTask.productID);
        }

        public void SolveKeeperTask()
        {
            // Назначает рабочим место, куда нужно разместить указанный товар
            ts.AddTransportTask(new TransportTask(keeperTask.productID, warehouse));
            Console.WriteLine("Keeper " + name + " назначил рабочим разместить на основной склад товар с ID " + keeperTask.productID);

        }
    }


    public class ClientTask
    {
       public string name = "clientTask";
    }

    // Заявка создается продовцом. 
    public class SellerTask
    {
        // Поля, заполняемые менеджером
        public int productID;

        public int sellerID { set; get; }

        //public Invoice inv { set; get; }
        //////////////////////////////
        public string sellerName { get; }

        public string sellerAddress { get; }

        public SellerTask(string address, string name, int productID)
        {
            this.sellerName = name;
            this.sellerAddress = address;
            this.productID = productID;
        }
    }

    // Кладовщик решает, как резместить товар
    public class KeeperTask
    {
        public int productID;

        public KeeperTask(int productID)
        {
            this.productID = productID;
        }

    }

    // Менеджер решает, принять товар на склад или нет
    public class ApprovementTask
    {
        public Product product { get; set; }

        public ApprovementTask(Product product)
        {
            this.product = product;
        }
    }

    // Работник проверяет поступивший на temp склад товар
    public class CheckTask
    {
        public Product product;

        public CheckTask(Product product)
        {
            this.product = product;
        }
    }

    // Работник размещает товар на основной склад
    public class TransportTask
    {
        public int productID;

        // ВЫДЕЛЯТЬ ПОЛКУ!!!!!!!!!!!!!!!!!!!!
        public Warehouse wh;

        // Нужно ID товара и место на складе
        public TransportTask(int productID, Warehouse wh)
        {
            this.productID = productID;
            this.wh = wh;
        }
    }


    public class TaskSystem
    {
        private Stack<ClientTask> clientTasks = new Stack<ClientTask>();
        private Stack<SellerTask> sellerTasks = new Stack<SellerTask>();
        private Stack<ApprovementTask> apprTasks = new Stack<ApprovementTask>();
        private Stack<CheckTask> checkTasks = new Stack<CheckTask>();
        private Stack<KeeperTask> keeperTasks = new Stack<KeeperTask>();
        private Stack<TransportTask> transportTasks = new Stack<TransportTask>();


        public void AddClientTask(ClientTask task)
        {
            clientTasks.Push(task);
        }

        public void AddSellerTask(SellerTask task)
        {
            sellerTasks.Push(task);
        }

        public void AddApprTask(ApprovementTask task)
        {
            apprTasks.Push(task);
        }

        public void AddCheckTask(CheckTask task)
        {
            checkTasks.Push(task);
        }

        public void AddTransportTask(TransportTask task)
        {
            transportTasks.Push(task);
        }

        public void AddKeeperTask(KeeperTask task)
        {
            keeperTasks.Push(task);
        }

        public ClientTask GetManagerClientTask()
        {
            if (clientTasks.Count > 0)
            {
                return clientTasks.Pop();
            }
            else
            {
                return null;
            }
        }

        public SellerTask GetManagerSellerTask()
        {
            if (sellerTasks.Count > 0)
            {
                return sellerTasks.Pop();
            }
            else
            {
                return null;
            }
        }

        public ApprovementTask GetApprTask()
        {
            if (apprTasks.Count > 0)
            {
                return apprTasks.Pop();
            }
            else
            {
                return null;
            }
        }

        public CheckTask GetCheckTask()
        {
            if (checkTasks.Count > 0)
            {
                return checkTasks.Pop();
            }
            else
            {
                return null;
            }
        }

        public TransportTask GetTransportTask()
        {
            if (transportTasks.Count > 0)
            {
                return transportTasks.Pop();
            }
            else
            {
                return null;
            }
        }

        public KeeperTask GetKeeperTask()
        {
            if (keeperTasks.Count > 0)
            {
                return keeperTasks.Pop();
            }
            else
            {
                return null;
            }
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            TaskSystem ts = new TaskSystem();

            Warehouse wh = new Warehouse();

            Product product1 = new Product(true, "Колбаса");

            Seller seller1 = new Seller(product1, ts, wh,  "Gosha", "Chelyabinsk");

            Manager manager1 = new Manager("Oleg", ts);

            Worker worker1 = new Worker("Grisha", ts, wh);

            Keepper keepper1 = new Keepper("Misha", ts, wh);

            // Продавец пробует согласовать поставку
            seller1.GetApprovement();

            // Менеджер решает соглисовать поставку или нет
            manager1.GetApprTask();
            manager1.SolveApprTask();

            // Поставщик привозит товар на временный склад, где товар сразу проверяют работники склада и присваивают ему ID (Могут вернуть)
            seller1.TransportProduct();

            worker1.GetCheckTask();
            worker1.SolveCheckTask();

            // Если проверка товара прошла успешно, поставщик создает заяку по размещению товара на основном складе
            seller1.SellProduct();

            // Менеджер обрабатывает заявку на поставку
            manager1.GetSellerTask();
            manager1.SolveSellerTask();

            // Хранитель решает свою задачу, путем создания CheckTask и TransportTask для Worker
            keepper1.GetKeeperTask();
            keepper1.SolveKeeperTask();

            // рабочий переносит товар из временного хранилиша в основное
            worker1.GetTransportTask();
            worker1.SolveTransportTask();
        }
    }
}
