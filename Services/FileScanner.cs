namespace IntegrityChecker.Services;

public sealed class FileScanner
{
    // TODO 1
    public IEnumerable<string> GetFiles(string rootDirectory)
    {
        if (!Directory.Exists(rootDirectory))
            throw new DirectoryNotFoundException($"Каталог не найден: {rootDirectory}");

        return Directory.EnumerateFiles(rootDirectory, "*", SearchOption.AllDirectories);
    }
}