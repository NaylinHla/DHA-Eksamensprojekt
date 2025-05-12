using Core.Domain.Exceptions;
using Infrastructure.Postgres.Postgresql.Data;
using Infrastructure.Postgres.Scaffolding;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Startup.Tests.GreenhouseDeviceTests;

public class GreenhouseDeviceServiceTests
{
    private MyDbContext _context = null!;
    private GreenhouseDeviceRepository _repository = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<MyDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new MyDbContext(options);
        _repository = new GreenhouseDeviceRepository(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    [Test]
    public void GetDeviceByIdAsync_ShouldThrowNotFoundException_WhenDeviceNotFound()
    {
        var nonExistingId = Guid.NewGuid();

        var ex = Assert.ThrowsAsync<NotFoundException>(async () =>
        {
            await _repository.GetDeviceOwnerUserId(nonExistingId);
        });

        Assert.That(ex!.Message, Does.Contain("not found"));
    }
}