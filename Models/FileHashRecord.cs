namespace IntegrityChecker.Models;

// TODO 1, 2
public sealed record FileHashRecord(
    string RelativePath,
    string Sha256,
    long Size,
    DateTime LastWriteUtc);