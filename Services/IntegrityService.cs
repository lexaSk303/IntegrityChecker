using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Channels;
using IntegrityChecker.Models;
using Microsoft.Win32;

namespace IntegrityChecker.Services;

public sealed class IntegrityService
{
    private const string RegistryKey = @"Software\IntegrityChecker";
    public async Task<Snapshot> ComputeHashesAsync(
        string rootDir, int workerCount, CancellationToken token)
    {
        var snapshot = new Snapshot();
        var bag = new System.Collections.Concurrent.ConcurrentDictionary<string, FileHashRecord>();

        //TODO 8
        using var semaphore = new SemaphoreSlim(workerCount, workerCount);
        var channel = Channel.CreateUnbounded<string>(new UnboundedChannelOptions { SingleWriter = true, SingleReader = false });


        //TODO 1
        var producer = Task.Run(async () =>
        {
            foreach (var file in Directory.EnumerateFiles(rootDir, "*", SearchOption.AllDirectories))
                await channel.Writer.WriteAsync(file, token);
            channel.Writer.TryComplete();
        }, token);


        //TODO 2, 5, 7
        var workers = Enumerable.Range(0, workerCount).Select(_ => Task.Run(async () =>
        {
            await foreach(var path in channel.Reader.ReadAllAsync(token))
            {
                await semaphore.WaitAsync(token);
                try
                {
                    await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read,
                        FileShare.Read, 81920, useAsync: true);

                    using var sha = SHA256.Create();
                    var hash = await sha.ComputeHashAsync(stream, token);

                    var relative = Path.GetRelativePath(rootDir, path);
                    bag[relative] = new FileHashRecord(relative, Convert.ToHexString(hash).ToLowerInvariant());
                }
                finally { semaphore.Release(); }
            }
        }, token)).ToArray();

        await producer;
        await Task.WhenAll(workers);

        foreach (var kvp in bag) snapshot.Files[kvp.Key] = kvp.Value;
        return snapshot;
    }

    //TODO 3
    public async Task SaveAsync(Snapshot snapshot, string path, CancellationToken token)
    {
        await File.WriteAllTextAsync(path, JsonSerializer.Serialize(snapshot, new JsonSerializerOptions
        { WriteIndented = true }), token);
    }

    //TODO 4, 9
    public void PrintDiff(Snapshot? previous, Snapshot current)
    {
        if (previous is null)
        { Console.WriteLine("Предыдущий снимок отсутствует. Все файлы новые."); return; }

        var added = current.Files.Keys.Except(previous.Files.Keys).ToList();
        var deleted = previous.Files.Keys.Except(current.Files.Keys).ToList();
        var modified = current.Files.Keys
            .Where(k => previous.Files.ContainsKey(k) &&
                   !string.Equals(previous.Files[k].Sha256, current.Files[k].Sha256, StringComparison.OrdinalIgnoreCase))
            .ToList();

        Console.WriteLine($"Новые: {added.Count}, Изменённые: {modified.Count}, Удалённые: {deleted.Count}");
        if (modified.Any()) Console.WriteLine("Изменённые файлы: " + string.Join(", ", modified));
    }

    //TODO 6
    public void BenchmarkLinqVsPlinq(IEnumerable<string> files, string root)
    {
        var list = files.ToList();

        var sw1 = System.Diagnostics.Stopwatch.StartNew();
        var linq = list.Select(f => HashSync(f, root)).ToDictionary(x => x.RelativePath, x => x.Sha256);
        sw1.Stop();

        var sw2 = System.Diagnostics.Stopwatch.StartNew();
        var plinq = list.AsParallel().Select(f => HashSync(f, root)).ToDictionary(x => x.RelativePath, x => x.Sha256);
        sw2.Stop();

        bool same = linq.Count == plinq.Count && linq.All(kvp => plinq.TryGetValue(kvp.Key, out var v) && v == kvp.Value);
        Console.WriteLine($"LINQ: {sw1.ElapsedMilliseconds} мс | PLINQ: {sw2.ElapsedMilliseconds} мс | Совпадение: {same}");
    }

    private static FileHashRecord HashSync(string path, string root)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 81920);
        var hash = SHA256.HashData(stream);
        return new FileHashRecord(Path.GetRelativePath(root, path), Convert.ToHexString(hash).ToLowerInvariant());
    }

    // TODO 10
    public void SaveLastPath(string jsonPath)
    {
        using var key = Registry.CurrentUser.CreateSubKey(RegistryKey, writable: true);
        key.SetValue("LastSnapshot", jsonPath, RegistryValueKind.String);
    }
}