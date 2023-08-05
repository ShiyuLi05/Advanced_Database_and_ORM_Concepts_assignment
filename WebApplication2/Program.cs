using WebApplication2.Data;
using WebApplication2.Models;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static WebApplication2.Models.Store;
using Microsoft.OpenApi.Extensions;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AssignmentContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("AssignmentConnectionString"));
});

builder.Services.Configure<JsonOptions>(options => {
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

var app = builder.Build();

using (IServiceScope scope = app.Services.CreateScope())
{
    IServiceProvider serviceProvider = scope.ServiceProvider;

    await SeedData.Initialize(serviceProvider);
}

app.MapGet("/laptops/search", (AssignmentContext db, decimal? lowest, decimal? highest, Guid? storeNumber, string? province, string? condition, int? brandId, string? searchPhrase) =>
{
    List<Laptop> results = db.Laptops.Include(l => l.LaptopStores).ThenInclude(ls => ls.Store).ToList();
    List<Laptop> temp = new List<Laptop>();

    if (lowest != null && highest != null)
    {
        lowest = lowest ?? Int32.MinValue;
        highest = highest ?? Int32.MaxValue;
        if (lowest > highest)
        {
            return Results.BadRequest("Cannot request laptop with a lowest price greater than highest price.");
        }
        temp = results.Where(l => l.Price >= lowest && l.Price <= highest).ToList();

        if (temp.Count <= 0)
        {
            return Results.NotFound("There is no laptop in this price range.");
        }
    } else if (lowest != null)
    {
        lowest = lowest ?? Int32.MinValue;
        temp = results.Where(l => l.Price >= lowest).ToList();

        if (temp.Count <= 0)
        {
            return Results.NotFound($"There is no laptop which price greater than {lowest}");
        }
        results.Clear();
        results.AddRange(temp);
        temp.Clear();

    } else if (highest != null)
    {
        highest = highest ?? Int32.MaxValue;
        temp = results.Where(l => l.Price  <= highest).ToList();
        if (temp.Count <= 0)
        {
            return Results.NotFound($"There is no laptop which price less than {highest}");
        }
        results.Clear();
        results.AddRange(temp);
        temp.Clear();
    }

    if (storeNumber != null)
    {
        try
        {
            temp = results
            .Where(l => l.LaptopStores
            .Any(ls => ls.StoreNumber == storeNumber && ls.Quantity > 0)).ToList();

            if (temp.Count <= 0)
            {
                return Results.NotFound($"There is no laptops in stock at this store.");
            }
            results.Clear();
            results.AddRange(temp);
            temp.Clear();
        }
        catch (ArgumentNullException)
        {
            return Results.NotFound("Could not find this store.");
        }
    }

    if (province != null)
    {
        try
        {
            string? provinceEnumName = Enum.GetNames(typeof(CanadianProvince)).FirstOrDefault(name => name.ToLower() == province.ToLower());
            if (provinceEnumName == null)
            {
                return Results.NotFound($"There is no {province} Province in Canada.");
            }
            CanadianProvince provinceEnum = (CanadianProvince)Enum.Parse(typeof(CanadianProvince), provinceEnumName);

            temp = results
            .Where(l => l.LaptopStores
            .Any(ls => ls.Store.Province == provinceEnum && ls.Quantity > 0)).ToList();

            if (temp.Count <= 0)
            {
                return Results.NotFound($"There is no laptops in stock in {province}.");
            }
            results.Clear();
            results.AddRange(temp);
            temp.Clear();
        } catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    if (condition != null)
    {
        try
        {

            string? conditionEnumName = Enum.GetNames(typeof(LaptopCondition)).FirstOrDefault(name => name.ToLower() == condition.ToLower());
            if (conditionEnumName == null)
            {
                return Results.NotFound($"There is no {condition} condition.");
            }
            LaptopCondition conditionEnum = (LaptopCondition)Enum.Parse(typeof(LaptopCondition), conditionEnumName);

            temp = results.Where(l => l.Condition == conditionEnum).ToList();

            if (temp.Count <= 0)
            {
                return Results.NotFound($"There are no laptops with the {condition} condition.");
            }
            results.Clear();
            results.AddRange(temp);
            temp.Clear();

        } catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    if (brandId != null)
    {
        try
        {
            temp = results.Where(l => l.BrandId == brandId).ToList();
            
            if (temp.Count <= 0)
            {
                return Results.NotFound("There is no laptops in this brand.");
            }
            results.Clear();
            results.AddRange(temp);
            temp.Clear();
        } catch (ArgumentNullException)
        {
            return Results.NotFound("Could not find this brand.");
        }
    }

    if (!string.IsNullOrEmpty(searchPhrase))
    {
        try
        {
            temp = results.Where(l => l.Model.Contains(searchPhrase)).ToList();
        } catch (ArgumentNullException)
        {
            return Results.NotFound($"There is no laptops' model contain {searchPhrase}");
        }
    }

    return Results.Ok(results);
});

app.MapGet("/stores/{storeNumber}/inventory", (AssignmentContext db, Guid storeNumber) =>
{
    try
    {
        Store? store = db.Stores
        .Include(s => s.LaptopStores).ThenInclude(ls => ls.Laptop)
        .FirstOrDefault(s => s.StoreNumber == storeNumber);

        if (store == null)
        {
            return Results.NotFound($"Store with StoreNumber {storeNumber} not found.");
        }

        List<Laptop> results = store.LaptopStores
        .Where(ls => ls.Quantity > 0)
        .Select(ls => ls.Laptop).ToList();

        if (results.Count <= 0) 
        { 
            return Results.NotFound("There is no laptop in stock.");
        }

        return Results.Ok(results);
    } catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

app.MapPost("/stores/{storeNumber}/{laptopNumber}/changeQuantity", (AssignmentContext db, Guid storeNumber, Guid laptopNumber, int amount) =>
{
    try
    {
        Store? store = db.Stores
        .Include(s => s.LaptopStores).ThenInclude(ls => ls.Laptop)
        .FirstOrDefault(s => s.StoreNumber == storeNumber);
        if (store == null)
        {
            return Results.NotFound($"Store with StoreNumber {storeNumber} not found.");
        }

        Laptop? laptop = db.Laptops
        .Include(l => l.LaptopStores).ThenInclude(ls => ls.Store)
        .FirstOrDefault(l => l.Number == laptopNumber);
        if (laptop == null)
        {
            return Results.NotFound($"Laptop with LaptopNumber {laptopNumber} not found.");
        }

        LaptopStore? laptopStore = store.LaptopStores
                .FirstOrDefault(ls => ls.LaptopId == laptop.Number);

        if (laptopStore == null)
        {
            laptopStore = new LaptopStore
            {
                Laptop = laptop,
                Store = store
            };
            db.LaptopStores.Add(laptopStore);
        }

        laptopStore.Quantity = amount;
        db.SaveChanges();

        return Results.Ok(laptop);

    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

app.MapGet("/laptops/average", (AssignmentContext db, int brandId) =>
{
    try
    {
        Brand? brand = db.Brands
        .Include(b => b.Laptops)
        .FirstOrDefault(b => b.Id == brandId);

        if (brand == null)
        {
            return Results.NotFound($"Brand with BrandId {brandId} not found.");
        }

        var results = new
        {
            LaptopCount = brand.Laptops.Count,
            AveragePrice = brand.Laptops.Average(l => l.Price)
        };

        return Results.Ok(results);
    } catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

app.MapGet("/stores/province", (AssignmentContext db) =>
{
    try
    {
        var storesByProvince = db.Stores
             .GroupBy(s => s.Province)
             .Select(group => new
             {
                 Province = group.Key.ToString(),
                 StoreCount = group.Count()
             })
             .ToList();

        Dictionary<string,int> result = storesByProvince.ToDictionary(stores => stores.Province, stores => stores.StoreCount);

        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});


app.Run();
