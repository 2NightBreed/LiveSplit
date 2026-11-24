using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;

namespace LiveSplit.View;

internal static class CustomXMLFileEditor
{
    private sealed class CustomPropertyDefinition
    {
        public string Name { get; set; }
        public string[] Settings { get; set; }
    }

    private sealed class CustomLayoutSettingsPropertiesDefinition
    {
        [JsonPropertyName("LiveSplit.Core")]
        public CustomPropertyDefinition[] Core { get; set; }

        [JsonPropertyName("LiveSplit Components")]
        public CustomPropertyDefinition[] Components { get; set; }
    }

    private sealed class CustomSplitsSettingsPropertiesDefinition
    {
        [JsonPropertyName("LiveSplit.Core")]
        public CustomPropertyDefinition[] Core { get; set; }
    }

    private static readonly CustomLayoutSettingsPropertiesDefinition _layoutSettingsProperties = LoadResource<CustomLayoutSettingsPropertiesDefinition>(
        "CustomLayoutSettings.json");
    private static readonly CustomSplitsSettingsPropertiesDefinition _splitsSettingsProperties = LoadResource<CustomSplitsSettingsPropertiesDefinition>(
        "CustomSplitsSettings.json");
    private static readonly Dictionary<string, HashSet<string>> _componentProperties = _layoutSettingsProperties.Components.ToDictionary(
        definition => definition.Name,
        definition => new HashSet<string>(definition.Settings, StringComparer.Ordinal),
        StringComparer.OrdinalIgnoreCase);
    private static readonly HashSet<string> _layoutProperties = GetProperties(_layoutSettingsProperties.Core, "Layout");
    private static readonly HashSet<string> _modelProperties = GetProperties(_splitsSettingsProperties.Core, "Model");

    internal static void Edit(Stream stream)
    {
        var document = new XmlDocument { PreserveWhitespace = true };
        stream.Position = 0;
        document.Load(stream);

        if (document.DocumentElement?.LocalName == "Layout")
        {
            RemoveProperties(GetChildElement(document.DocumentElement, "Settings"), _layoutProperties);
            RemoveComponentProperties(document.DocumentElement);
        }
        else if (document.DocumentElement?.LocalName == "Run")
        {
            RemoveModelProperties(document.DocumentElement);
        }

        stream.SetLength(0);
        stream.Position = 0;
        document.Save(stream);
        stream.Position = 0;
    }

    private static void RemoveComponentProperties(XmlElement layout)
    {
        XmlElement components = GetChildElement(layout, "Components");
        if (components == null)
        {
            return;
        }

        foreach (XmlElement component in GetChildElements(components, "Component"))
        {
            string path = GetChildElement(component, "Path")?.InnerText;
            string componentName = Path.GetFileNameWithoutExtension(path);
            if (componentName != null && _componentProperties.TryGetValue(componentName, out HashSet<string> properties))
            {
                RemoveProperties(GetChildElement(component, "Settings"), properties);
            }
        }
    }

    private static void RemoveModelProperties(XmlElement run)
    {
        if (_modelProperties.Count == 0)
        {
            return;
        }

        XmlElement segments = GetChildElement(run, "Segments");
        if (segments == null)
        {
            return;
        }

        foreach (XmlElement segment in GetChildElements(segments, "Segment"))
        {
            RemoveProperties(segment, _modelProperties);
        }
    }

    private static void RemoveProperties(XmlElement parent, HashSet<string> properties)
    {
        if (parent == null || properties.Count == 0)
        {
            return;
        }

        foreach (XmlNode child in parent.ChildNodes.Cast<XmlNode>().ToArray())
        {
            if (child is XmlElement element && properties.Contains(element.LocalName))
            {
                RemoveChildAndWhitespace(parent, child);
            }
            else if (child is XmlElement childElement)
            {
                RemoveProperties(childElement, properties);
            }
        }
    }

    private static void RemoveChildAndWhitespace(XmlElement parent, XmlNode child)
    {
        XmlNode previous = child.PreviousSibling;
        while (IsWhitespace(previous))
        {
            XmlNode whitespace = previous;
            previous = previous.PreviousSibling;
            parent.RemoveChild(whitespace);
        }

        parent.RemoveChild(child);
    }

    private static bool IsWhitespace(XmlNode node)
    {
        return node is XmlWhitespace || (node is XmlText && string.IsNullOrWhiteSpace(node.Value));
    }

    private static HashSet<string> GetProperties(CustomPropertyDefinition[] definitions, string name)
    {
        CustomPropertyDefinition definition = definitions.First(item => item.Name == name);
        return new HashSet<string>(definition.Settings, StringComparer.Ordinal);
    }

    private static T LoadResource<T>(string resourceName)
    {
        using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Missing embedded resource: {resourceName}");
        using var reader = new StreamReader(stream);

        return JsonSerializer.Deserialize<T>(reader.ReadToEnd())
            ?? throw new InvalidOperationException($"Invalid embedded resource: {resourceName}");
    }

    private static XmlElement GetChildElement(XmlElement parent, string name)
    {
        return parent == null ? null : GetChildElements(parent, name).FirstOrDefault();
    }

    private static IEnumerable<XmlElement> GetChildElements(XmlElement parent, string name)
    {
        foreach (XmlNode child in parent.ChildNodes)
        {
            if (child is XmlElement element && element.LocalName == name)
            {
                yield return element;
            }
        }
    }
}
