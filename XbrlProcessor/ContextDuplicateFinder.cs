using XbrlProcessor.Models;

namespace XbrlProcessor;

/// <summary>
/// Сервис для поиска дублирующихся контекстов
/// </summary>
public class ContextDuplicateFinder
{
    /// <summary>
    /// Поиск дублирующихся контекстов в отчете
    /// </summary>
    /// <returns>Словарь: ключ хэша -> список контекстов с этим хэшем</returns>
    public Dictionary<string, List<Context>> FindDuplicates(Instance instance)
    {
        var groups = new Dictionary<string, List<Context>>();

        foreach (var context in instance.Contexts)
        {
            var key = GenerateContextKey(context);
            
            if (!groups.ContainsKey(key))
                groups[key] = new List<Context>();
            
            groups[key].Add(context);
        }

        // Возвращаем только группы с дубликатами (более 1 контекста)
        return groups.Where(g => g.Value.Count > 1).ToDictionary(g => g.Key, g => g.Value);
    }

    /// <summary>
    /// Генерация уникального ключа для контекста на основе его содержания
    /// </summary>
    public string GenerateContextKey(Context context)
    {
        var parts = new List<string>();

        // Entity
        parts.Add($"E:{context.EntityScheme}|{context.EntityValue}");

        // Period
        string periodKey;
        if (context.PeriodInstant.HasValue)
        {
            periodKey = $"I:{context.PeriodInstant.Value:yyyy-MM-dd}";
        }
        else if (context.PeriodStartDate.HasValue && context.PeriodEndDate.HasValue)
        {
            periodKey = $"R:{context.PeriodStartDate.Value:yyyy-MM-dd}_{context.PeriodEndDate.Value:yyyy-MM-dd}";
        }
        else if (context.PeriodForever)
        {
            periodKey = "F:forever";
        }
        else
        {
            periodKey = "N:none";
        }
        parts.Add(periodKey);

        // Scenarios - сортируем для независимости от порядка
        if (context.Scenarios.Count > 0)
        {
            var scenarioKeys = context.Scenarios.Select(s =>
            {
                return $"{s.DimensionType}:{s.DimensionName}|{s.DimensionCode}|{s.DimensionValue}";
            }).OrderBy(k => k);
            
            parts.Add($"S:{string.Join(";", scenarioKeys)}");
        }
        else
        {
            parts.Add("S:none");
        }

        return string.Join("||", parts);
    }

    /// <summary>
    /// Проверка двух контекстов на идентичность (без учета id)
    /// </summary>
    public bool AreContextsEqual(Context c1, Context c2)
    {
        return GenerateContextKey(c1) == GenerateContextKey(c2);
    }
}
