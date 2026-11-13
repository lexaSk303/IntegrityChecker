namespace IntegrityChecker.Models;
using System.Collections.Generic;

// TODO 3
public sealed class Snapshot
{
    public string RootDirectory { get; set; } = "";
    public DateTime CreatedUtc { get; set; }
    public Dictionary<string, FileHashRecord> Files { get; set; } = new();
}