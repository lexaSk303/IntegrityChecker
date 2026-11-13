namespace IntegrityChecker.Models;
using System.Collections.Generic;

// TODO 4, 9
public sealed class DiffResult
{
    public List<string> Added { get; } = new();
    public List<string> Modified { get; } = new();
    public List<string> Deleted { get; } = new();
    public bool IsEmpty => Added.Count == 0 && Modified.Count == 0 && Deleted.Count == 0;
}