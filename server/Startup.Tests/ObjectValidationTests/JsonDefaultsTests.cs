using Application;
using NUnit.Framework;
using System.Text.Json;

namespace Startup.Tests.ObjectValidationTests;

[TestFixture]
public class JsonDefaultsTests
{
    private static readonly object Sample = new { Foo = 1, Bar = 2 };
    
    [Test]
    public void WriteIndentedOptions_ShouldHaveWriteIndentedTrue()
    {
        Assert.That(JsonDefaults.WriteIndented.WriteIndented, Is.True);
    }

    [Test]
    public void MqttSerializeOptions_ShouldHaveWriteIndentedTrue()
    {
        Assert.That(JsonDefaults.MqttSerialize.WriteIndented, Is.True);
    }
    
    [Test]
    public void WriteIndentedOptions_ShouldProduceMultiLineJson()
    {
        var json = JsonSerializer.Serialize(Sample, JsonDefaults.WriteIndented);
        Assert.That(json, Does.Contain("\n"));
    }

    [Test]
    public void MqttSerializeOptions_ShouldProduceMultiLineJson()
    {
        var json = JsonSerializer.Serialize(Sample, JsonDefaults.MqttSerialize);
        Assert.That(json, Does.Contain("\n"));
    }
    
}