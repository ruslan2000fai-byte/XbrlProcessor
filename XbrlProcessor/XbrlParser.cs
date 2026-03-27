using System.Xml.Linq;
using System.Xml.XPath;
using XbrlProcessor.Models;

namespace XbrlProcessor;

/// <summary>
/// Парсер XBRL файлов
/// </summary>
public class XbrlParser
{
    private readonly XNamespace _xbrli = "http://www.xbrl.org/2003/instance";
    private readonly XNamespace _xbrldi = "http://xbrl.org/2006/xbrldi";

    /// <summary>
    /// Парсинг XBRL файла
    /// </summary>
    public Instance Parse(string filePath)
    {
        var doc = XDocument.Load(filePath);
        var instance = new Instance();

        // Парсинг контекстов
        foreach (var contextElem in doc.Root!.Elements(_xbrli + "context"))
        {
            var context = ParseContext(contextElem);
            instance.Contexts.Add(context);
        }

        // Парсинг единиц измерения
        foreach (var unitElem in doc.Root!.Elements(_xbrli + "unit"))
        {
            var unit = ParseUnit(unitElem);
            instance.Units.Add(unit);
        }

        // Парсинг фактов (все элементы, не являющиеся context, unit, schemaRef)
        foreach (var factElem in doc.Root!.Elements().Where(e => 
            e.Name != _xbrli + "context" && 
            e.Name != _xbrli + "unit" &&
            e.Name.LocalName != "schemaRef"))
        {
            var fact = ParseFact(factElem);
            instance.Facts.Add(fact);
        }

        return instance;
    }

    /// <summary>
    /// Парсинг контекста
    /// </summary>
    public Context ParseContext(XElement contextElem)
    {
        var context = new Context
        {
            Id = contextElem.Attribute("id")?.Value ?? string.Empty
        };

        // Entity
        var entityElem = contextElem.Element(_xbrli + "entity");
        if (entityElem != null)
        {
            var identifierElem = entityElem.Element(_xbrli + "identifier");
            if (identifierElem != null)
            {
                context.EntityValue = identifierElem.Value;
                context.EntityScheme = identifierElem.Attribute("scheme")?.Value ?? string.Empty;
            }

            var segmentElem = entityElem.Element(_xbrli + "segment");
            if (segmentElem != null)
            {
                context.EntitySegment = segmentElem.ToString();
            }
        }

        // Period
        var periodElem = contextElem.Element(_xbrli + "period");
        if (periodElem != null)
        {
            var instantElem = periodElem.Element(_xbrli + "instant");
            if (instantElem != null)
            {
                if (DateTime.TryParse(instantElem.Value, out var instant))
                    context.PeriodInstant = instant;
            }

            var startElem = periodElem.Element(_xbrli + "startDate");
            var endElem = periodElem.Element(_xbrli + "endDate");
            if (startElem != null && endElem != null)
            {
                if (DateTime.TryParse(startElem.Value, out var start))
                    context.PeriodStartDate = start;
                if (DateTime.TryParse(endElem.Value, out var end))
                    context.PeriodEndDate = end;
            }

            var foreverElem = periodElem.Element(_xbrli + "forever");
            context.PeriodForever = foreverElem != null;
        }

        // Scenario
        var scenarioElem = contextElem.Element(_xbrli + "scenario");
        if (scenarioElem != null)
        {
            foreach (var memberElem in scenarioElem.Elements())
            {
                var scenario = new Scenario
                {
                    DimensionType = memberElem.Name.LocalName,
                    DimensionName = memberElem.Attribute(_xbrldi + "dimension")?.Value ?? string.Empty
                };

                if (memberElem.Name.LocalName == "typedMember")
                {
                    var child = memberElem.Elements().FirstOrDefault();
                    if (child != null)
                    {
                        scenario.DimensionCode = child.Name.LocalName;
                        scenario.DimensionValue = child.Value;
                    }
                }
                else if (memberElem.Name.LocalName == "explicitMember")
                {
                    scenario.DimensionValue = memberElem.Value;
                }

                context.Scenarios.Add(scenario);
            }
        }

        return context;
    }

