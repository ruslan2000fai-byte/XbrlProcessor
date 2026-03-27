using XbrlProcessor.Models;

namespace XbrlProcessor;

/// <summary>
/// Результат сравнения фактов
/// </summary>
public class FactComparisonResult
{
    /// <summary>
    /// Факты, присутствующие только в первом отчете
    /// </summary>
    public List<FactDifference> OnlyInReport1 { get; set; } = new();

    /// <summary>
    /// Факты, присутствующие только во втором отчете
    /// </summary>
    public List<FactDifference> OnlyInReport2 { get; set; } = new();

    /// <summary>
    /// Факты с разными значениями
    /// </summary>
    public List<FactDifference> DifferentValues { get; set; } = new();

    /// <summary>
    /// Идентичные факты
    /// </summary>
    public List<FactDifference> Matching { get; set; } = new();
}

/// <summary>
/// Информация о различии фактов
/// </summary>
public class FactDifference
{
    /// <summary>
    /// Имя факта (например, purcb-dic:INN)
    /// </summary>
    public string FactName { get; set; } = string.Empty;

    /// <summary>
    /// Ключ контекста
    /// </summary>
    public string ContextKey { get; set; } = string.Empty;

    /// <summary>
    /// Факт из первого отчета
    /// </summary>
    public Fact? Fact1 { get; set; }

    /// <summary>
    /// Факт из второго отчета
    /// </summary>
    public Fact? Fact2 { get; set; }

    /// <summary>
    /// Контекст из первого отчета
    /// </summary>
    public Context? Context1 { get; set; }

    /// <summary>
    /// Контекст из второго отчета
    /// </summary>
    public Context? Context2 { get; set; }
}

/// <summary>
/// Сервис для сравнения фактов между XBRL отчетами
/// </summary>
public class FactComparator
{
    private readonly ContextDuplicateFinder _contextFinder = new();

    /// <summary>
    /// Сравнение фактов двух отчетов
    /// </summary>
    public FactComparisonResult Compare(Instance report1, Instance report2)
    {
        var result = new FactComparisonResult();

        // Создаем словари: (factName, contextKey) -> fact
        var facts1Dict = new Dictionary<string, Fact>();
        var facts2Dict = new Dictionary<string, Fact>();

        // Создаем словари для доступа к контекстам
        var contexts1Dict = report1.Contexts.ToDictionary(c => c.Id);
        var contexts2Dict = report2.Contexts.ToDictionary(c => c.Id);

        // Заполняем словарь фактов первого отчета
        foreach (var fact in report1.Facts)
        {
            if (contexts1Dict.TryGetValue(fact.ContextRef, out var ctx))
            {
                var key = CreateFactKey(fact, ctx);
                facts1Dict[key] = fact;
            }
        }

        // Заполняем словарь фактов второго отчета
        foreach (var fact in report2.Facts)
        {
            if (contexts2Dict.TryGetValue(fact.ContextRef, out var ctx))
            {
                var key = CreateFactKey(fact, ctx);
                facts2Dict[key] = fact;
            }
        }

        // Находим факты, присутствующие только в report1 и различающиеся
        foreach (var kvp in facts1Dict)
        {
            var (factName, contextKey) = ParseFactKey(kvp.Key);
            
            if (!facts2Dict.TryGetValue(kvp.Key, out var fact2))
            {
                // Факт только в первом отчете
                result.OnlyInReport1.Add(new FactDifference
                {
                    FactName = factName,
                    ContextKey = contextKey,
                    Fact1 = kvp.Value,
                    Context1 = contexts1Dict[kvp.Value.ContextRef]
                });
            }
            else if (kvp.Value.Value != fact2.Value)
            {
                // Факты с разными значениями
                result.DifferentValues.Add(new FactDifference
                {
                    FactName = factName,
                    ContextKey = contextKey,
                    Fact1 = kvp.Value,
                    Fact2 = fact2,
                    Context1 = contexts1Dict[kvp.Value.ContextRef],
                    Context2 = contexts2Dict[fact2.ContextRef]
                });
            }
            else
            {
                // Идентичные факты
                result.Matching.Add(new FactDifference
                {
                    FactName = factName,
                    ContextKey = contextKey,
                    Fact1 = kvp.Value,
                    Fact2 = fact2
                });
            }
        }

        // Находим факты, присутствующие только во втором отчете
        foreach (var kvp in facts2Dict)
        {
            if (!facts1Dict.ContainsKey(kvp.Key))
            {
                var (factName, contextKey) = ParseFactKey(kvp.Key);
                result.OnlyInReport2.Add(new FactDifference
                {
                    FactName = factName,
                    ContextKey = contextKey,
                    Fact2 = kvp.Value,
                    Context2 = contexts2Dict[kvp.Value.ContextRef]
                });
            }
        }

        return result;
    }

    /// <summary>
    /// Создание уникального ключа для факта на основе имени и контекста
    /// </summary>
    private string CreateFactKey(Fact fact, Context context)
    {
        var contextKey = _contextFinder.GenerateContextKey(context);
        return $"{fact.Name}||{contextKey}";
    }

    private (string factName, string contextKey) ParseFactKey(string key)
    {
        var parts = key.Split(new[] { "||" }, 2, StringSplitOptions.None);
        return (parts[0], parts.Length > 1 ? parts[1] : string.Empty);
    }
}
