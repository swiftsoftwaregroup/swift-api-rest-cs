using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using SwiftAPI.Data;
using SwiftAPI.Models;
using Xunit;

namespace SwiftAPI.Tests
{
    public class BookApiTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly SqliteConnection _connection;

        public BookApiTests(WebApplicationFactory<Program> factory)
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Test");
            
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    services.AddDbContext<ApplicationDbContext>(options =>
                    {
                        options.UseSqlite(_connection);
                    });

                    var sp = services.BuildServiceProvider();

                    using (var scope = sp.CreateScope())
                    {
                        var scopedServices = scope.ServiceProvider;
                        var db = scopedServices.GetRequiredService<ApplicationDbContext>();
                        db.Database.EnsureCreated();
                    }
                });
            });
        }

        public void Dispose()
        {
            _connection.Close();
        }

        [Fact]
        public async Task CreateBook_ReturnsCreatedStatus()
        {
            // Arrange
            var client = _factory.CreateClient();
            var newBook = new BookDto
            {
                Title = "Test Book",
                Author = "Test Author",
                DatePublished = DateTime.Now,
                CoverImage = "http://example.com/image.jpg"
            };

            // Act
            var response = await client.PostAsJsonAsync("/books", newBook);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var createdBook = await response.Content.ReadFromJsonAsync<Book>();
            Assert.NotNull(createdBook);
            Assert.Equal(newBook.Title, createdBook.Title);
        }

        [Fact]
        public async Task GetBooks_ReturnsOkStatus()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/books");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var books = await response.Content.ReadFromJsonAsync<List<Book>>();
            Assert.NotNull(books);
        }

        [Fact]
        public async Task GetBook_WithValidId_ReturnsOkStatus()
        {
            // Arrange
            var client = _factory.CreateClient();
            var newBook = new BookDto
            {
                Title = "Test Book",
                Author = "Test Author",
                DatePublished = DateTime.Now,
                CoverImage = "http://example.com/image.jpg"
            };
            var createResponse = await client.PostAsJsonAsync("/books", newBook);
            var createdBook = await createResponse.Content.ReadFromJsonAsync<Book>();

            // Act
            var response = await client.GetAsync($"/books/{createdBook?.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var book = await response.Content.ReadFromJsonAsync<Book>();
            Assert.NotNull(book);
            Assert.Equal(newBook.Title, book.Title);
        }

        [Fact]
        public async Task UpdateBook_WithValidId_ReturnsNoContentStatus()
        {
            // Arrange
            var client = _factory.CreateClient();
            var newBook = new BookDto
            {
                Title = "Test Book",
                Author = "Test Author",
                DatePublished = DateTime.Now,
                CoverImage = "http://example.com/image.jpg"
            };
            var createResponse = await client.PostAsJsonAsync("/books", newBook);
            var createdBook = await createResponse.Content.ReadFromJsonAsync<Book>();

            var updatedBook = new BookDto
            {
                Title = "Updated Test Book",
                Author = "Updated Test Author",
                DatePublished = DateTime.Now.AddDays(1),
                CoverImage = "http://example.com/updated-image.jpg"
            };

            // Act
            var response = await client.PutAsJsonAsync($"/books/{createdBook?.Id}", updatedBook);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task DeleteBook_WithValidId_ReturnsOkStatus()
        {
            // Arrange
            var client = _factory.CreateClient();
            var newBook = new BookDto
            {
                Title = "Test Book",
                Author = "Test Author",
                DatePublished = DateTime.Now,
                CoverImage = "http://example.com/image.jpg"
            };
            var createResponse = await client.PostAsJsonAsync("/books", newBook);
            var createdBook = await createResponse.Content.ReadFromJsonAsync<Book>();

            // Act
            var response = await client.DeleteAsync($"/books/{createdBook?.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}