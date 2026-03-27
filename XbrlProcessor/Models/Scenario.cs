using System.Collections.ObjectModel;

namespace XbrlProcessor.Models;

/// <summary>
/// Модель записи сценария контекста
/// </summary>
public class Scenario
{
    /// <summary>Тип измерерия XBRL</summary>
    public virtual string DimensionType { get; set; } = string.Empty;

    /// <summary>Наименование измерерия XBRL</summary>
    public virtual string DimensionName { get; set; } = string.Empty;

    /// <summary>Код измерерия XBRL</summary>
    public virtual string DimensionCode { get; set; } = string.Empty;

    /// <summary>Значение измерерия XBRL (dimension)</summary>
    public virtual string DimensionValue { get; set; } = string.Empty;
}

public class Scenarios : Collection<Scenario>
{
    public Scenarios() { }

    public Scenarios(List<Scenario> scenarios)
    {
        foreach (var scenario in scenarios)
        {
            Add(scenario);
        }
    }
}
