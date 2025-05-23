using Core.Domain.Exceptions;
using Fleck;
using Infrastructure.Websocket;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Startup.Tests.EventTests;

public class WebSocketConnectionManagerTests
{
    private WebSocketConnectionManager _connectionManager;

    [SetUp]
    public void SetUp()
    {
        var loggerMock = new Mock<ILogger<WebSocketConnectionManager>>();
        _connectionManager = new WebSocketConnectionManager(loggerMock.Object);
    }

    [Test]
    public void GetConnectionIdToSocketDictionary_ShouldReturnCorrectData()
    {
        // Arrange
        const string clientId = "client1";
        var socketMock = new Mock<IWebSocketConnection>();

        // Mock the IWebSocketConnectionInfo, which is returned from ConnectionInfo property
        var connectionInfoMock = new Mock<IWebSocketConnectionInfo>();
        connectionInfoMock.Setup(c => c.Id).Returns(Guid.NewGuid());
        socketMock.Setup(s => s.ConnectionInfo).Returns(connectionInfoMock.Object);

        _ = _connectionManager.OnOpen(socketMock.Object, clientId);

        // Act
        var result = _connectionManager.GetConnectionIdToSocketDictionary();

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(result.ContainsKey(clientId), Is.True);
            Assert.That(result[clientId], Is.EqualTo(socketMock.Object));
        });
    }

    [Test]
    public async Task OnOpen_ShouldAddConnection()
    {
        // Arrange
        const string clientId = "client1";
        var socketMock = new Mock<IWebSocketConnection>();

        // Mock the IWebSocketConnectionInfo, which is returned from ConnectionInfo property
        var connectionInfoMock = new Mock<IWebSocketConnectionInfo>();
        connectionInfoMock.Setup(c => c.Id).Returns(Guid.NewGuid());
        socketMock.Setup(s => s.ConnectionInfo).Returns(connectionInfoMock.Object);

        // Act
        await _connectionManager.OnOpen(socketMock.Object, clientId);

        // Assert
        var result = _connectionManager.GetConnectionIdToSocketDictionary();
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result.ContainsKey(clientId), Is.True);
    }

    [Test]
    public async Task OnClose_ShouldRemoveConnection()
    {
        // Arrange
        const string clientId = "client1";
        var socketMock = new Mock<IWebSocketConnection>();

        // Mock the IWebSocketConnectionInfo, which is returned from ConnectionInfo property
        var connectionInfoMock = new Mock<IWebSocketConnectionInfo>();
        connectionInfoMock.Setup(c => c.Id).Returns(Guid.NewGuid());
        socketMock.Setup(s => s.ConnectionInfo).Returns(connectionInfoMock.Object);

        // Act
        await _connectionManager.OnOpen(socketMock.Object, clientId);
        await _connectionManager.OnClose(socketMock.Object, clientId);

        // Assert
        var result = _connectionManager.GetConnectionIdToSocketDictionary();
        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task AddToTopic_ShouldAddMember()
    {
        // Arrange
        const string clientId = "client1";
        const string topic = "test/topic";
        var socketMock = new Mock<IWebSocketConnection>();

        // Mock the IWebSocketConnectionInfo, which is returned from ConnectionInfo property
        var connectionInfoMock = new Mock<IWebSocketConnectionInfo>();
        connectionInfoMock.Setup(c => c.Id).Returns(Guid.NewGuid());
        socketMock.Setup(s => s.ConnectionInfo).Returns(connectionInfoMock.Object);

        await _connectionManager.OnOpen(socketMock.Object, clientId);

        // Act
        await _connectionManager.AddToTopic(topic, clientId);

        // Assert
        var members = await _connectionManager.GetMembersFromTopicId(topic);
        Assert.That(members, Contains.Item(clientId));
    }

    [Test]
    public async Task RemoveFromTopic_ShouldRemoveMember()
    {
        // Arrange
        const string clientId = "client1";
        const string topic = "test/topic";
        var socketMock = new Mock<IWebSocketConnection>();

        // Mock the IWebSocketConnectionInfo, which is returned from ConnectionInfo property
        var connectionInfoMock = new Mock<IWebSocketConnectionInfo>();
        connectionInfoMock.Setup(c => c.Id).Returns(Guid.NewGuid());
        socketMock.Setup(s => s.ConnectionInfo).Returns(connectionInfoMock.Object);

        await _connectionManager.OnOpen(socketMock.Object, clientId);
        await _connectionManager.AddToTopic(topic, clientId);

        // Act
        await _connectionManager.RemoveFromTopic(topic, clientId);

        // Assert
        var members = await _connectionManager.GetMembersFromTopicId(topic);
        Assert.That(members, Does.Not.Contain(clientId));
    }

    [Test]
    public async Task BroadcastToTopic_ShouldSendMessageToMembers()
    {
        // Arrange
        const string topic = "test/topic";
        var message = new { text = "Hello" };
        const string clientId = "client1";
        var socketMock = new Mock<IWebSocketConnection>();

        // Mock the IWebSocketConnectionInfo, which is returned from ConnectionInfo property
        var connectionInfoMock = new Mock<IWebSocketConnectionInfo>();
        connectionInfoMock.Setup(c => c.Id).Returns(Guid.NewGuid());
        socketMock.Setup(s => s.ConnectionInfo).Returns(connectionInfoMock.Object);
        socketMock.Setup(s => s.IsAvailable).Returns(true); // Ensure the socket is available

        // Act
        await _connectionManager.OnOpen(socketMock.Object, clientId); // Open connection
        await _connectionManager.AddToTopic(topic, clientId); // Add to topic

        // Assert the socket was added to the topic
        var members = await _connectionManager.GetMembersFromTopicId(topic);
        Assert.That(members, Contains.Item(clientId));

        // Broadcast the message to the topic
        await _connectionManager.BroadcastToTopic(topic, message);

        // Assert that Send was called on the mock socket
        socketMock.Verify(s => s.Send(It.IsAny<string>()), Times.Once);
    }


    [Test]
    public async Task GetTopicsFromMemberId_ShouldReturnCorrectTopics()
    {
        // Arrange
        const string clientId = "client1";
        const string topic = "test/topic";
        var socketMock = new Mock<IWebSocketConnection>();

        // Mock the IWebSocketConnectionInfo, which is returned from ConnectionInfo property
        var connectionInfoMock = new Mock<IWebSocketConnectionInfo>();
        connectionInfoMock.Setup(c => c.Id).Returns(Guid.NewGuid());
        socketMock.Setup(s => s.ConnectionInfo).Returns(connectionInfoMock.Object);

        await _connectionManager.OnOpen(socketMock.Object, clientId);
        await _connectionManager.AddToTopic(topic, clientId);

        // Act
        var topics = await _connectionManager.GetTopicsFromMemberId(clientId);

        // Assert
        Assert.That(topics, Contains.Item(topic));
    }

    [Test]
    public async Task GetMembersFromTopicId_ShouldReturnCorrectMembers()
    {
        // Arrange
        const string clientId = "client1";
        const string topic = "test/topic";
        var socketMock = new Mock<IWebSocketConnection>();

        // Mock the IWebSocketConnectionInfo, which is returned from ConnectionInfo property
        var connectionInfoMock = new Mock<IWebSocketConnectionInfo>();
        connectionInfoMock.Setup(c => c.Id).Returns(Guid.NewGuid()); // Return a mocked Guid
        socketMock.Setup(s => s.ConnectionInfo).Returns(connectionInfoMock.Object);

        await _connectionManager.OnOpen(socketMock.Object, clientId);
        await _connectionManager.AddToTopic(topic, clientId);

        // Act
        var members = await _connectionManager.GetMembersFromTopicId(topic);

        // Assert
        Assert.That(members, Contains.Item(clientId));
    }

    [Test]
    public void GetSocketIdToClientIdDictionary_ShouldReturnCorrectData()
    {
        // Arrange
        const string clientId = "client1";
        var socketMock = new Mock<IWebSocketConnection>();
        var connectionInfoMock = new Mock<IWebSocketConnectionInfo>();
        connectionInfoMock.Setup(c => c.Id).Returns(Guid.NewGuid());
        socketMock.Setup(s => s.ConnectionInfo).Returns(connectionInfoMock.Object);

        _ = _connectionManager.OnOpen(socketMock.Object, clientId);

        // Act
        var result = _connectionManager.GetSocketIdToClientIdDictionary();

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result.ContainsKey(socketMock.Object.ConnectionInfo.Id.ToString()), Is.True);
    }

    [Test]
    public void GetClientIdFromSocket_ShouldReturnCorrectClientId()
    {
        // Arrange
        const string clientId = "client1";
        var socketMock = new Mock<IWebSocketConnection>();
        var connectionInfoMock = new Mock<IWebSocketConnectionInfo>();
        connectionInfoMock.Setup(c => c.Id).Returns(Guid.NewGuid());
        socketMock.Setup(s => s.ConnectionInfo).Returns(connectionInfoMock.Object);

        _ = _connectionManager.OnOpen(socketMock.Object, clientId);

        // Act
        var result = _connectionManager.GetClientIdFromSocket(socketMock.Object);

        // Assert
        Assert.That(result, Is.EqualTo(clientId));
    }

    [Test]
    public void GetSocketFromClientId_ShouldReturnCorrectSocket()
    {
        // Arrange
        const string clientId = "client1";
        var socketMock = new Mock<IWebSocketConnection>();
        var connectionInfoMock = new Mock<IWebSocketConnectionInfo>();
        connectionInfoMock.Setup(c => c.Id).Returns(Guid.NewGuid());
        socketMock.Setup(s => s.ConnectionInfo).Returns(connectionInfoMock.Object);

        _ = _connectionManager.OnOpen(socketMock.Object, clientId);

        // Act
        var result = _connectionManager.GetSocketFromClientId(clientId);

        // Assert
        Assert.That(result, Is.EqualTo(socketMock.Object));
    }

    [Test]
    public async Task OnOpen_ShouldReplaceOldSocketIfExists()
    {
        const string clientId = "client1";

        var oldSocketMock = new Mock<IWebSocketConnection>();
        var oldConnectionInfo = new Mock<IWebSocketConnectionInfo>();
        oldConnectionInfo.Setup(c => c.Id).Returns(Guid.NewGuid());
        oldSocketMock.Setup(s => s.ConnectionInfo).Returns(oldConnectionInfo.Object);

        var newSocketMock = new Mock<IWebSocketConnection>();
        var newConnectionInfo = new Mock<IWebSocketConnectionInfo>();
        newConnectionInfo.Setup(c => c.Id).Returns(Guid.NewGuid());
        newSocketMock.Setup(s => s.ConnectionInfo).Returns(newConnectionInfo.Object);

        await _connectionManager.OnOpen(oldSocketMock.Object, clientId);
        await _connectionManager.OnOpen(newSocketMock.Object, clientId);

        var currentSocket = _connectionManager.GetSocketFromClientId(clientId);
        Assert.That(currentSocket, Is.EqualTo(newSocketMock.Object));
    }

    [Test]
    public async Task BroadcastToTopic_ShouldLogErrorIfSendFails()
    {
        const string topic = "test/topic";
        const string clientId = "client1";
        var socketMock = new Mock<IWebSocketConnection>();
        var connectionInfoMock = new Mock<IWebSocketConnectionInfo>();
        connectionInfoMock.Setup(c => c.Id).Returns(Guid.NewGuid());
        socketMock.Setup(s => s.ConnectionInfo).Returns(connectionInfoMock.Object);
        socketMock.Setup(s => s.IsAvailable).Returns(true);
        socketMock.Setup(s => s.Send(It.IsAny<string>())).ThrowsAsync(new Exception("Send failed"));

        await _connectionManager.OnOpen(socketMock.Object, clientId);
        await _connectionManager.AddToTopic(topic, clientId);

        // No assert needed — we're just ensuring no exception propagates
        Assert.DoesNotThrowAsync(() => _connectionManager.BroadcastToTopic(topic, new { msg = "fail" }));
    }

    [Test]
    public void GetClientIdFromSocket_ShouldThrowIfSocketNotRegistered()
    {
        var socketMock = new Mock<IWebSocketConnection>();
        var connectionInfoMock = new Mock<IWebSocketConnectionInfo>();
        connectionInfoMock.Setup(c => c.Id).Returns(Guid.NewGuid());
        socketMock.Setup(s => s.ConnectionInfo).Returns(connectionInfoMock.Object);

        Assert.Throws<NotFoundException>(() => _connectionManager.GetClientIdFromSocket(socketMock.Object));
    }

    [Test]
    public void GetSocketFromClientId_ShouldThrowIfClientNotFound()
    {
        Assert.Throws<NotFoundException>(() => _connectionManager.GetSocketFromClientId("nonexistent"));
    }
    
    [Test]
    public void BroadcastToTopic_ShouldDoNothingIfTopicNotFound()
    {
        // Arrange
        var message = new { msg = "hello" };

        // Act & Assert (should not throw or send anything)
        Assert.DoesNotThrowAsync(() => _connectionManager.BroadcastToTopic("nonexistent-topic", message));
    }
    
    [Test]
    public async Task BroadcastToTopic_ShouldSkipUnavailableSockets()
    {
        const string topic = "test/topic";
        const string clientId = "client1";
        var socketMock = new Mock<IWebSocketConnection>();
        var connectionInfoMock = new Mock<IWebSocketConnectionInfo>();

        connectionInfoMock.Setup(c => c.Id).Returns(Guid.NewGuid());
        socketMock.Setup(s => s.ConnectionInfo).Returns(connectionInfoMock.Object);
        socketMock.Setup(s => s.IsAvailable).Returns(false); // Make socket unavailable

        await _connectionManager.OnOpen(socketMock.Object, clientId);
        await _connectionManager.AddToTopic(topic, clientId);

        // Act
        await _connectionManager.BroadcastToTopic(topic, new { msg = "skip" });

        // Assert that Send was never called
        socketMock.Verify(s => s.Send(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task BroadcastToTopic_ShouldSendOnlyToAvailableSockets()
    {
        const string topic = "group/topic";
        const string client1 = "client1";
        const string client2 = "client2";

        // Client 1 - Available
        var socketMock1 = new Mock<IWebSocketConnection>();
        var connectionInfoMock1 = new Mock<IWebSocketConnectionInfo>();
        connectionInfoMock1.Setup(c => c.Id).Returns(Guid.NewGuid());
        socketMock1.Setup(s => s.ConnectionInfo).Returns(connectionInfoMock1.Object);
        socketMock1.Setup(s => s.IsAvailable).Returns(true);

        // Client 2 - Unavailable
        var socketMock2 = new Mock<IWebSocketConnection>();
        var connectionInfoMock2 = new Mock<IWebSocketConnectionInfo>();
        connectionInfoMock2.Setup(c => c.Id).Returns(Guid.NewGuid());
        socketMock2.Setup(s => s.ConnectionInfo).Returns(connectionInfoMock2.Object);
        socketMock2.Setup(s => s.IsAvailable).Returns(false);

        await _connectionManager.OnOpen(socketMock1.Object, client1);
        await _connectionManager.OnOpen(socketMock2.Object, client2);

        await _connectionManager.AddToTopic(topic, client1);
        await _connectionManager.AddToTopic(topic, client2);

        // Act
        await _connectionManager.BroadcastToTopic(topic, new { msg = "hi" });

        // Assert
        socketMock1.Verify(s => s.Send(It.IsAny<string>()), Times.Once);
        socketMock2.Verify(s => s.Send(It.IsAny<string>()), Times.Never);
    }
}