using Library.Models;
using Library.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;
using System.Reflection.Metadata.Ecma335;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// allows passing datetimes without time zone data 
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// allows api endpoints to access the database through Entity Framework Core
builder.Services.AddNpgsql<LoncotesLibraryDbContext>(builder.Configuration["LoncotesLibraryDbConnectionString"]);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//TO-DO: Probaly will need /api for endponts once start on the client 

// Get all materials (excluding materials that have an OutOfCirculationSince value)
app.MapGet("/api/materials", (LoncotesLibraryDbContext db) =>
{
    return db.Materials
        .Where(m => m.OutOfCirculationSince == null)
        .Select(m => new MaterialDTO
        {
            Id = m.Id,
            MaterialName = m.MaterialName,
            MaterialTypeId = m.MaterialTypeId,
            MaterialType = new MaterialTypeDTO
            {
                Id = m.MaterialType.Id,
                Name = m.MaterialType.Name,
                CheckoutDays = m.MaterialType.CheckoutDays
            },
            GenreId = m.GenreId,
            Genre = new GenreDTO
            {
                Id = m.Genre.Id,
                Name = m.Genre.Name
            },
            OutOfCirculationSince = m.OutOfCirculationSince
        })
        .ToList();
});

// Get material by Id
app.MapGet("/api/materials/{id}", (LoncotesLibraryDbContext db, int id) =>
{
    return db.Materials
        .Include(m => m.MaterialType)
        .Include(m => m.Genre)
        .Include(m => m.Checkouts)
        .ThenInclude(c => c.Patron)
        .Select(m => new MaterialDTO
        {
            Id = m.Id,
            MaterialName = m.MaterialName,
            MaterialTypeId = m.MaterialTypeId,
            MaterialType = new MaterialTypeDTO
            {
                Id = m.MaterialType.Id,
                Name = m.MaterialType.Name,
                CheckoutDays = m.MaterialType.CheckoutDays
            },
            GenreId = m.GenreId,
            Genre = new GenreDTO
            {
                Id = m.Genre.Id,
                Name = m.Genre.Name
            },
            OutOfCirculationSince = m.OutOfCirculationSince,
            Checkouts = m.Checkouts.Select(c => new CheckoutWithLateFeeDTO
            {
                Id = c.Id,
                PatronId = c.PatronId,
                Patron = new PatronDTO
                {
                    Id = c.Patron.Id,
                    FirstName = c.Patron.FirstName,
                    LastName = c.Patron.LastName,
                    Email = c.Patron.Address,
                    Address = c.Patron.Address,
                    IsActive = c.Patron.IsActive
                },
                CheckoutDate = c.CheckoutDate,
                ReturnDate = c.ReturnDate
            }).ToList()
        })
        .SingleOrDefault(m => m.Id == id);
});

// Post a new material
app.MapPost("/api/materials", (LoncotesLibraryDbContext db, Material newMaterial) =>
{
    try
    {
        db.Materials.Add(newMaterial);
        db.SaveChanges();
        return Results.Created($"/materials/{newMaterial.Id}", newMaterial);
    }
    catch (DbUpdateException)
    {
        return Results.BadRequest("Invalid data submitted");
    }
});

// Remove material from circulation by setting OutOfCirculationSince property to DateTime.Now
app.MapPut("/api/materials/{id}", (LoncotesLibraryDbContext db, int id, Material material) =>
{
    Material materialToUpdate = db.Materials.SingleOrDefault(material => material.Id == id);
    if (materialToUpdate == null)
    {
        return Results.NotFound();
    }
    // materialToUpdate.MaterialName = material.MaterialName;
    // materialToUpdate.MaterialTypeId = material.MaterialTypeId;
    // materialToUpdate.GenreId = material.GenreId;
    materialToUpdate.OutOfCirculationSince = DateTime.Now;

    db.SaveChanges();
    return Results.Ok(materialToUpdate);
});

// Get MaterialTypes
app.MapGet("/api/materialtypes", (LoncotesLibraryDbContext db) =>
{
    return db.MaterialTypes
        .Select(mt => new MaterialTypeDTO
        {
            Id = mt.Id,
            Name = mt.Name,
            CheckoutDays = mt.CheckoutDays
        })
        .ToList();
});

// Get genres
app.MapGet("/api/genres", (LoncotesLibraryDbContext db) =>
{
    return db.Genres
        .Select(g => new GenreDTO
        {
            Id = g.Id,
            Name = g.Name
        });
});

