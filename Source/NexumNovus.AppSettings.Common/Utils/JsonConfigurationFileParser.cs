// source: https://source.dot.net/#Microsoft.Extensions.Configuration.Json/JsonConfigurationFileParser.cs
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
namespace NexumNovus.AppSettings.Common.Utils;

using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

/// <summary>
/// Used to convert json to flat key-value settings dictionary.
/// </summary>
internal sealed class JsonConfigurationFileParser
{
  private readonly Dictionary<string, string?> _data = new(StringComparer.OrdinalIgnoreCase);
  private readonly Stack<string> _paths = new();

  /// <summary>
  /// Convers json stream to key value dictionary.
  /// </summary>
  /// <param name="input">The stream to read.</param>
  /// <returns>Key value dictonary.</returns>
  public static IDictionary<string, string?> Parse(Stream input)
      => new JsonConfigurationFileParser().ParseStream(input);

  private IDictionary<string, string?> ParseStream(Stream input)
  {
    var jsonDocumentOptions = new JsonDocumentOptions
    {
      CommentHandling = JsonCommentHandling.Skip,
      AllowTrailingCommas = true,
    };

    using (var reader = new StreamReader(input))
    using (var doc = JsonDocument.Parse(reader.ReadToEnd(), jsonDocumentOptions))
    {
      if (doc.RootElement.ValueKind != JsonValueKind.Object)
      {
        throw new FormatException($"Error_InvalidTopLevelJSONElement: {doc.RootElement.ValueKind}");
      }

      VisitObjectElement(doc.RootElement);
    }

    return _data;
  }

  private void VisitObjectElement(JsonElement element)
  {
    var isEmpty = true;

    foreach (var property in element.EnumerateObject())
    {
      isEmpty = false;
      EnterContext(property.Name);
      VisitValue(property.Value);
      ExitContext();
    }

    SetNullIfElementIsEmpty(isEmpty);
  }

  private void VisitArrayElement(JsonElement element)
  {
    var index = 0;

    foreach (var arrayElement in element.EnumerateArray())
    {
      EnterContext(index.ToString());
      VisitValue(arrayElement);
      ExitContext();
      index++;
    }

    SetNullIfElementIsEmpty(isEmpty: index == 0);
  }

  private void SetNullIfElementIsEmpty(bool isEmpty)
  {
    if (isEmpty && _paths.Count > 0)
    {
      _data[_paths.Peek()] = null;
    }
  }

  private void VisitValue(JsonElement value)
  {
    Debug.Assert(_paths.Count > 0);

    switch (value.ValueKind)
    {
      case JsonValueKind.Object:
        VisitObjectElement(value);
        break;

      case JsonValueKind.Array:
        VisitArrayElement(value);
        break;

      case JsonValueKind.Number:
      case JsonValueKind.String:
      case JsonValueKind.True:
      case JsonValueKind.False:
      case JsonValueKind.Null:
        var key = _paths.Peek();
        if (_data.ContainsKey(key))
        {
          throw new FormatException($"Error_KeyIsDuplicated: {key}");
        }

        _data[key] = value.ToString();
        break;
      case JsonValueKind.Undefined:
      default:
        throw new FormatException($"Error_UnsupportedJSONToken: {value.ValueKind}");
    }
  }

  private void EnterContext(string context) =>
      _paths.Push(_paths.Count > 0 ?
          _paths.Peek() + ConfigurationPath.KeyDelimiter + context :
          context);

  private void ExitContext() => _paths.Pop();
}
