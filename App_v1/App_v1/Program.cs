// Продавец должен получить деньги а покупатель товар!

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
        public string name;
        public TaskSystem ts { get; set; }
    }

    public class Seller : Client
    {
        // Придумать логику возврата товара, чтобы при отрпавке/возврате товар пропадал/возвращался
        public Product productToSell;

        // Адресс склада
        Warehouse warehouse;

        public int sellerId { get; set; }


        public Seller(Product product, string name, TaskSystem ts, Warehouse warehouse)
        {
            this.productToSell = product;
            this.name = name;
            this.ts = ts;
            this.warehouse = warehouse;
        }

        public void GetApprovement()
        {
            // Согласует поставку с менеджером
            ts.AddApprTask(new ApprovementTask(productToSell));

            Console.WriteLine("Seller " + name + " создает заявку на согласование поставки " + productToSell.name);

        }

        public void SellProduct()
        {
            // Если поставка согласована, то можно отсылать товар и создавать заявку


            // Создаем заявку
            ts.AddSellerTask(new SellerTask(this));
            Console.WriteLine("Seller " + name + " создает заявку на продажу товара " + productToSell.name);

            // Отправляем продукт на склад
            warehouse.AddTempProduct(productToSell);
            Console.WriteLine("Seller " + name + " отправляет " + productToSell.name + " на временный склад");

            // Удаляем этот продукт у продовца
            Console.WriteLine("Seller " + name + " остался без продукста " + productToSell.name);
            productToSell = null;
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

        // Свойство для хранения уникального ID
        public int id { get; set; } 

        public int price;

        public string name;

        Seller seller = null;

        // Товар сотвествует требованиям хранения на складе
        public bool meetRequirements = false;

        // Товар согласован для хранения
        public bool isApproved = false;

        // Товар принят на хранение
        public bool isAccepted = false;

        // Товар согласован для отправки


        public Product(int price, bool mR, string name)
        {
            this.price = price;
            this.meetRequirements = mR;
            this.name = name;
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

        public DataBase db;

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
            //Console.WriteLine("Менеджер " + name + " взял в исполнение заявку по продаже " + sellerTask.seller.productToSell.name);
        }

        public void GetApprTask()
        {
            this.apprTask = ts.GetApprTask();
        }


        public void SolveSellerTask()
        {
            // Выдаем ID продавцу из заявки
            sellerTask.sellerID = nextId++;
            Console.WriteLine("Менеджер " + name + " присвоил Seller " + sellerTask.seller.name + " ID " + sellerTask.sellerID);

            // Выдаем ID товару из заявки
            sellerTask.productID = nextId++;                    // У него уже нет этого продукта // У него уже нет этого продукта // У него уже нет этого продукта
            Console.WriteLine("Менеджер " + name + " присвоил Product " + /*sellerTask.seller.productToSell.name + */" ID " + sellerTask.sellerID);

            // Создаем накладную и прикрепляем ее к заяке
            sellerTask.inv = new Invoice(sellerTask.sellerID, sellerTask.productID, nextId++);
            Console.WriteLine("Менеджер " + name + " создал накладную и присвоил ей ID " + sellerTask.inv.invId);

            // Создает задачу для Keeper, чтобы он  проверил и разместил товар на складе
            ts.AddKeeperTask(new KeeperTask(sellerTask.inv));
            Console.WriteLine("Менеджер " + name + " создал заявку на размещение по накладной с ID " + sellerTask.inv.invId);
        }

        public void SolveApprTask()
        {
            apprTask.product.isApproved = true;
        }

        // Менеджер может согласовать товар на поступление
        void ApproveProduct(Product product)
        {
            product.isApproved = true;
            Console.WriteLine("Товар " + sellerTask.seller.productToSell + " от продавца" + sellerTask.seller.name + " принят на хранение.");
        }

        // По накладной, хранящейся в базе данных?
        public void ReturnProductToSeller()
        {

        }

        //// Менеджер выделяет ID товару
        //public void IdentProduct(Product product)
        //{
        //    // Присваиваем уникальный ID и увеличиваем счетчик
        //    product.id = nextId++; 
        //}

        //// Менеджер выделяет ID продавцу
        //public void IdentSeller(Seller seller)
        //{
        //    // Присваиваем уникальный ID и увеличиваем счетчик
        //    seller.sellerId = nextId++;
        //}

    }

    public class Worker : Staff
    {
        CheckTask checkTask = null;
        TransportTask transportTask = null;
        Warehouse warehouse;

        public Worker(string name, TaskSystem ts, Warehouse warehouse)
        {
            this.name = name;
            this.ts = ts;
            this.warehouse = warehouse;
        }

        public void GetCheckTask()
        {
            this.checkTask = ts.GetCheckTask();
            Console.WriteLine("Worker " + name + " взял в исполнение задачу на проверку товара с ID " + checkTask.productID);
        }

        public void SolveCheckTask()
        {
            Product tempProduct = warehouse.GetTempProduct(checkTask.productID);
            tempProduct.isAccepted = true;
            Console.WriteLine("Worker " + name + " присвоил товару " + tempProduct.name + " с ID " + tempProduct.id + " статус accepted");
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
            Console.WriteLine("Keeper " + name + " взял в исполнение заявку по накладной с ID " + keeperTask.inv.invId);
        }

        public void SolveKeeperTask()
        {
            // Назначает рабочим проверить поступивший товар
            ts.AddCheckTask(new CheckTask(keeperTask.inv.productId));
            Console.WriteLine("Keeper " + name + " назначил рабочим проверить товар с ID " + keeperTask.inv.productId);

            // Назначает рабочим место, куда нужно разместить указанный товар
            ts.AddTransportTask(new TransportTask(keeperTask.inv.productId, warehouse));
            Console.WriteLine("Keeper " + name + " назначил рабочим разместить на основной склад товар с ID " + keeperTask.inv.productId);

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
        public int productID { set; get; }

        public int sellerID { set; get; }

        public Invoice inv { set; get; }
        //////////////////////////////

        public Seller seller;

        public SellerTask(Seller seller)
        {
            this.seller = seller;
        }
    }

    // Кладовщик решает, как резместить товар
    public class KeeperTask
    {
        public Invoice inv;

        public KeeperTask(Invoice inv)
        {
            this.inv = inv;
        }

    }

    // Менеджер решает, принять товар на склад или нет
    public class ApprovementTask
    {
        public Product product { get; }

        public ApprovementTask(Product product)
        {
            this.product = product;
        }
    }

    // Работник проверяет поступивший на temp склад товар
    public class CheckTask
    {
        public int productID;

        // Нужно ID товара
        public CheckTask(int productID)
        {
            this.productID = productID;
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

            Product product1 = new Product(12, true, "Колбаса");

            Seller seller1 = new Seller(product1, "Gosha", ts, wh);

            Manager manager1 = new Manager("Oleg", ts);

            Worker worker1 = new Worker("Grisha", ts, wh);

            Keepper keepper1 = new Keepper("Misha", ts, wh);

            // Продавец согласует поставку
            seller1.GetApprovement();

            // После одобрения поставки, продавец создает заявку на поставку
            seller1.SellProduct();

            // Продовец привозит товар на временный склад

            // Менеджер обрабатывает заявку на поставку
            manager1.GetSellerTask();

            // Менеджер присваивает ID заявке, и создает задачу по проверке и размещению поступившего товара
            manager1.SolveSellerTask();

            // Хранитель решает свою задачу, путем создания CheckTask и TransportTask для Worker
            keepper1.GetKeeperTask();

            keepper1.SolveKeeperTask();

            // Рабочий проверяет товар
            worker1.GetCheckTask();

            worker1.SolveCheckTask();

            // рабочий переносит товар из временного хранилиша в основное
            worker1.GetTransportTask();

            worker1.SolveTransportTask();


        }
    }
}
