// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

using System.Diagnostics;

namespace Microsoft.AspNetCore.Mvc.ModelBinding;

/// <summary>
/// This is a container for prefix values. It normalizes all the values into dotted-form and then stores
/// them in a sorted array. All queries for prefixes are also normalized to dotted-form, and searches
/// for ContainsPrefix are done with a binary search.
/// </summary>
public class PrefixContainer
{
    private readonly ICollection<string> _originalValues;
    private readonly string[] _sortedValues;

    public PrefixContainer(ICollection<string> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        _originalValues = values;

        if (_originalValues.Count == 0)
        {
            _sortedValues = Array.Empty<string>();
        }
        else
        {
            _sortedValues = new string[_originalValues.Count];
            _originalValues.CopyTo(_sortedValues, 0);
            Array.Sort(_sortedValues, StringComparer.OrdinalIgnoreCase);
        }
    }

    public bool ContainsPrefix(string prefix)
    {
        ArgumentNullException.ThrowIfNull(prefix);

        if (_sortedValues.Length == 0)
        {
            return false;
        }

        if (prefix.Length == 0)
        {
            return true;
        }

        return BinarySearch(prefix) > -1;
    }

    public IDictionary<string, string> GetKeysFromPrefix(string prefix)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var entry in _originalValues)
        {
            if (entry != null)
            {
                if (entry.Length == prefix.Length)
                {
                    continue;
                }

                if (prefix.Length == 0)
                {
                    GetKeyFromEmptyPrefix(entry, result);
                }
                else if (entry.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    GetKeyFromNonEmptyPrefix(prefix, entry, result);
                }
            }
        }

        return result;
    }

    private static void GetKeyFromEmptyPrefix(string entry, IDictionary<string, string> results)
    {
        string key;
        string fullName;
        var delimiterPosition = entry.AsSpan().IndexOfAny('[', '.');

        if (delimiterPosition == 0 && entry[0] == '[')
        {
            var bracketPosition = entry.IndexOf(']', 1);
            if (bracketPosition == -1)
            {
                return;
            }

            key = entry.Substring(1, bracketPosition - 1);
            fullName = entry.Substring(0, bracketPosition + 1);
        }
        else
        {
            key = delimiterPosition == -1 ? entry : entry.Substring(0, delimiterPosition);
            fullName = key;
        }

        if (!results.ContainsKey(key))
        {
            results.Add(key, fullName);
        }
    }

    private static void GetKeyFromNonEmptyPrefix(string prefix, string entry, IDictionary<string, string> results)
    {
        string key;
        string fullName;
        var keyPosition = prefix.Length + 1;

        switch (entry[prefix.Length])
        {
            case '.':
                var delimiterPosition = entry.AsSpan(keyPosition).IndexOfAny('[', '.');
                if (delimiterPosition < 0)
                {
                    key = entry.Substring(keyPosition);
                    fullName = entry;
                }
                else
                {
                    key = entry.Substring(keyPosition, delimiterPosition);
                    fullName = entry.Substring(0, delimiterPosition + keyPosition);
                }
                break;

            case '[':
                var bracketPosition = entry.IndexOf(']', keyPosition);
                if (bracketPosition == -1)
                {
                    return;
                }

                key = entry.Substring(keyPosition, bracketPosition - keyPosition);
                fullName = entry.Substring(0, bracketPosition + 1);
                break;

            default:
                return;
        }

        if (!results.ContainsKey(key))
        {
            results.Add(key, fullName);
        }
    }

    // This is tightly coupled to the definition at ModelStateDictionary.StartsWithPrefix
    private int BinarySearch(string prefix)
    {
        var start = 0;
        var end = _sortedValues.Length - 1;

        while (start <= end)
        {
            var pivot = start + ((end - start) / 2);
            var candidate = _sortedValues[pivot];
            var compare = string.Compare(
                prefix,
                0,
                candidate,
                0,
                prefix.Length,
                StringComparison.OrdinalIgnoreCase);
            if (compare == 0)
            {
                Debug.Assert(candidate.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));

                if (candidate.Length == prefix.Length)
                {
                    return pivot;
                }

                var c = candidate[prefix.Length];
                if (c == '.' || c == '[')
                {
                    return pivot;
                }

                return LinearSearch(prefix, start, end);
            }

            if (compare > 0)
            {
                start = pivot + 1;
            }
            else
            {
                end = pivot - 1;
            }
        }

        return ~start;
    }

    private int LinearSearch(string prefix, int start, int end)
    {
        for (; start <= end; start++)
        {
            var candidate = _sortedValues[start];
            var compare = string.Compare(
                prefix,
                0,
                candidate,
                0,
                prefix.Length,
                StringComparison.OrdinalIgnoreCase);
            if (compare == 0)
            {
                Debug.Assert(candidate.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));

                if (candidate.Length == prefix.Length)
                {
                    return start;
                }

                var c = candidate[prefix.Length];
                if (c == '.' || c == '[')
                {
                    return start;
                }
            }

            if (compare < 0)
            {
                break;
            }
        }

        return ~start;
    }
}
