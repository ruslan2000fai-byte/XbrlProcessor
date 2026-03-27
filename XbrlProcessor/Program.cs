using XbrlProcessor;
using XbrlProcessor.Models;

namespace XbrlProcessor;

class Program
{
    static void Main(string[] args)
    {
        // Пути к файлам
        var basePath = AppDomain.CurrentDomain.BaseDirectory;
        var inputPath = Path.Combine(basePath, "DataFiles", "Input");
        var outputPath = Path.Combine(basePath, "DataFiles", "Output");

        var report1Path = Path.Combine(inputPath, "report1.xbrl");
        var report2Path = Path.Combine(inputPath, "report2.xbrl");
        var mergedPath = Path.Combine(outputPath, "report_merged.xbrl");

        // Создание папки Output если не существует
        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }

        Console.WriteLine("=== XBRL Processor ===\n");

        // Инициализация
        var parser = new XbrlParser();
        var contextFinder = new ContextDuplicateFinder();
        var merger = new XbrlMerger();
        var comparator = new FactComparator();

        // Парсинг отчетов
        Console.WriteLine("Парсинг report1.xbrl...");
        var report1 = parser.Parse(report1Path);
        Console.WriteLine($"  Контекстов: {report1.Contexts.Count}");
        Console.WriteLine($"  Единиц измерения: {report1.Units.Count}");
        Console.WriteLine($"  Фактов: {report1.Facts.Count}");

        Console.WriteLine("\nПарсинг report2.xbrl...");
        var report2 = parser.Parse(report2Path);
        Console.WriteLine($"  Контекстов: {report2.Contexts.Count}");
        Console.WriteLine($"  Единиц измерения: {report2.Units.Count}");
        Console.WriteLine($"  Фактов: {report2.Facts.Count}");

        // Задача 1: Поиск дублирующихся контекстов
        Console.WriteLine("\n=== Задача 1: Поиск дублирующихся контекстов ===");
        var duplicates = contextFinder.FindDuplicates(report1);
        
        if (duplicates.Count > 0)
        {
            Console.WriteLine($"Найдено {duplicates.Count} групп дублирующихся контекстов:");
            foreach (var group in duplicates)
            {
                Console.WriteLine($"\n  Группа (ключ: {group.Key.Substring(0, Math.Min(50, group.Key.Length))}...):");
                foreach (var ctx in group.Value)
                {
                    Console.WriteLine($"    - {ctx.Id}");
                }
            }
        }
        else
        {
            Console.WriteLine("Дублирующиеся контексты не найдены.");
        }

        // Задача 2: Объединение отчетов
        Console.WriteLine("\n=== Задача 2: Объединение отчетов ===");
        var merged = merger.Merge(report1, report2);
        Console.WriteLine($"Объединенный отчет:");
        Console.WriteLine($"  Контекстов: {merged.Contexts.Count}");
        Console.WriteLine($"  Единиц измерения: {merged.Units.Count}");
        Console.WriteLine($"  Фактов: {merged.Facts.Count}");

        // Сохранение объединенного отчета
        parser.Save(merged, mergedPath);
        Console.WriteLine($"Сохранен в: {mergedPath}");

        // Задача 3: Сравнение фактов
        Console.WriteLine("\n=== Задача 3: Сравнение фактов ===");
        var comparison = comparator.Compare(report1, report2);
        Console.WriteLine($"Результаты сравнения:");
        Console.WriteLine($"  Идентичных фактов: {comparison.Matching.Count}");
        Console.WriteLine($"  Только в report1: {comparison.OnlyInReport1.Count}");
        Console.WriteLine($"  Только в report2: {comparison.OnlyInReport2.Count}");
        Console.WriteLine($"  С разными значениями: {comparison.DifferentValues.Count}");

        if (comparison.OnlyInReport1.Any())
        {
            Console.WriteLine("\n  Факты только в report1 (первые 5):");
            foreach (var diff in comparison.OnlyInReport1.Take(5))
            {
                Console.WriteLine($"    - {diff.FactName} (контекст: {diff.ContextKey.Substring(0, Math.Min(30, diff.ContextKey.Length))}...)");
            }
        }

        if (comparison.OnlyInReport2.Any())
        {
            Console.WriteLine("\n  Факты только в report2 (первые 5):");
            foreach (var diff in comparison.OnlyInReport2.Take(5))
            {
                Console.WriteLine($"    - {diff.FactName} (контекст: {diff.ContextKey.Substring(0, Math.Min(30, diff.ContextKey.Length))}...)");
            }
        }

        if (comparison.DifferentValues.Any())
        {
            Console.WriteLine("\n  Факты с разными значениями:");
            foreach (var diff in comparison.DifferentValues.Take(5))
            {
                Console.WriteLine($"    - {diff.FactName}: report1='{diff.Fact1?.Value}' vs report2='{diff.Fact2?.Value}'");
            }
        }

        // XPath-запросы
        Console.WriteLine("\n=== XPath-запросы ===");
        
        var xpathQueries = new XbrlXPathQueries(report1Path);

        // Запрос 1: контексты с instant="2019-04-30"
        Console.WriteLine("\n1. Контексты с периодом instant='2019-04-30':");
        var instantContexts = xpathQueries.GetContextsWithInstantDate("2019-04-30");
        Console.WriteLine($"   Найдено: {instantContexts.Count}");
        foreach (var ctx in instantContexts.Take(5))
        {
            Console.WriteLine($"   - {ctx.Id}: {ctx.EntityValue}");
        }

        // Запрос 2: контексты с dimension="dim-int:ID_sobstv_CZBTaxis"
        Console.WriteLine("\n2. Контексты с измерением dim-int:ID_sobstv_CZBTaxis:");
        var dimensionContexts = xpathQueries.GetContextsWithDimension("dim-int:ID_sobstv_CZBTaxis");
        Console.WriteLine($"   Найдено: {dimensionContexts.Count}");
        foreach (var ctx in dimensionContexts.Take(5))
        {
            Console.WriteLine($"   - {ctx.Id}: {ctx.EntityValue}");
        }

        // Запрос 3: контексты без сценария
        Console.WriteLine("\n3. Контексты без сценария:");
        var noScenarioContexts = xpathQueries.GetContextsWithoutScenario();
        Console.WriteLine($"   Найдено: {noScenarioContexts.Count}");
        foreach (var ctx in noScenarioContexts)
        {
            Console.WriteLine($"   - {ctx.Id}: {ctx.EntityValue}");
        }

        Console.WriteLine("\n=== Готово ===");
    }
}
