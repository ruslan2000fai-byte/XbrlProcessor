using System.Collections.ObjectModel;

namespace XbrlProcessor.Models;

/// <summary>
/// Модель класса Unit
/// </summary>
public class Unit
{
    /// <summary>Идентификатор единицы измерения</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Парметр Measure (значение XBRL)</summary>
    public virtual string Measure { get; set; } = string.Empty;

    /// <summary>Парметр Numerator (значение XBRL)</summary>
    public virtual string Numerator { get; set; } = string.Empty;

    /// <summary>Парметр unitDenominator (значение XBRL)</summary>
    public virtual string Denominator { get; set; } = string.Empty;
}

public class Units : Collection<Unit>
{
}