// Get patrons
app.MapGet("/api/patrons", (LoncotesLibraryDbContext db) =>
{
    return db.Patrons
        .Select(p => new PatronDTO
        {
            Id = p.Id,
            FirstName = p.FirstName,
            LastName = p.LastName,
            Email = p.Email,
            Address = p.Address,
            IsActive = p.IsActive,
            Checkouts = p.Checkouts.Select(c => new CheckoutWithLateFeeDTO
            {
                Id = c.Id,
                MaterialId = c.MaterialId,
                CheckoutDate = c.CheckoutDate,
                ReturnDate = c.ReturnDate
            }).ToList()
        });
});

// Get patron by Id with their associated checkouts
app.MapGet("/api/patrons/{id}", (LoncotesLibraryDbContext db, int id) =>
{
    return db.Patrons
        .Include(p => p.Checkouts)
        .ThenInclude(c => c.Material)
        .ThenInclude(c => c.MaterialType)
        .Select(p => new PatronDTO
        {
            Id = p.Id,
            FirstName = p.FirstName,
            LastName = p.LastName,
            Email = p.Email,
            Address = p.Address,
            IsActive = p.IsActive,
            Checkouts = p.Checkouts.Select(c => new CheckoutWithLateFeeDTO
            {
                Id = c.Id,
                MaterialId = c.MaterialId,
                Material = new MaterialDTO
                {
                    Id = c.Material.Id,
                    MaterialName = c.Material.MaterialName,
                    MaterialTypeId = c.Material.MaterialTypeId,
                    GenreId = c.Material.GenreId,
                    MaterialType = new MaterialTypeDTO
                    {
                        Id = c.Material.MaterialType.Id,
                        Name = c.Material.MaterialType.Name,
                        CheckoutDays = c.Material.MaterialType.CheckoutDays

                    }
                },
                CheckoutDate = c.CheckoutDate,
                ReturnDate = c.ReturnDate
            }).ToList()
        })
        .Single(c => c.Id == id);
});

// Update patron details
app.MapPut("/api/patrons/{id}", (LoncotesLibraryDbContext db, int id, Patron patron) =>
{
    Patron patronToUpdate = db.Patrons.SingleOrDefault(patron => patron.Id == id);
    if (patronToUpdate == null)
    {
        return Results.NotFound();
    }

    patronToUpdate.Email = patron.Email;
    patronToUpdate.Address = patron.Address;

    db.SaveChanges();
    return Results.Ok(patronToUpdate);
});

// Deactivate a patron
app.MapPut("/api/patrons/deactivate/{id}", (LoncotesLibraryDbContext db, int id, Patron patron) =>
{
    Patron patronToUpdate = db.Patrons.SingleOrDefault(patron => patron.Id == id);
    if (patronToUpdate == null)
    {
        return Results.NotFound();
    }

    patronToUpdate.IsActive = false;

    db.SaveChanges();
    return Results.Ok(patronToUpdate);
});

// Activate a patron
app.MapPut("/api/patrons/activate/{id}", (LoncotesLibraryDbContext db, int id, Patron patron) =>
{
    Patron patronToUpdate = db.Patrons.SingleOrDefault(patron => patron.Id == id);
    if (patronToUpdate == null)
    {
        return Results.NotFound();
    }

    patronToUpdate.IsActive = true;

    db.SaveChanges();
    return Results.Ok(patronToUpdate);
});


// Get all checkouts
app.MapGet("/api/checkouts", (LoncotesLibraryDbContext db) =>
{
    return db.Checkouts
        .Include(p => p.Patron)
        .Include(p => p.Material)
        .ThenInclude(m => m.MaterialType)
        .Select(p => new CheckoutDTO
        {
            Id = p.Id,
            CheckoutDate = p.CheckoutDate,
            ReturnDate = p.ReturnDate,
            MaterialId = p.MaterialId,
            Material = new MaterialDTO
            {
                Id = p.Material.Id,
                MaterialName = p.Material.MaterialName,
                MaterialTypeId = p.Material.MaterialTypeId,
                MaterialType = new MaterialTypeDTO 
                {
                    Id = p.Material.MaterialType.Id,
                    Name = p.Material.MaterialType.Name,
                    CheckoutDays = p.Material.MaterialType.CheckoutDays
                },
                GenreId = p.Material.GenreId,
                Genre = new GenreDTO
                {
                    Id = p.Material.Genre.Id,
                    Name = p.Material.Genre.Name
                }
            },
            PatronId = p.PatronId,
            Patron = new PatronDTO
            {
                Id = p.Patron.Id,
                FirstName = p.Patron.FirstName,
                LastName = p.Patron.LastName,
                Email = p.Patron.Email,
                Address = p.Patron.Address,
                IsActive = p.Patron.IsActive
            }
        })
        .ToList();
});

