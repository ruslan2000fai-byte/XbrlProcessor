using XbrlProcessor.Models;

namespace XbrlProcessor;

/// <summary>
/// Сервис для объединения двух XBRL отчетов
/// </summary>
public class XbrlMerger
{
    private readonly ContextDuplicateFinder _contextFinder = new();

    /// <summary>
    /// Объединение двух XBRL отчетов
    /// </summary>
    public Instance Merge(Instance report1, Instance report2)
    {
        var merged = new Instance();

        // Объединение контекстов (уникальные по содержанию)
        var contextMap = MergeContexts(report1.Contexts, report2.Contexts, merged.Contexts);

        // Объединение единиц измерения (уникальные по содержанию)
        var unitMap = MergeUnits(report1.Units, report2.Units, merged.Units);

        // Объединение фактов с обновлением ссылок на контексты и единицы
        MergeFacts(report1.Facts, report2.Facts, merged.Facts, contextMap, unitMap);

        return merged;
    }

    /// <summary>
    /// Объединение контекстов с сохранением маппинга старых ID в новые
    /// </summary>
    private Dictionary<string, string> MergeContexts(Contexts contexts1, Contexts contexts2, Contexts merged)
    {
        var contextMap = new Dictionary<string, string>(); // oldId -> newId
        var keyToContextId = new Dictionary<string, string>(); // contextKey -> mergedContextId
        int contextIndex = 0;

        // Обработка контекстов из первого отчета
        foreach (var context in contexts1)
        {
            var key = _contextFinder.GenerateContextKey(context);
            
            if (!keyToContextId.ContainsKey(key))
            {
                var newId = $"C{contextIndex++}";
                var newContext = CloneContext(context, newId);
                merged.Add(newContext);
                keyToContextId[key] = newId;
            }
            
            contextMap[context.Id] = keyToContextId[key];
        }

        // Обработка контекстов из второго отчета
        foreach (var context in contexts2)
        {
            var key = _contextFinder.GenerateContextKey(context);
            
            if (!keyToContextId.ContainsKey(key))
            {
                var newId = $"C{contextIndex++}";
                var newContext = CloneContext(context, newId);
                merged.Add(newContext);
                keyToContextId[key] = newId;
            }
            
            contextMap[context.Id] = keyToContextId[key];
        }

        return contextMap;
    }

    /// <summary>
    /// Объединение единиц измерения с сохранением маппинга старых ID в новые
    /// </summary>
    private Dictionary<string, string> MergeUnits(Units units1, Units units2, Units merged)
    {
        var unitMap = new Dictionary<string, string>(); // oldId -> newId
        var keyToUnitId = new Dictionary<string, string>(); // unitKey -> mergedUnitId
        int unitIndex = 0;

        // Генерация ключа для единицы измерения
        string GetUnitKey(Unit unit) => $"{unit.Measure}|{unit.Numerator}|{unit.Denominator}";

        // Обработка единиц из первого отчета
        foreach (var unit in units1)
        {
            var key = GetUnitKey(unit);
            
            if (!keyToUnitId.ContainsKey(key))
            {
                var newId = $"U{unitIndex++}";
                var newUnit = CloneUnit(unit, newId);
                merged.Add(newUnit);
                keyToUnitId[key] = newId;
            }
            
            unitMap[unit.Id] = keyToUnitId[key];
        }

        // Обработка единиц из второго отчета
        foreach (var unit in units2)
        {
            var key = GetUnitKey(unit);
            
            if (!keyToUnitId.ContainsKey(key))
            {
                var newId = $"U{unitIndex++}";
                var newUnit = CloneUnit(unit, newId);
                merged.Add(newUnit);
                keyToUnitId[key] = newId;
            }
            
            unitMap[unit.Id] = keyToUnitId[key];
        }

        return unitMap;
    }

    /// <summary>
    /// Объединение фактов с обновлением ссылок
    /// </summary>
    private void MergeFacts(Facts facts1, Facts facts2, Facts merged, 
        Dictionary<string, string> contextMap, Dictionary<string, string> unitMap)
    {
        int factIndex = 0;

        // Обработка фактов из первого отчета
        foreach (var fact in facts1)
        {
            var newFact = CloneFact(fact, $"F{factIndex++}");
            newFact.ContextRef = contextMap.TryGetValue(fact.ContextRef, out var ctxId) ? ctxId : fact.ContextRef;
            newFact.UnitRef = unitMap.TryGetValue(fact.UnitRef, out var unitId) ? unitId : fact.UnitRef;
            merged.Add(newFact);
        }

        // Обработка фактов из второго отчета
        foreach (var fact in facts2)
        {
            var newFact = CloneFact(fact, $"F{factIndex++}");
            newFact.ContextRef = contextMap.TryGetValue(fact.ContextRef, out var ctxId) ? ctxId : fact.ContextRef;
            newFact.UnitRef = unitMap.TryGetValue(fact.UnitRef, out var unitId) ? unitId : fact.UnitRef;
            merged.Add(newFact);
        }
    }

    private Context CloneContext(Context source, string newId)
    {
        return new Context
        {
            Id = newId,
            EntityValue = source.EntityValue,
            EntityScheme = source.EntityScheme,
            EntitySegment = source.EntitySegment,
            PeriodInstant = source.PeriodInstant,
            PeriodStartDate = source.PeriodStartDate,
            PeriodEndDate = source.PeriodEndDate,
            PeriodForever = source.PeriodForever,
            Scenarios = new Scenarios(source.Scenarios.Select(s => CloneScenario(s)).ToList())
        };
    }

    private Scenario CloneScenario(Scenario source)
    {
        return new Scenario
        {
            DimensionType = source.DimensionType,
            DimensionName = source.DimensionName,
            DimensionCode = source.DimensionCode,
            DimensionValue = source.DimensionValue
        };
    }

    private Unit CloneUnit(Unit source, string newId)
    {
        return new Unit
        {
            Id = newId,
            Measure = source.Measure,
            Numerator = source.Numerator,
            Denominator = source.Denominator
        };
    }

    private Fact CloneFact(Fact source, string newId)
    {
        return new Fact
        {
            Id = newId,
            ContextRef = source.ContextRef,
            UnitRef = source.UnitRef,
            Value = source.Value,
            Decimals = source.Decimals,
            Precision = source.Precision
        };
    }
}
