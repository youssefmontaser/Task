namespace Task
{

    public interface IShippable
    {
        string GetName();
        double GetWeight();
    }
    public abstract class Product
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }

        public Product(string name, decimal price, int quantity)
        {
            Name = name;
            Price = price;
            Quantity = quantity;
        }
        public virtual bool IsExpired() => false;
        public virtual void DisplayInfo()
        {
            Console.WriteLine($"Name: {Name}, Price: {Price:C}, Quantity: {Quantity}");
        }
    }
    public class ExpirableProduct : Product
    {
        public DateTime ExpiryDate { get; set; }

        public ExpirableProduct(string name, decimal price, int quantity, DateTime expiryDate)
            : base(name, price, quantity)
        {
            ExpiryDate = expiryDate;
        }

        public override void DisplayInfo()
        {
            base.DisplayInfo();
            Console.WriteLine($"Expiry Date: {ExpiryDate.ToShortDateString()}");
        }
        public override bool IsExpired() => DateTime.Now > ExpiryDate;
    }
    public class ShippableExpirableProduct : ExpirableProduct, IShippable
    {
        public double Weight { get; }

        public ShippableExpirableProduct(string name, decimal price, int quantity, DateTime expiryDate, double weight)
            : base(name, price, quantity, expiryDate)
        {
            Weight = weight;
        }
        public string GetName() => Name;
        public double GetWeight() => Weight;
    }
    public class ShippableProduct : Product, IShippable
    {
        public double Weight { get; }

        public ShippableProduct(string name, decimal price, int quantity, double weight)
            : base(name, price, quantity)
        {
            Weight = weight;
        }
        public string GetName() => Name;
        public double GetWeight() => Weight;
    }
    public class CartItem
    {
        public Product Product { get; }
        public int Quantity { get; }

        public CartItem(Product product, int quantity)
        {
            Product = product;
            Quantity = quantity;
        }

        public double GetTotalPrice() => (double)Product.Price * Quantity;
    }
    public class Cart
    {
        public List<CartItem> Items { get; } = new();

        public void AddProduct(Product product, int quantity)
        {
            if (product.Quantity < quantity)
                throw new Exception($"Only {product.Quantity} of {product.Name} is available.");
            Items.Add(new CartItem(product, quantity));
        }

        public double GetSubtotal() => Items.Sum(item => item.GetTotalPrice());

        public List<IShippable> GetShippableItems()
        {
            return Items
                .Where(item => item.Product is IShippable)
                .Select(item => (IShippable)item.Product)
                .ToList();
        }

        public bool IsEmpty() => !Items.Any();
    }
    public class Customer
    {
        public string Name { get; }
        public double Balance { get; private set; }

        public Cart Cart { get; } = new();

        public Customer(string name, double balance)
        {
            Name = name;
            Balance = balance;
        }

        public void Checkout()
        {
            const double WeightBasedShipping = 20;
            if (Cart.IsEmpty())
                throw new Exception("Cart is empty.");
            foreach (var item in Cart.Items)
            {
                if (item.Product.IsExpired())
                    throw new Exception($"{item.Product.Name} is expired.");
                if (item.Product.Quantity < item.Quantity)
                    throw new Exception($"{item.Product.Name} doesn't have enough stock.");
            }

            double subtotal = Cart.GetSubtotal();
            double shipping = Cart.Items
      .Where(ci => ci.Product is IShippable)
      .Sum(ci => ((IShippable)ci.Product).GetWeight()/1000 * ci.Quantity)
      * WeightBasedShipping;
            double totalWeight = 0;
            Console.WriteLine("** Shipment notice **");
            foreach (var item in Cart.Items.Where(i => i.Product is IShippable))
            {
                var ship = (IShippable)item.Product;
                totalWeight += ship.GetWeight() * item.Quantity;
                Console.WriteLine($"- {item.Quantity}x {ship.GetName()} ({ship.GetWeight()*item.Quantity} g)");
            }
            Console.WriteLine($"Total package weight {totalWeight/1000:0.0}kg");
            Console.WriteLine("** Checkout receipt **");
            foreach (var item in Cart.Items)
            {
                Console.WriteLine($"- {item.Quantity}x {item.Product.Name} {item.Product.Price*item.Quantity}");
            }
            Console.WriteLine("----------------------");
            double total = subtotal + shipping;

            if (Balance < total)
                throw new Exception("Insufficient balance.");
            foreach (var item in Cart.Items)
                item.Product.Quantity -= item.Quantity;

            Balance -= total;

            Console.WriteLine($"Subtotal: ${subtotal:F2}");
            Console.WriteLine($"Shipping: ${shipping:F2}");
            Console.WriteLine($"Total Paid: ${total:F2}");
            Console.WriteLine($"Remaining Balance: ${Balance:F2}");
            Cart.Items.Clear();
        }
    }
    internal class Program
    {
        static void Main(string[] args)
        {
            var chesse = new ShippableExpirableProduct("chesse", 100.00m, 2, DateTime.Now.AddDays(2), 400);
            var Biscuits = new ShippableProduct("Biscuits", 150.00m, 1, 700);
            var candy = new ExpirableProduct("Candy", 2.00m, 20, DateTime.Now.AddDays(1));
            var customer = new Customer("Alice", 500);
            customer.Cart.AddProduct(chesse, 2);
            customer.Cart.AddProduct(Biscuits, 1);
            try
            {
                customer.Cart.AddProduct(candy, 1);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            customer.Checkout();
        }
    }
}
