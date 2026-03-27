# XBRL Processor

Консольное приложение для обработки XBRL (eXtensible Business Reporting Language) отчетов.

## Запуск приложения

```bash
# Сборка и запуск
dotnet build
dotnet run

# Или запуск Release версии
dotnet run --configuration Release
```

## Что делает программа

Приложение выполняет следующие задачи с XBRL отчетами:

1. **Поиск дублирующихся контекстов** - находит контексты с идентичным содержимым в report1.xbrl
2. **Объединение отчетов** - создает единый отчет из report1.xbrl и report2.xbrl с уникальными элементами
3. **Сравнение фактов** - выявляет различия между фактами в двух отчетах
4. **XPath-запросы** - выполняет специализированные запросы к XBRL документам

## Входные и выходные файлы

**Входные файлы** (должны находиться в `DataFiles/Input/`):
- `report1.xbrl` - первый XBRL отчет
- `report2.xbrl` - второй XBRL отчет

**Выходные файлы** (создаются в `DataFiles/Output/`):
- `report_merged.xbrl` - объединенный XBRL отчет

## Пример результата

```
=== XBRL Processor ===

Парсинг report1.xbrl...
  Контекстов: 125
  Единиц измерения: 3
  Фактов: 456

=== Задача 1: Поиск дублирующихся контекстов ===
Найдено 3 групп дублирующихся контекстов

=== Задача 2: Объединение отчетов ===
Объединенный отчет:
  Контекстов: 201
  Единиц измерения: 4
  Фактов: 843
Сохранен в: DataFiles/Output/report_merged.xbrl

=== Задача 3: Сравнение фактов ===
Результаты сравнения:
  Идентичных фактов: 234
  Только в report1: 78
  Только в report2: 45
  С разными значениями: 12

=== XPath-запросы ===
1. Контексты с периодом instant='2019-04-30': Найдено 45
2. Контексты с измерением dim-int:ID_sobstv_CZBTaxis: Найдено 23
3. Контексты без сценария: Найдено 5

=== Готово ===
```

---

## Техническое задание

### ОПИСАНИЕ

Содержание файла отчета (report1.xbrl и report2.xbrl):
- Контексты (xbrli:context) - описание разреза значений, указанных в отчете (на пример, в разрезе разных периодов). Набор содержащихся в контексте параметров должен быть уникальным. Сценариев в контексте может быть несколько (0..*). Запись объекта (entity) в контексте должна быть только одна (1...1). Период может быть описан следующими вариантами: значение instant, значение forever, или набор значений startDate и endDate.
- Единица измерения (xbrli:unit, юнит) - описание измерения значений, указанных в отчете (на пример, штуки или рубли). Набор содержащихся параметров должен быть уникальным. Измерене в записи может быть только одно (1..1).
- Значения (факты, в примере purcb-dic:*) - значения отчета. Код значения в разрезе контектса должен быть уникальным (например "purcb-dic:Kod_Okato3" в разрезе "A0"), идентификатор параметра (атрибут @id) должен быть уникальным.

### Задания:

1. Найти в файле report1.xbrl повторяющиеся контексты. Контекст считается уникальным, если у него уникальны набор значений объекта отчета entity, периода period (включая тип) и сценариев (включая имена). Идентификаторы контекстов (id) не учитываются при сравнении. Грубо говоря, должно быть полностью уникально содержащихся веток XML. Порядок самих веток в контексте не важен.

2. Объединить данные файлов report1.xbrl и report2.xbrl. На выходе получить новый объединенный отчет (xbrl) с объединными списками уникальных контекстов context, уникальных единиц измерений unit и значений (фактов).

3. Сравнить данных файлов report1.xbrl и report2.xbrl, выявить различающихся фактов (отсутствующие или имеющие разные значения). Факты являются идентифицируются по содержанию контекста (см. описание уникальности) и имени ветки значения (например purcb-dic:Kod_Okato3). Идентификаторы фактов (id) не учитываются при сравнении.

Написать запросы XPath для получения:
- контексты с периодом xbrli:period/xbrli:instant, равным "2019-04-30";
- контексты со сценарием, использующим измерение dimension="dim-int:ID_sobstv_CZBTaxis";
- контексты без сценария;

Проверка корректности записей контекстов, единица измерений и фактов в примере НЕ НУЖНА. Предполагается их использование просто, как набор данных.
В приложении предлагаемые модели классов используемых объектов. В описании немного лишей информации (из спецификации таксономии).

Полное и корректное выполнение задач желательно, но не обязательно. Важна архитектура решения и пример программного кода.

### Примеры идентичных контекстов и фактов:

```xml
<!-- Идентичные контексты -->
<xbrli:context id="A00">
    <xbrli:entity>
        <xbrli:identifier scheme="http://www.cbr.ru">1111111111111</xbrli:identifier>
    </xbrli:entity>
    <xbrli:period>
        <xbrli:instant>2019-01-01</xbrli:instant>
    </xbrli:period>
    <xbrli:scenario>
        <xbrldi:typedMember dimension="dim-int:ID_sobstv_CZBTaxis">
            <dim-int:ID_CZBTypedname>idRU000A0JV4Q1</dim-int:ID_CZBTypedname>
        </xbrldi:typedMember>
        <xbrldi:typedMember dimension="dim-int:IDEmitentaTaxis">
            <dim-int:ID_YULTypedName>id1037739085636</dim-int:ID_YULTypedName>
        </xbrldi:typedMember>
        <xbrldi:typedMember dimension="dim-int:ID_strokiTaxis">
            <dim-int:ID_strokiTypedname>НП</dim-int:ID_strokiTypedname>
        </xbrldi:typedMember>
        <xbrldi:explicitMember dimension="dim-int:Detaliz_kolva_czenBum_naPravSobstvAxis">mem-int:KolCPrinBuxUchVkachFinVlozhZaIsklVozvrOsnBezPrekrPriznMember</xbrldi:explicitMember>
    </xbrli:scenario>
</xbrli:context>  
<xbrli:context id="A58">
    <xbrli:period>
        <xbrli:instant>2019-01-01</xbrli:instant>
    </xbrli:period>
    <xbrli:entity>
        <xbrli:identifier scheme="http://www.cbr.ru">1111111111111</xbrli:identifier>
    </xbrli:entity>
    <xbrli:scenario>
        <xbrldi:explicitMember dimension="dim-int:Detaliz_kolva_czenBum_naPravSobstvAxis">mem-int:KolCPrinBuxUchVkachFinVlozhZaIsklVozvrOsnBezPrekrPriznMember</xbrldi:explicitMember>
        <xbrldi:typedMember dimension="dim-int:IDEmitentaTaxis">
            <dim-int:ID_YULTypedName>id1037739085636</dim-int:ID_YULTypedName>
        </xbrldi:typedMember>
        <xbrldi:typedMember dimension="dim-int:ID_sobstv_CZBTaxis">
            <dim-int:ID_CZBTypedname>idRU000A0JV4Q1</dim-int:ID_CZBTypedname>
        </xbrldi:typedMember>
        <xbrldi:typedMember dimension="dim-int:ID_strokiTaxis">
            <dim-int:ID_strokiTypedname>НП</dim-int:ID_strokiTypedname>
        </xbrldi:typedMember>
    </xbrli:scenario>
</xbrli:context>  

<!-- Идентичные факты -->  
<purcb-dic:INN contextRef="A00">1111111</purcb-dic:INN>
<purcb-dic:INN contextRef="A58">0000000</purcb-dic:INN>