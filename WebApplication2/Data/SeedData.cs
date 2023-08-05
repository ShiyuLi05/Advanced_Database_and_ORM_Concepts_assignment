using WebApplication2.Models;
using Microsoft.EntityFrameworkCore;
using System.Net;
using static WebApplication2.Models.Store;

namespace WebApplication2.Data
{
    public static class SeedData
    {
        public async static Task Initialize(IServiceProvider serviceProvider)
        {

            AssignmentContext db = new AssignmentContext(serviceProvider.GetRequiredService<DbContextOptions<AssignmentContext>>());

            db.Database.EnsureDeleted();
            db.Database.Migrate();

            // BRANDS
            Brand brand1 = new Brand {Name = "Dell"};
            Brand brand2 = new Brand {Name = "Apple" };

            if (!db.Brands.Any())
            {
                db.Brands.AddRange(brand1, brand2);
                db.SaveChanges();
            }

            // LAPTOPS
            Laptop laptop1 = new Laptop { Model = "the first model", Price = 1500, Condition = LaptopCondition.New };
            Laptop laptop2 = new Laptop { Model = "the second model", Price = 1000, Condition = LaptopCondition.Refurbished };
            Laptop laptop3 = new Laptop { Model = "the third model", Price = 500, Condition = LaptopCondition.Rental };

            laptop1.Brand = brand2;
            laptop2.Brand = brand1;
            laptop3.Brand = brand1;

            if (!db.Laptops.Any())
            {
                db.Laptops.Add(laptop1);
                db.Laptops.Add(laptop2);
                db.Laptops.Add(laptop3);
                db.SaveChanges();
            }
            
            // STORES
            Store store1 = new Store { StreetNameAndNumber = "123 Fake St.", Province = CanadianProvince.Ontario };
            Store store2 = new Store { StreetNameAndNumber = "456 Fake Rd.", Province = CanadianProvince.Saskatchewan };
            Store store3 = new Store { StreetNameAndNumber = "789 Fake Blvd.", Province = CanadianProvince.Manitoba };

            if (!db.Stores.Any())
            {
                db.Stores.Add(store1);
                db.Stores.Add(store2);
                db.Stores.Add(store3);
                db.SaveChanges();
            }

            // LAPTOPSTORES
            LaptopStore laptopStore1 = new LaptopStore { Laptop = laptop1, Store = store1, Quantity = -5 };
            LaptopStore laptopStore2 = new LaptopStore { Laptop = laptop2, Store = store2, Quantity = 5 };
            LaptopStore laptopStore3 = new LaptopStore { Laptop = laptop3, Store = store3, Quantity = 10 };

            if (!db.LaptopStores.Any())
            {
                db.LaptopStores.Add(laptopStore1);
                db.LaptopStores.Add(laptopStore2);
                db.LaptopStores.Add(laptopStore3);
                db.SaveChanges();
            }

        }
    }
}

