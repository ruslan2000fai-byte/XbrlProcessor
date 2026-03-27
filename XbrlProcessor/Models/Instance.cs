namespace XbrlProcessor.Models;

/// <summary>
/// Модель записи инстанса (файла отчета)
/// </summary>
public class Instance
{
    /// <summary>Коллекция контекстов (context)</summary>
    public virtual Contexts Contexts { get; set; } = new Contexts();

    /// <summary>Коллекция единиц измерения (unit)</summary>
    public virtual Units Units { get; set; } = new Units();

    /// <summary>Коллекция фактов (fact)</summary>
    public virtual Facts Facts { get; set; } = new Facts();
}
