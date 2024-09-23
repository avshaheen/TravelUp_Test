using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelUp.Application.Controllers;
using TravelUp.Application.Models;
using TravelUp.DataAccess;
using TravelUp.Domain.Entities;
using TravelUp.Models;

namespace TravelUp.Tests;

[TestFixture]
public class ItemControllerBackendTests : IDisposable
{
    private ItemController _controller;
    private AppDbContext _dbContext;
    private DbContextOptions<AppDbContext> _dbContextOptions;

    public ItemControllerBackendTests()
    {
        // Set up an in-memory database for testing
        _dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("TestDb")
            .Options;

        _dbContext = new AppDbContext(_dbContextOptions);
        _controller = new ItemController(_dbContext);
    }

    [SetUp]
    public void Setup()
    {
        // Optionally clear the database before each test
        using (var context = new AppDbContext(_dbContextOptions))
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }
    }

    [Test]
    public async Task Create_ReturnsSuccess_WithValidModel()
    {
        // Arrange
        var createItem = new CreateItemVM
        {
            Name = "Test Item",
            Description = "Test Description"
        };

        // Act
        var response = await _controller.Create(createItem) as JsonResult;

        response.Should().NotBeNull("The response should not be null.");
        var result = response.Value as JsonResponse; // Use the strongly typed model
        result.Should().NotBeNull("The result should not be null.");
        result.Success.Should().BeTrue("The result should indicate success.");

        var item = await _dbContext.Items.FirstOrDefaultAsync(i => i.Name == createItem.Name);
        item.Should().NotBeNull("The item should have been created.");
        item.Description.Should().Be(createItem.Description);
    }

    [Test]
    public async Task Create_ReturnsError_WhenModelIsNull()
    {
        // Act
        var response = await _controller.Create(null) as JsonResult;

        response.Should().NotBeNull("The response should not be null.");
        var result = response.Value as JsonResponse;
        result.Should().NotBeNull("The result should not be null.");
        result.Success.Should().BeFalse("The result should indicate failure.");
        result.Errors.Should().Contain("Model is null");
    }

    [Test]
    public async Task Index_ReturnsListOfItems()
    {
        // Arrange
        await _dbContext.Items.AddAsync(new Item { Name = "Test Item", Description = "Test Description" });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _controller.Index() as ViewResult;

        // Assert
        result.Should().NotBeNull();
        var model = result.Model as List<ItemVM>;
        model.Should().NotBeNull();
        model.Count.Should().BeGreaterThan(0);
    }

    [Test]
    public async Task Edit_ReturnsSuccess_WhenItemExists()
    {
        // Arrange
        var existingItem = new Item { Name = "Old Name", Description = "Old Description" };
        await _dbContext.Items.AddAsync(existingItem);
        await _dbContext.SaveChangesAsync();

        var editItem = new CreateItemVM
        {
            Name = "Updated Name",
            Description = "Updated Description"
        };

        // Act
        var response = await _controller.Edit(existingItem.Id, editItem) as JsonResult;

        // Assert
        response.Should().NotBeNull("The response should not be null.");
        var result = response.Value as JsonResponse; // Use the strongly typed model
        result.Should().NotBeNull("The result should not be null.");
        result.Success.Should().BeTrue("The result should indicate success.");

        var updatedItem = await _dbContext.Items.FindAsync(existingItem.Id);
        updatedItem.Should().NotBeNull("The updated item should exist.");
        updatedItem.Name.Should().Be(editItem.Name);
        updatedItem.Description.Should().Be(editItem.Description);
    }

    [Test]
    public async Task DeleteConfirmed_ReturnsSuccess_WhenItemExists()
    {
        // Arrange
        var existingItem = new Item { Name = "Item to Delete", Description = "Delete This Item" };
        await _dbContext.Items.AddAsync(existingItem);
        await _dbContext.SaveChangesAsync();

        // Act
        var response = await _controller.DeleteConfirmed(existingItem.Id) as JsonResult;

        // Assert
        response.Should().NotBeNull("The response should not be null.");
        var result = response.Value as JsonResponse;
        result.Should().NotBeNull("The result should not be null.");
        result.Success.Should().BeTrue("The result should indicate success.");

        var deletedItem = await _dbContext.Items.FindAsync(existingItem.Id);
        deletedItem.Should().NotBeNull("The deleted item should exist.");
        deletedItem.IsDeleted.Should().BeTrue("The item should be marked as deleted.");
    }

    public void Dispose()
    {
        _dbContext?.Dispose();
        // Ensure the database is deleted
        using (var context = new AppDbContext(_dbContextOptions))
        {
            context.Database.EnsureDeleted();
        }
    }
}
