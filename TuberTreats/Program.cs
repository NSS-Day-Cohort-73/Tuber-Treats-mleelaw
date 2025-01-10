using System.Data;
using System.Xml.Schema;
using TuberTreats.Models;
using TuberTreats.Models.DTO;

var builder = WebApplication.CreateBuilder(args);
List<TuberDriver> TuberDrivers = new List<TuberDriver>
{
    new TuberDriver { Id = 1, Name = "John Smith" },
    new TuberDriver { Id = 2, Name = "Sarah Wilson" },
    new TuberDriver { Id = 3, Name = "Mike Johnson" },
};

List<Customer> Customers = new List<Customer>
{
    new Customer
    {
        Id = 1,
        Name = "Alice Brown",
        Address = "123 Oak St",
    },
    new Customer
    {
        Id = 2,
        Name = "Bob White",
        Address = "456 Maple Ave",
    },
    new Customer
    {
        Id = 3,
        Name = "Carol Davis",
        Address = "789 Pine Rd",
    },
    new Customer
    {
        Id = 4,
        Name = "David Miller",
        Address = "321 Elm St",
    },
    new Customer
    {
        Id = 5,
        Name = "Eve Wilson",
        Address = "654 Birch Ln",
    },
};

List<Topping> Toppings = new List<Topping>
{
    new Topping { Id = 1, Name = "Cheese" },
    new Topping { Id = 2, Name = "Bacon" },
    new Topping { Id = 3, Name = "Sour Cream" },
    new Topping { Id = 4, Name = "Chives" },
    new Topping { Id = 5, Name = "Butter" },
};

List<TuberOrder> TuberOrders = new List<TuberOrder>
{
    new TuberOrder
    {
        Id = 1,
        CustomerId = 1,
        TuberDriverId = 1,
        OrderPlacedOnDate = new DateTime(2024, 1, 7, 10, 0, 0),
        DeliveredOnDate = new DateTime(2024, 1, 7, 10, 30, 0),
    },
    new TuberOrder
    {
        Id = 2,
        CustomerId = 2,
        TuberDriverId = 2,
        OrderPlacedOnDate = new DateTime(2024, 1, 7, 11, 15, 0),
        DeliveredOnDate = new DateTime(2024, 1, 7, 11, 45, 0),
    },
    new TuberOrder
    {
        Id = 3,
        CustomerId = 3,
        TuberDriverId = null,
        OrderPlacedOnDate = new DateTime(2024, 1, 7, 12, 30, 0),
        DeliveredOnDate = null,
    },
};

List<TuberTopping> TuberToppings = new List<TuberTopping>
{
    new TuberTopping
    {
        Id = 1,
        TuberOrderId = 1,
        ToppingId = 1,
    },
    new TuberTopping
    {
        Id = 2,
        TuberOrderId = 1,
        ToppingId = 2,
    },
    new TuberTopping
    {
        Id = 3,
        TuberOrderId = 2,
        ToppingId = 3,
    },
    new TuberTopping
    {
        Id = 4,
        TuberOrderId = 2,
        ToppingId = 4,
    },
    new TuberTopping
    {
        Id = 5,
        TuberOrderId = 3,
        ToppingId = 5,
    },
};

// Add services to the container.

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

//add endpoints here

//tuberorders endpoints
app.MapGet(
    "/tuberorders",
    () =>
    {
        var allTuberOrders = TuberOrders.Select(to => new TuberOrderDTO
        {
            Id = to.Id,
            OrderPlacedOnDate = to.OrderPlacedOnDate,
            CustomerId = to.CustomerId,
            TuberDriverId = to.TuberDriverId,
            DeliveredOnDate = to.DeliveredOnDate,
        });
        return allTuberOrders;
    }
);

app.MapGet(
    "/tuberorders/{ordersId}",
    (int ordersId) =>
    {
        var orderById = TuberOrders.FirstOrDefault(o => o.Id == ordersId);
        if (orderById == null)
            return Results.NotFound();

        var customerForThisOrder = Customers.FirstOrDefault(c => c.Id == orderById.CustomerId);
        var customerInfo = new CustomerDTO
        {
            Id = customerForThisOrder.Id,
            Name = customerForThisOrder.Name,
            Address = customerForThisOrder.Address,
        };

        var driverForThisOrder = TuberDrivers.FirstOrDefault(d => d.Id == orderById.TuberDriverId);
        var driverInfo =
            driverForThisOrder == null
                ? null
                : new TuberDriverDTO { Id = driverForThisOrder.Id, Name = driverForThisOrder.Name };

        var tuberToppingsForThisOrder = TuberToppings
            .Where(tt => tt.TuberOrderId == ordersId)
            .ToList();

        var toppingsList = tuberToppingsForThisOrder
            .Select(tt => Toppings.FirstOrDefault(t => t.Id == tt.ToppingId))
            .ToList();
        var toppingsForThisOrder = toppingsList
            .Select(t => new ToppingDTO { Id = t.Id, Name = t.Name })
            .ToList();

        var orderByIdDTO = new TuberOrderDTO
        {
            Id = orderById.Id,
            OrderPlacedOnDate = orderById.OrderPlacedOnDate,
            CustomerId = orderById.CustomerId,
            Customer = customerInfo,
            TuberDriverId = orderById.TuberDriverId,
            TuberDriver = driverInfo,
            DeliveredOnDate = orderById.DeliveredOnDate,
            Toppings = toppingsForThisOrder,
        };

        return Results.Ok(orderByIdDTO);
    }
);

