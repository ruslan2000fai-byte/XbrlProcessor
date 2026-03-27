using System.Collections.ObjectModel;

namespace XbrlProcessor.Models;

/// <summary>
/// Модель записи контекста
/// </summary>
public class Context
{
    public string Id { get; set; } = string.Empty;

    /// <summary>Значение контекста</summary>
    public virtual string EntityValue { get; set; } = string.Empty;

    /// <summary>Схема контекста</summary>
    public virtual string EntityScheme { get; set; } = string.Empty;

    /// <summary>Сегмент контекста (значение XBRL)</summary>
    public virtual string EntitySegment { get; set; } = string.Empty;

    /// <summary>Дата отчета (значение XBRL)</summary>
    public virtual System.DateTime? PeriodInstant { get; set; }

    /// <summary>Дата начала отчета (для определения периода) (значение XBRL)</summary>
    public virtual System.DateTime? PeriodStartDate { get; set; }

    /// <summary>Дата окончания отчета (для определения периода) (значение XBRL)</summary>
    public virtual System.DateTime? PeriodEndDate { get; set; }

    /// <summary>Метка отсутствия периода отчета (бессрочный) (значение XBRL)</summary>
    public virtual bool PeriodForever { get; set; }

    /// <summary>Коллекция сценариев контекста</summary>
    public virtual Scenarios Scenarios { get; set; } = new Scenarios();
}

public class Contexts : Collection<Context>
{
    public Contexts() { }

    public Contexts(List<Context> contexts)
    {
        foreach (var context in contexts)
        {
            Add(context);
        }
    }
}
