using IntegrityChecker.Models;
using IntegrityChecker.Services;

namespace IntegrityChecker;

        // TODO: перенесите предметную логику в отдельные методы и классы.
        // TODO: не размещайте весь проект в Main.
        // TODO 1: Получить список файлов в каталоге.
        // TODO 2: Вычислить SHA256 каждого файла.
        // TODO 3: Сохранить контрольные суммы в JSON.
        // TODO 4: Сравнить текущие хеши с предыдущим снимком.
        // TODO 5: Реализовать параллельное вычисление хешей.
        // TODO 6: Сравнить LINQ и PLINQ по времени и корректности.
        // TODO 7: Корректно закрывать файловые потоки.

        // TODO 8: Добавить Channel и ограниченное число worker-ов.
        // TODO 9: Вывести новые, изменённые и удалённые файлы.
        // TODO 10: Сохранять путь последнего снимка в HKCU.

public sealed class ProjectService
{
    public async Task RunAsync()
    {
        var root = Path.Combine(AppContext.BaseDirectory, "TestData");
        Directory.CreateDirectory(root);
        SeedFiles(root);

        var svc = new IntegrityService();
        var jsonPath = Path.Combine(AppContext.BaseDirectory, "snapshot.json");
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        Console.WriteLine("[1,2,5,7,8] Вычисление хешей...");
        var current = await svc.ComputeHashesAsync(root, 4, cts.Token);
        Console.WriteLine($"Найдено файлов: {current.Files.Count}");

        Console.WriteLine("[3] Сохранение в JSON...");
        await svc.SaveAsync(current, jsonPath, cts.Token);

        Console.WriteLine("[10] Запись пути в реестр HKCU...");
        svc.SaveLastPath(jsonPath);

        Console.WriteLine("[4,9] Сравнение с предыдущим снимком...");
        var prev = await LoadPrev(jsonPath, cts.Token);
        svc.PrintDiff(prev, current);

        Console.WriteLine("[6] Бенчмарк LINQ vs PLINQ...");
        svc.BenchmarkLinqVsPlinq(Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories), root);

        Console.WriteLine("\nВсе 10 заданий выполнены.");
    }

    private static async Task<Snapshot?> LoadPrev(string path, CancellationToken token)
    {
        if (!File.Exists(path)) return null;
        try
        {
            var json = await File.ReadAllTextAsync(path, token);
            return System.Text.Json.JsonSerializer.Deserialize<Snapshot>(json);
        }
        catch (System.Text.Json.JsonException ex)
        {
            Console.WriteLine($" Снимок повреждён: {ex.Message}");
            return null;
        }
    }

    private static void SeedFiles(string root)
    {
        if (Directory.EnumerateFiles(root).Any()) return;
        for (int i = 1; i <= 10; i++)
            File.WriteAllText(Path.Combine(root, $"test{i}.txt"), $"Content {i}");
    }
}
