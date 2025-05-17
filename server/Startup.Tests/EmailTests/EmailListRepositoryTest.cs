using Core.Domain.Entities;
using Infrastructure.Postgres.Postgresql.Data;
using Infrastructure.Postgres.Scaffolding;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Startup.Tests.EmailTests;

[TestFixture]
public class EmailListRepositoryTest
{
    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<MyDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new MyDbContext(options);
        _repository = new EmailListRepository(_dbContext);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Dispose();
    }

    private MyDbContext _dbContext = null!;
    private EmailListRepository _repository = null!;

    [Test]
    public void Add_ShouldAddEmail()
    {
        var entry = new EmailList { Email = "test@example.com" };

        _repository.Add(entry);
        _repository.Save();

        var result = _dbContext.EmailList.ToList();

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].Email, Is.EqualTo("test@example.com"));
    }

    [Test]
    public void GetAllEmails_ShouldReturnAll()
    {
        _dbContext.EmailList.AddRange(new List<EmailList>
        {
            new() { Email = "a@example.com" },
            new() { Email = "b@example.com" }
        });
        _dbContext.SaveChanges();

        var result = _repository.GetAllEmails();

        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result, Does.Contain("a@example.com"));
        Assert.That(result, Does.Contain("b@example.com"));
    }

    [Test]
    public void RemoveByEmail_ShouldRemoveIfExists()
    {
        _dbContext.EmailList.Add(new EmailList { Email = "remove@example.com" });
        _dbContext.SaveChanges();

        _repository.RemoveByEmail("remove@example.com");
        _repository.Save();

        var exists = _dbContext.EmailList.Any(e => e.Email == "remove@example.com");
        Assert.That(exists, Is.False);
    }

    [Test]
    public void RemoveByEmail_ShouldDoNothing_IfNotExists()
    {
        _repository.RemoveByEmail("nonexistent@example.com");
        _repository.Save();

        var count = _dbContext.EmailList.Count();
        Assert.That(count, Is.EqualTo(0));
    }

    [Test]
    public void EmailExists_ShouldReturnTrue_IfExists()
    {
        _dbContext.EmailList.Add(new EmailList { Email = "exists@example.com" });
        _dbContext.SaveChanges();

        var result = _repository.EmailExists("exists@example.com");

        Assert.That(result, Is.True);
    }

    [Test]
    public void EmailExists_ShouldReturnFalse_IfNotExists()
    {
        var result = _repository.EmailExists("missing@example.com");

        Assert.That(result, Is.False);
    }
}