app.MapPost(
    "/tuberorders",
    (TuberOrder neworder) =>
    {
        var maxOrderId = TuberOrders.Max(o => o.Id);
        var newOrder = new TuberOrder
        {
            Id = maxOrderId + 1,
            OrderPlacedOnDate = DateTime.Now,
            CustomerId = neworder.CustomerId,
        };

        TuberOrders.Add(newOrder);
        return Results.Created("/tuberorders", newOrder);
    }
);

app.MapPost(
    "/tuberorders/{id}/complete",
    (int id) =>
    {
        var orderById = TuberOrders.FirstOrDefault(o => o.Id == id);
        if (orderById == null)
        {
            return Results.NotFound();
        }

        orderById.DeliveredOnDate = DateTime.Now;
        return Results.Ok(orderById);
    }
);

app.MapPut(
    "/tuberorders/{id}",
    (int id, TuberOrder editedOrder) =>
    {
        var tuberOrder = TuberOrders.FirstOrDefault(o => o.Id == id);
        if (tuberOrder == null)
        {
            return Results.NotFound();
        }
        tuberOrder.TuberDriverId = editedOrder.TuberDriverId;
        return Results.Ok(tuberOrder);
    }
);

//toppings endpoints
app.MapGet(
    "/toppings",
    () =>
    {
        var allToppings = Toppings.Select(t => new ToppingDTO { Id = t.Id, Name = t.Name });
        return allToppings;
    }
);

app.MapGet(
    "/toppings/{firstToppingId}",
    (int firstToppingId) =>
    {
        var toppingById = Toppings.FirstOrDefault(t => t.Id == firstToppingId);
        if (toppingById == null)
            return Results.NotFound();

        var toppingByIdDTO = new ToppingDTO { Id = toppingById.Id, Name = toppingById.Name };

        return Results.Ok(toppingByIdDTO);
    }
);

//tubbertoppings endpoints
app.MapGet(
    "/tubertoppings",
    () =>
    {
        var allTuberToppings = TuberToppings.Select(tt => new TuberToppingDTO
        {
            Id = tt.Id,
            TuberOrderId = tt.TuberOrderId,
            ToppingId = tt.ToppingId,
        });
        return allTuberToppings;
    }
);

app.MapDelete(
    "/tubertoppings/{newTuberId}",
    (int newTuberId) =>
    {
        var deleteTopping = TuberToppings.FirstOrDefault(tt => tt.Id == newTuberId);
        if (deleteTopping == null)
        {
            return Results.NotFound();
        }
        TuberToppings.Remove(deleteTopping);
        return Results.Ok("deleted");
    }
);

app.MapPost(
    "/tubertoppings",
    (TuberTopping TuberTopping) =>
    {
        var maxTTId = TuberToppings.Max(tt => tt.Id);

        var newTT = new TuberTopping
        {
            Id = maxTTId + 1,
            ToppingId = TuberTopping.ToppingId,
            TuberOrderId = TuberTopping.TuberOrderId,
        };

        TuberToppings.Add(newTT);
        return Results.Ok(newTT);
    }
);

//customers endpoints
app.MapGet(
    "/customers",
    () =>
    {
        var allCustomers = Customers.Select(c => new CustomerDTO
        {
            Id = c.Id,
            Name = c.Name,
            Address = c.Address,
        });
        return allCustomers;
    }
);

app.MapGet(
    "/customers/{lastCustomerId}",
    (int lastCustomerId) =>
    {
        var customerById = Customers.FirstOrDefault(c => c.Id == lastCustomerId);
        if (customerById == null)
            return Results.NotFound();

        var customerOrdersList = TuberOrders.Where(to => to.CustomerId == lastCustomerId).ToList();

        var customerByIdDTO = new CustomerDTO
        {
            Id = customerById.Id,
            Name = customerById.Name,
            Address = customerById.Address,
            TuberOrders = customerOrdersList,
        };
        return Results.Ok(customerByIdDTO);
    }
);

app.MapDelete(
    "/customers/{newCustomerId}",
    (int newCustomerId) =>
    {
        var deleteCustomer = Customers.FirstOrDefault(c => c.Id == newCustomerId);
        if (deleteCustomer == null)
        {
            return Results.NotFound();
        }

        Customers.Remove(deleteCustomer);
        return Results.NoContent();
    }
);

app.MapPost(
    "/customers",
    (Customer Customer) =>
    {
        var maxCustomerId = Customers.Max(c => c.Id);

        var newCustomer = new Customer
        {
            Id = maxCustomerId + 1,
            Name = Customer.Name,
            Address = Customer.Address,
        };

        Customers.Add(newCustomer);
        return Results.Created("/customers", newCustomer);
    }
);

//tuberdrivers endpoints
app.MapGet(
    "/tuberdrivers",
    () =>
    {
        var allTuberDrivers = TuberDrivers.Select(td => new TuberDriverDTO
        {
            Id = td.Id,
            Name = td.Name,
        });
        return allTuberDrivers;
    }
);

app.MapGet(
    "/tuberdrivers/{firstDriverId}",
    (int firstDriverId) =>
    {
        var employeeById = TuberDrivers.FirstOrDefault(td => td.Id == firstDriverId);
        if (employeeById == null)
            return Results.NotFound();

        var tuberDeliveryList = TuberOrders.Where(to => to.TuberDriverId == firstDriverId).ToList();

        var employeeByIdDTO = new TuberDriverDTO
        {
            Id = employeeById.Id,
            Name = employeeById.Name,
            TuberDeliveries = tuberDeliveryList,
        };
        return Results.Ok(employeeByIdDTO);
    }
);

app.Run();

//don't touch or move this!
public partial class Program { }