    /// <summary>
    /// Парсинг единицы измерения
    /// </summary>
    public Unit ParseUnit(XElement unitElem)
    {
        var unit = new Unit
        {
            Id = unitElem.Attribute("id")?.Value ?? string.Empty
        };

        var measureElem = unitElem.Element(_xbrli + "measure");
        if (measureElem != null)
        {
            unit.Measure = measureElem.Value;
        }

        var divideElem = unitElem.Element(_xbrli + "divide");
        if (divideElem != null)
        {
            var numeratorElem = divideElem.Element(_xbrli + "unitNumerator");
            if (numeratorElem != null)
            {
                var measure = numeratorElem.Element(_xbrli + "measure");
                if (measure != null)
                    unit.Numerator = measure.Value;
            }

            var denominatorElem = divideElem.Element(_xbrli + "unitDenominator");
            if (denominatorElem != null)
            {
                var measure = denominatorElem.Element(_xbrli + "measure");
                if (measure != null)
                    unit.Denominator = measure.Value;
            }
        }

        return unit;
    }

    /// <summary>
    /// Парсинг факта
    /// </summary>
    private Fact ParseFact(XElement factElem)
    {
        var fact = new Fact
        {
            Id = factElem.Attribute("id")?.Value ?? string.Empty,
            ContextRef = factElem.Attribute("contextRef")?.Value ?? string.Empty,
            UnitRef = factElem.Attribute("unitRef")?.Value ?? string.Empty,
            Value = factElem.Value,
            Name = GetFactQualifiedName(factElem)
        };

        if (factElem.Attribute("decimals") != null && int.TryParse(factElem.Attribute("decimals")!.Value, out var decimals))
            fact.Decimals = decimals;

        if (factElem.Attribute("precision") != null && int.TryParse(factElem.Attribute("precision")!.Value, out var precision))
            fact.Precision = precision;

        return fact;
    }

    /// <summary>
    /// Получение квалифицированного имени факта
    /// </summary>
    private string GetFactQualifiedName(XElement factElem)
    {
        var ns = factElem.Name.Namespace;
        var localName = factElem.Name.LocalName;
        
        if (ns == XNamespace.None)
            return localName;
        
        // Получаем префикс пространства имен
        var prefix = factElem.GetPrefixOfNamespace(ns);
        if (string.IsNullOrEmpty(prefix))
            return localName;
        
        return $"{prefix}:{localName}";
    }

    /// <summary>
    /// Сохранение XBRL файла
    /// </summary>
    public void Save(Instance instance, string filePath)
    {
        var ns = new XNamespace[]
        {
            "http://www.xbrl.org/2003/instance",
            "http://xbrl.org/2006/xbrldi",
            "http://www.cbr.ru/xbrl/udr/dim/dim-int",
            "http://www.cbr.ru/xbrl/udr/dom/mem-int",
            "http://www.cbr.ru/xbrl/nso/purcb/dic/purcb-dic",
            "http://www.w3.org/1999/xlink",
            "http://www.xbrl.org/2003/iso4217"
        };

        var doc = new XDocument(
            new XDeclaration("1.0", "UTF-8", null),
            new XElement(_xbrli + "xbrl",
                new XAttribute(XNamespace.Xmlns + "xbrli", _xbrli.NamespaceName),
                new XAttribute(XNamespace.Xmlns + "xbrldi", _xbrldi.NamespaceName),
                new XAttribute(XNamespace.Xmlns + "dim-int", "http://www.cbr.ru/xbrl/udr/dim/dim-int"),
                new XAttribute(XNamespace.Xmlns + "mem-int", "http://www.cbr.ru/xbrl/udr/dom/mem-int"),
                new XAttribute(XNamespace.Xmlns + "purcb-dic", "http://www.cbr.ru/xbrl/nso/purcb/dic/purcb-dic"),
                new XAttribute(XNamespace.Xmlns + "iso4217", "http://www.xbrl.org/2003/iso4217"),
                new XAttribute(XNamespace.Xmlns + "link", "http://www.xbrl.org/2003/linkbase"),
                new XAttribute(XNamespace.Xmlns + "xlink", "http://www.w3.org/1999/xlink")
            )
        );

        var root = doc.Root!;

        // Добавляем контексты
        foreach (var context in instance.Contexts)
        {
            root.Add(CreateContextElement(context));
        }

        // Добавляем единицы измерения
        foreach (var unit in instance.Units)
        {
            root.Add(CreateUnitElement(unit));
        }

        // Добавляем факты
        foreach (var fact in instance.Facts)
        {
            root.Add(CreateFactElement(fact));
        }

        doc.Save(filePath);
    }

