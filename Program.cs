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

// Get all materials (excluding materials that have an OutOfCirculationSince value)
app.MapGet("/materials", (LoncotesLibraryDbContext db) =>
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
app.MapGet("materials/{id}", (LoncotesLibraryDbContext db, int id) =>
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
            Checkouts = m.Checkouts.Select(c => new CheckoutDTO
            {
                Id = c.Id,
                MaterialId = c.MaterialId,
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
        .Single(c => c.Id == id);
});

// Post a new material
app.MapPost("/materials", (LoncotesLibraryDbContext db, Material newMaterial) =>
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
app.MapPut("/materials/{id}", (LoncotesLibraryDbContext db, int id, Material material) =>
{
    Material materialToUpdate = db.Materials.SingleOrDefault(material => material.Id == id);
    if (materialToUpdate == null)
    {
        return Results.NotFound();
    }
    materialToUpdate.MaterialName = material.MaterialName;
    materialToUpdate.MaterialTypeId = material.MaterialTypeId;
    materialToUpdate.GenreId = material.GenreId;
    materialToUpdate.OutOfCirculationSince = DateTime.Now;

    db.SaveChanges();
    return Results.NoContent();
});

// Get MaterialTypes
app.MapGet("/materialtypes", (LoncotesLibraryDbContext db) =>
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
app.MapGet("/genres", (LoncotesLibraryDbContext db) =>
{
    return db.Genres
        .Select(g => new GenreDTO
    {
            Id = g.Id,
            Name = g.Name
    });
});

// Get patrons
app.MapGet("/patrons", (LoncotesLibraryDbContext db) => 
{
    return db.Patrons
        .Select(p => new PatronDTO
        {
            Id = p.Id,
            FirstName = p.FirstName,
            LastName = p.LastName,
            Email = p.Email,
            Address = p.Address,
            IsActive = p.IsActive
        });
});

// Get patron by Id with their associated checkouts
app.MapGet("patrons/{id}", (LoncotesLibraryDbContext db, int id) =>
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
            Checkouts = p.Checkouts.Select(c => new CheckoutDTO
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
app.MapPut("/patrons/{id}", (LoncotesLibraryDbContext db, int id, Patron patron) =>
{
    Patron patronToUpdate = db.Patrons.SingleOrDefault(patron => patron.Id == id);
    if (patronToUpdate == null)
    {
        return Results.NotFound();
    }
    
    patronToUpdate.Email = patron.Email;
    patronToUpdate.Address = patron.Address;

    db.SaveChanges();
    return Results.NoContent();
});

// Deactivate a patron
app.MapPut("/patrons/deactivate/{id}", (LoncotesLibraryDbContext db, int id, Patron patron) =>
{
    Patron patronToUpdate = db.Patrons.SingleOrDefault(patron => patron.Id == id);
    if (patronToUpdate == null)
    {
        return Results.NotFound();
    }
    
    patronToUpdate.IsActive = false;

    db.SaveChanges();
    return Results.NoContent();
});

// Checkout a material
app.MapPost("/checkouts", (LoncotesLibraryDbContext db, Checkout newCheckout) =>
{
    try
    {
        newCheckout.CheckoutDate = DateTime.Now; // set CheckoutDate to DateTime.Now
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
app.MapPost("/checkouts/return", (LoncotesLibraryDbContext db, Checkout returnCheckout) =>
{
    try
    {
        returnCheckout.ReturnDate = DateTime.Now; // set CheckoutDate to DateTime.Now
        db.Checkouts.Add(returnCheckout);
        db.SaveChanges();
        return Results.Created($"/checkouts/return/{returnCheckout.Id}", returnCheckout);
    }
    catch (DbUpdateException)
    {
        return Results.BadRequest("Invalid data submitted");
    }
});

app.Run();
