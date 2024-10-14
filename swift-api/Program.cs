using Microsoft.EntityFrameworkCore;
using SwiftAPI.Data;
using SwiftAPI.Models;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseKestrel(options => options.ConfigureEndpointDefaults(o => o.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/books", async (BookDto bookDto, ApplicationDbContext db) =>
{
    var book = new Book
    {
        Title = bookDto.Title,
        Author = bookDto.Author,
        DatePublished = bookDto.DatePublished,
        CoverImage = bookDto.CoverImage
    };

    db.Books.Add(book);
    await db.SaveChangesAsync();

    return Results.Created($"/books/{book.Id}", book);
})
.WithName("CreateBook")
.WithOpenApi();

app.MapGet("/books", async (ApplicationDbContext db, int skip = 0, int limit = 100) =>
    await db.Books.Skip(skip).Take(limit).ToListAsync())
.WithName("GetBooks")
.WithOpenApi();

app.MapGet("/books/{id}", async (ApplicationDbContext db, int id) =>
    await db.Books.FindAsync(id)
        is Book book
            ? Results.Ok(book)
            : Results.NotFound())
.WithName("GetBook")
.WithOpenApi();

app.MapPut("/books/{id}", async (ApplicationDbContext db, int id, BookDto bookDto) =>
{
    var book = await db.Books.FindAsync(id);

    if (book is null) return Results.NotFound();

    book.Title = bookDto.Title;
    book.Author = bookDto.Author;
    book.DatePublished = bookDto.DatePublished;
    book.CoverImage = bookDto.CoverImage;

    await db.SaveChangesAsync();

    return Results.NoContent();
})
.WithName("UpdateBook")
.WithOpenApi();

app.MapDelete("/books/{id}", async (ApplicationDbContext db, int id) =>
{
    if (await db.Books.FindAsync(id) is Book book)
    {
        db.Books.Remove(book);
        await db.SaveChangesAsync();
        return Results.Ok(book);
    }

    return Results.NotFound();
})
.WithName("DeleteBook")
.WithOpenApi();

app.Run();
