using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using XbrlProcessor.Models;

namespace XbrlProcessor;

/// <summary>
/// Сервис для выполнения XPath-запросов к XBRL файлам
/// </summary>
public class XbrlXPathQueries
{
    private readonly XNamespace _xbrli = "http://www.xbrl.org/2003/instance";
    private readonly XNamespace _xbrldi = "http://xbrl.org/2006/xbrldi";
    private readonly XNamespace _dimInt = "http://www.cbr.ru/xbrl/udr/dim/dim-int";

    private readonly XDocument _doc;
    private readonly XmlNamespaceManager _nsManager;
    private readonly XbrlParser _parser = new();

    public XbrlXPathQueries(string filePath)
    {
        _doc = XDocument.Load(filePath);
        
        // Настройка менеджера пространств имен для XPath
        _nsManager = new XmlNamespaceManager(new NameTable());
        _nsManager.AddNamespace("xbrli", _xbrli.NamespaceName);
        _nsManager.AddNamespace("xbrldi", _xbrldi.NamespaceName);
        _nsManager.AddNamespace("dim-int", _dimInt.NamespaceName);
    }

    /// <summary>
    /// XPath-запрос: контексты с периодом xbrli:period/xbrli:instant, равным "2019-04-30"
    /// </summary>
    public List<Context> GetContextsWithInstantDate(string date)
    {
        var xpath = $"//xbrli:context[xbrli:period/xbrli:instant='{date}']";
        var elements = _doc.Root?.XPathSelectElements(xpath, _nsManager) ?? Enumerable.Empty<XElement>();
        
        return elements.Select(elem => _parser.ParseContext(elem)).ToList();
    }

    /// <summary>
    /// XPath-запрос: контексты со сценарием, использующим измерение dimension="dim-int:ID_sobstv_CZBTaxis"
    /// </summary>
    public List<Context> GetContextsWithDimension(string dimensionName)
    {
        var xpath = $"//xbrli:context[xbrli:scenario/xbrldi:typedMember[@dimension='{dimensionName}']]";
        var elements = _doc.Root?.XPathSelectElements(xpath, _nsManager) ?? Enumerable.Empty<XElement>();
        
        return elements.Select(elem => _parser.ParseContext(elem)).ToList();
    }

    /// <summary>
    /// XPath-запрос: контексты без сценария
    /// </summary>
    public List<Context> GetContextsWithoutScenario()
    {
        var xpath = "//xbrli:context[not(xbrli:scenario)]";
        var elements = _doc.Root?.XPathSelectElements(xpath, _nsManager) ?? Enumerable.Empty<XElement>();
        
        return elements.Select(elem => _parser.ParseContext(elem)).ToList();
    }

    /// <summary>
    /// Получить все контексты из документа
    /// </summary>
    public List<Context> GetAllContexts()
    {
        var xpath = "//xbrli:context";
        var elements = _doc.Root?.XPathSelectElements(xpath, _nsManager) ?? Enumerable.Empty<XElement>();
        
        return elements.Select(elem => _parser.ParseContext(elem)).ToList();
    }

    /// <summary>
    /// Получить все единицы измерения
    /// </summary>
    public List<Unit> GetAllUnits()
    {
        var xpath = "//xbrli:unit";
        var elements = _doc.Root?.XPathSelectElements(xpath, _nsManager) ?? Enumerable.Empty<XElement>();
        
        return elements.Select(elem => _parser.ParseUnit(elem)).ToList();
    }
}
