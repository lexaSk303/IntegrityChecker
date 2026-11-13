namespace IntegrityChecker.Models;

public sealed record FileHashRecord(string RelativePath, string Sha256);

// TODO 3
public sealed class Snapshot
{
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public Dictionary<string, FileHashRecord> Files { get; set; } = new();
}