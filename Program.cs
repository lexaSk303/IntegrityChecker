namespace Variant06;

internal static class Program
{
    private static async Task Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine("Зачётный проект 06: Анализатор целостности файлов");
        Console.WriteLine(new string('=', 64));

        var service = new ProjectService();
        await service.RunAsync();

        Console.WriteLine();
        Console.WriteLine("Нажмите Enter для завершения.");
        Console.ReadLine();
    }
}

public sealed class ProjectService
{
    public async Task RunAsync()
    {
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

        Console.WriteLine("Стартовый проект собран. Реализуйте задания варианта.");
        await Task.CompletedTask;
    }
}
