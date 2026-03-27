using System.Collections.ObjectModel;

namespace XbrlProcessor.Models;

/// <summary>
/// Модель факта (значения отчета)
/// </summary>
public class Fact
{
    /// <summary>Идентификатор фактов (значений отчета)</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Имя факта (например, purcb-dic:INN)</summary>
    public virtual string Name { get; set; } = string.Empty;

    /// <summary>Ссылка на контекст</summary>
    public virtual string ContextRef { get; set; } = string.Empty;

    /// <summary>Используемые контекст</summary>
    public virtual Context? Context { get; set; }

    /// <summary>Ссылка на единицу измерения</summary>
    public virtual string UnitRef { get; set; } = string.Empty;

    /// <summary>Используемые юнит</summary>
    public virtual Unit? Unit { get; set; }

    /// <summary>Точность измерения</summary>
    public virtual int? Decimals { get; set; }

    /// <summary>Точность значения</summary>
    public virtual int? Precision { get; set; }

    /// <summary>Значение</summary>
    public virtual string Value { get; set; } = string.Empty;
}

public class Facts : Collection<Fact>
{
}
