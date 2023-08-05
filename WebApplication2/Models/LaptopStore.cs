namespace WebApplication2.Models
{
    public class LaptopStore
    {
        public int Id { get; set; }
        public int Quantity { get; set; }

        public Guid LaptopId { get; set; }
        public Guid StoreNumber { get; set; }

        public Laptop Laptop { get; set; }
        public Store Store { get; set; }
    }
}