    private XElement CreateContextElement(Context context)
    {
        var entityElem = new XElement(_xbrli + "entity",
            new XElement(_xbrli + "identifier",
                new XAttribute("scheme", context.EntityScheme),
                context.EntityValue
            )
        );

        var periodElem = new XElement(_xbrli + "period");
        if (context.PeriodInstant.HasValue)
        {
            periodElem.Add(new XElement(_xbrli + "instant", context.PeriodInstant.Value.ToString("yyyy-MM-dd")));
        }
        else if (context.PeriodStartDate.HasValue && context.PeriodEndDate.HasValue)
        {
            periodElem.Add(
                new XElement(_xbrli + "startDate", context.PeriodStartDate.Value.ToString("yyyy-MM-dd")),
                new XElement(_xbrli + "endDate", context.PeriodEndDate.Value.ToString("yyyy-MM-dd"))
            );
        }
        else if (context.PeriodForever)
        {
            periodElem.Add(new XElement(_xbrli + "forever"));
        }

        var contextElem = new XElement(_xbrli + "context",
            new XAttribute("id", context.Id),
            entityElem,
            periodElem
        );

        if (context.Scenarios.Count > 0)
        {
            var scenarioElem = new XElement(_xbrli + "scenario");
            foreach (var scenario in context.Scenarios)
            {
                XElement memberElem;
                if (scenario.DimensionType == "typedMember")
                {
                    memberElem = new XElement(_xbrldi + "typedMember",
                        new XAttribute("dimension", scenario.DimensionName),
                        new XElement(XName.Get(scenario.DimensionCode ?? "unknown"), scenario.DimensionValue)
                    );
                }
                else
                {
                    memberElem = new XElement(_xbrldi + "explicitMember",
                        new XAttribute("dimension", scenario.DimensionName),
                        scenario.DimensionValue
                    );
                }
                scenarioElem.Add(memberElem);
            }
            contextElem.Add(scenarioElem);
        }

        return contextElem;
    }

    private XElement CreateUnitElement(Unit unit)
    {
        XElement unitElem;
        if (!string.IsNullOrEmpty(unit.Numerator) || !string.IsNullOrEmpty(unit.Denominator))
        {
            var divideElem = new XElement(_xbrli + "divide");
            
            if (!string.IsNullOrEmpty(unit.Numerator))
            {
                divideElem.Add(new XElement(_xbrli + "unitNumerator",
                    new XElement(_xbrli + "measure", unit.Numerator)));
            }
            
            if (!string.IsNullOrEmpty(unit.Denominator))
            {
                divideElem.Add(new XElement(_xbrli + "unitDenominator",
                    new XElement(_xbrli + "measure", unit.Denominator)));
            }
            
            unitElem = new XElement(_xbrli + "unit",
                new XAttribute("id", unit.Id),
                divideElem
            );
        }
        else
        {
            unitElem = new XElement(_xbrli + "unit",
                new XAttribute("id", unit.Id),
                new XElement(_xbrli + "measure", unit.Measure)
            );
        }

        return unitElem;
    }

    private XElement CreateFactElement(Fact fact)
    {
        var factElem = new XElement(
            XNamespace.None + GetFactName(fact),
            new XAttribute("contextRef", fact.ContextRef),
            fact.Value
        );

        if (!string.IsNullOrEmpty(fact.UnitRef))
            factElem.Add(new XAttribute("unitRef", fact.UnitRef));

        if (fact.Decimals.HasValue)
            factElem.Add(new XAttribute("decimals", fact.Decimals.Value));

        if (fact.Precision.HasValue)
            factElem.Add(new XAttribute("precision", fact.Precision.Value));

        if (!string.IsNullOrEmpty(fact.Id))
            factElem.Add(new XAttribute("id", fact.Id));

        return factElem;
    }

    private string GetFactName(Fact fact)
    {
        // Извлекаем имя факта из ContextRef или используем дефолтное
        return "fact";
    }
}