// Checkout a material
app.MapPost("/api/checkouts", (LoncotesLibraryDbContext db, Checkout newCheckout) =>
{

    try
    {
        newCheckout.CheckoutDate = DateTime.Today;

        db.Checkouts.Add(newCheckout);
        db.SaveChanges();
        return Results.Created($"/checkouts/{newCheckout.Id}", newCheckout);
    }
    catch (DbUpdateException)
    {
        return Results.BadRequest("Invalid data submitted");
    }
});

// Return a material
app.MapPut("/api/checkouts/{id}", (LoncotesLibraryDbContext db, int id, Checkout checkout) =>
{
    Checkout checkoutToUpdate = db.Checkouts.SingleOrDefault(checkout => checkout.Id == id);
    if (checkoutToUpdate == null)
    {
        return Results.NotFound();
    }

    checkoutToUpdate.ReturnDate = DateTime.Today;

    db.SaveChanges();
    return Results.Ok(checkoutToUpdate);
});

// Delete a checkout
app.MapDelete("/api/checkouts/{id}", (LoncotesLibraryDbContext db, int id) =>
{
    Checkout checkout = db.Checkouts.SingleOrDefault(c => c.Id == id);
    if (checkout == null)
    {
        return Results.NotFound();
    }
    db.Checkouts.Remove(checkout);
    db.SaveChanges();
    return Results.NoContent();

});

// Get only available materials
app.MapGet("/api/materials/available", (LoncotesLibraryDbContext db) =>
{
    return db.Materials
    .Where(m => m.OutOfCirculationSince == null)
    .Where(m => m.Checkouts.All(co => co.ReturnDate != null))
    .Select(m => new MaterialDTO
        {
            Id = m.Id,
            MaterialName = m.MaterialName,
            MaterialTypeId = m.MaterialTypeId,
            MaterialType = new MaterialTypeDTO
            {
                Id = m.MaterialType.Id,
                Name = m.MaterialType.Name,
                CheckoutDays = m.MaterialType.CheckoutDays
            },
            GenreId = m.GenreId,
            Genre = new GenreDTO
            {
                Id = m.Genre.Id,
                Name = m.Genre.Name
            },
            OutOfCirculationSince = m.OutOfCirculationSince
        })
        .ToList();
});

// Get overdue checkouts
app.MapGet("/api/checkouts/overdue", (LoncotesLibraryDbContext db) =>
{
    return db.Checkouts
    .Include(p => p.Patron)
    .Include(co => co.Material)
    .ThenInclude(m => m.MaterialType)
    .Where(co =>
        (DateTime.Today - co.CheckoutDate.Value).Days >
        co.Material.MaterialType.CheckoutDays &&
        co.ReturnDate == null)
        .Select(co => new CheckoutWithLateFeeDTO
        {
            Id = co.Id,
            MaterialId = co.MaterialId,
            Material = new MaterialDTO
            {
                Id = co.Material.Id,
                MaterialName = co.Material.MaterialName,
                MaterialTypeId = co.Material.MaterialTypeId,
                MaterialType = new MaterialTypeDTO
                {
                    Id = co.Material.MaterialTypeId,
                    Name = co.Material.MaterialType.Name,
                    CheckoutDays = co.Material.MaterialType.CheckoutDays
                },
                GenreId = co.Material.GenreId,
                Genre = new GenreDTO
                {
                    Id = co.Material.Genre.Id,
                    Name = co.Material.Genre.Name

                },
                OutOfCirculationSince = co.Material.OutOfCirculationSince
            },
            PatronId = co.PatronId,
            Patron = new PatronDTO
            {
                Id = co.Patron.Id,
                FirstName = co.Patron.FirstName,
                LastName = co.Patron.LastName,
                Address = co.Patron.Address,
                Email = co.Patron.Email,
                IsActive = co.Patron.IsActive
            },
            CheckoutDate = co.CheckoutDate,
            ReturnDate = co.ReturnDate
        })
    .ToList();
});


app.Run();
