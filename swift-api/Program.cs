using Microsoft.EntityFrameworkCore;
using SwiftAPI.Data;
using SwiftAPI.Models;
using Microsoft.Data.Sqlite;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseKestrel(options => options.ConfigureEndpointDefaults(o => o.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2));

// Load .env file if it exists
if (File.Exists(".env"))
{
    DotNetEnv.Env.Load();
}

// Configure the database
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
SqliteConnection? keepAliveConnection = null;

if (string.IsNullOrEmpty(databaseUrl))
{
    // Use in-memory SQLite database if DATABASE_URL is not provided
    keepAliveConnection = new SqliteConnection("Data Source=:memory:");
    keepAliveConnection.Open();

    builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(keepAliveConnection));
}
else
{
    // Use SQLite with the provided connection string
    var connectionString = databaseUrl.Replace("sqlite:///", "Data Source=");
    builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connectionString));
}

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Books API",
        Description = "A simple API for managing books",
        Version = "v1"
    });
});

var app = builder.Build();

// Ensure the database is created and schema is applied
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.EnsureCreated();
}

// Dispose the keep-alive connection when the application shuts down
app.Lifetime.ApplicationStopping.Register(() => {
    keepAliveConnection?.Dispose();
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(c =>
    {
        c.RouteTemplate = "openapi/{documentName}/openapi.json";
    });
    
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/openapi/v1/openapi.json", "Books API V1");
        c.RoutePrefix = "docs";
    });
    
    // Add Redoc UI
    app.UseStaticFiles();
    app.UseFileServer(new FileServerOptions
    {
        RequestPath = "/redoc",
        FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "redoc"))
    });

    app.MapGet("/redoc", async context =>
    {
        context.Response.ContentType = "text/html";
        await context.Response.SendFileAsync(Path.Combine(Directory.GetCurrentDirectory(), "redoc", "index.html"));
    });    
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
.WithOpenApi(operation => new OpenApiOperation(operation)
{
    Summary = "Create a new book",
    Description = "Creates a new book entry in the database",
    Tags = new List<OpenApiTag> { new() { Name = "Books" } }
})
.Produces<Book>(StatusCodes.Status201Created)
.ProducesValidationProblem();

app.MapGet("/books", async (ApplicationDbContext db, int skip = 0, int limit = 100) =>
    await db.Books.Skip(skip).Take(limit).ToListAsync())
.WithName("GetBooks")
.WithOpenApi(operation => new OpenApiOperation(operation)
{
    Summary = "Get all books",
    Description = "Retrieves a list of all books in the database",
    Tags = new List<OpenApiTag> { new() { Name = "Books" } }
})
.Produces<List<Book>>();

app.MapGet("/books/{id}", async (ApplicationDbContext db, int id) =>
    await db.Books.FindAsync(id)
        is Book book
            ? Results.Ok(book)
            : Results.NotFound())
.WithName("GetBook")
.WithOpenApi(operation => new OpenApiOperation(operation)
{
    Summary = "Get a specific book",
    Description = "Retrieves a specific book by its ID",
    Tags = new List<OpenApiTag> { new() { Name = "Books" } }
})
.Produces<Book>()
.ProducesProblem(StatusCodes.Status404NotFound);

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
.WithOpenApi(operation => new OpenApiOperation(operation)
{
    Summary = "Update a book",
    Description = "Updates an existing book's information",
    Tags = new List<OpenApiTag> { new() { Name = "Books" } }
})
.ProducesValidationProblem()
.ProducesProblem(StatusCodes.Status404NotFound)
.Produces(StatusCodes.Status204NoContent);

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
.WithOpenApi(operation => new OpenApiOperation(operation)
{
    Summary = "Delete a book",
    Description = "Deletes a specific book from the database",
    Tags = new List<OpenApiTag> { new() { Name = "Books" } }
})
.Produces<Book>()
.ProducesProblem(StatusCodes.Status404NotFound);

app.Run();