using System.Reflection;
using Application.Models;
using Application.Models.Dtos;
using NJsonSchema;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using WebSocketBoilerplate;

namespace Startup.Documentation;

public sealed class AddStringConstantsProcessor : IDocumentProcessor
{
    public void Process(DocumentProcessorContext context)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        var derivedTypeNames = assemblies
            .SelectMany(a =>
            {
                try
                {
                    return a.GetTypes();
                }
                catch
                {
                    return Array.Empty<Type>();
                }
            })
            .Where(t =>
                t != typeof(BaseDto) &&
                !t.IsAbstract &&
                (typeof(BaseDto).IsAssignableFrom(t) ||
                 typeof(ApplicationBaseDto).IsAssignableFrom(t))
            )
            .Select(t => t.Name)
            .ToList();

        var stringConstants = assemblies
            .SelectMany(a =>
            {
                try
                {
                    return a.GetTypes();
                }
                catch
                {
                    return Array.Empty<Type>();
                }
            })
            .Where(type => type.Name == nameof(StringConstants))
            .SelectMany(type =>
                type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                    .Where(field =>
                        field.IsLiteral &&
                        !field.IsInitOnly &&
                        field.FieldType == typeof(string)
                    )
                    .Select(field => field.GetValue(null)?.ToString())
            )
            .Where(constant => constant != null)
            .Distinct()
            .ToList();

        var allConstants = derivedTypeNames.Concat(stringConstants).Distinct().ToList();

        var schema = new JsonSchema
        {
            Type = JsonObjectType.String,
            Description = "Available eventType and string constants"
        };

        foreach (var constant in allConstants) schema.Enumeration.Add(constant);

        context.Document.Definitions["StringConstants"] = schema;
    }
}