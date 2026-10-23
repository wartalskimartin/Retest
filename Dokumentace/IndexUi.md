# UI stránky `RetestWebAssembly/Pages/Index.razor`

## Účel stránky

Stránka slouží k výběru Bilboxu, načtení jeho NOK záznamů a označení kusů určených k retestu. Je implementovaná jako Blazor WebAssembly stránka s komponentami DevExpress Blazor 23.2.3.

## Celkové rozložení

Obrazovka se skládá z těchto částí:

1. Kaskádové filtry `Hala`, `Řada` a `Bilbox`.
2. Informační karta vybraného Bilboxu.
3. Chybový popup.
4. Toolbar gridu.
5. Grid s NOK záznamy.
6. Loading overlay při načítání nebo komunikaci s Bilboxem.

```text
+---------------------------------------------------------------+
| Hala              Řada                 Bilbox                |
| [Vyberte halu...] [Vyberte řadu...]     [Vyberte bilbox...]  |
+---------------------------------------------------------------+
| Úsek | Hala | Řada | Retesty                                  |
| NOK počítadlo kvality: ...                                   |
| Počet NOK vybraných k retestu: ...                           |
+---------------------------------------------------------------+
| Retest [počet] | Načíst znovu | Vybrat sloupce | [ ] ...     |
+---------------------------------------------------------------+
| [ ] | Svazek | Operace | Zakazka | ... | Komponent | Vada   |
+---------------------------------------------------------------+
```

## Horní filtry

Filtry jsou umístěné v jednom Bootstrap řádku se třemi sloupci. Každý filtr obsahuje nadpis `<h5>` a `DxComboBox` přes celou dostupnou šířku.

### Hala

- Zdroj dat: `halls`.
- Placeholder: `Vyberte halu...`.
- Vyhledávání používá režim `Contains`.
- Po změně se vymaže řada i Bilbox a načtou se dostupné řady.
- Seznam obsahuje speciální položku `VŠE`.

### Řada

- Zdroj dat: `rows`.
- Placeholder: `Vyberte řadu...`.
- Po změně se vymaže Bilbox a načtou se dostupné Bilboxy.
- Pokud je vybráno `VŠE`, dostupné Bilboxy se určují podle kombinace haly a řady.

### Bilbox

- Zdroj dat: `avaibleBilboxes`.
- Placeholder: `Vyberte bilbox...`.
- Po změně se načte detail Bilboxu, počet retestů a seznam NOK záznamů.
- Současně se vymaže výběr a neuložené změny.

Kaskáda filtrů je:

```text
Hala → Řada → Bilbox → Detail Bilboxu + NOK seznam
```

## Informační karta Bilboxu

Karta se vykreslí pouze tehdy, pokud existuje `bilboxInformation`. Používá Bootstrap třídy `card`, `p-2`, `row`, `col` a `mb-1`.

Zobrazuje:

- `Úsek` z `bilboxInformation.Section`.
- `Hala` z `bilboxInformation.Hall`.
- `Řada` z `bilboxInformation.StandRow`.
- `Retesty` z `RetestCount`.
- `NOK počítadlo kvality` z `bilboxInformation.QualityCounter`.
- `Počet NOK vybraných k retestu` podle `Grid.SelectedDataItems.Count`.

Pokud není vybrán žádný záznam, počet vybraných kusů je `0`.

## Loading overlay

Celý obsah stránky je obalen komponentou `DxLoadingPanel`.

Panel:

- blokuje obsah pomocí `IsContentBlocked="true"`,
- zobrazuje překryvné pozadí,
- nelze zavřít kliknutím,
- zobrazuje text z `loadText`,
- je řízen hodnotou `loadVisible`.

Používané texty jsou například `Načítání` a `Připojování k Bilboxu`.

Při asynchronní operaci se panel nejprve zobrazí, provede se akce a v bloku `finally` se vždy skryje.

## Chybový popup

Chyby se zobrazují pomocí `DxPopup`:

- titulek: `Chyba`,
- viditelnost: `ErrorPopupVisible`,
- text: `ErrorPopupText`.

Příklady zpráv:

- `Načítání selhalo. Zkontrolujte připojení a zkuste to znovu.`
- `Nedostupný Bilbox.`
- `Vyplňte u všech vybraných kusů Komponent, Vadu a Vznik vady operace.`

# Hlavní grid

Hlavní seznam používá `DxGrid` s datovým zdrojem `nokList`.

| Vlastnost | Hodnota |
|---|---|
| Datový zdroj | `nokList` |
| Klíč řádku | `ProductionDataNokId` |
| Režim editace | `GridEditMode.EditRow` |
| Velikost stránky | `100` |
| Řazení | vypnuté |
| Přesouvání sloupců | vypnuté |
| Klávesová navigace | zapnutá |
| Velikost komponenty | `SizeMode.Large` |
| CSS třída | `mt-3` |
| Validace DevExpressu | vypnutá |

Editace probíhá inline přímo v řádku. Popup editovací formulář se nepoužívá.

## Toolbar gridu

Toolbar obsahuje:

### `Retest`

Primární tlačítko, které volá `Save_Click`. Za textem zobrazuje počet vybraných záznamů pomocí badge:

```text
Retest [3]
```

### `Načíst znovu`

Znovu načte aktuálně vybraný Bilbox. Tlačítko je aktivní pouze tehdy, pokud je vybraná hodnota `FilterService.SelectedBilbox`.

### `Vybrat sloupce`

Otevře DevExpress column chooser pomocí `Grid.ShowColumnChooser()`.

### `Začít Retest po dokončení kusu`

Checkbox je navázaný na `DelayedRetest`. Hodnota se předává při volání služby `SetRetestMode`.

## Výběr řádků

První sloupec je `DxGridSelectionColumn`.

V hlavičce je checkbox pro hromadný výběr. Při zaškrtnutí se vyberou všechny vhodné řádky kromě záznamů z předchozí směny.

Checkbox v jednotlivém řádku:

- je zakázaný pro `IsFromPreviousShift == true`,
- je zakázaný pro `IsInserted == true`,
- nezpůsobí otevření editace řádku díky `@onclick:stopPropagation`.

Výběr a editace jsou tedy oddělené operace.

## Zobrazovací sloupce

Grid obsahuje tyto běžné sloupce:

| Pole | Titulek | Nastavení |
|---|---|---|
| `ProductNumber` | Svazek | šířka `130` |
| `OperationNumber` | Operace | šířka `90` |
| `OrderNumber` | Zakazka | formát `######`, šířka `100` |
| `OperatorNumber` | Operator | formát `##`, šířka `100` |
| `NokDateTime` | Datum | `dd.MM.yyyy HH:mm:ss`, šířka `190` |
| `DeviceNumber` | Device | šířka `130` |
| `WorkingPlaceNumber` | Stojan | šířka `100` |
| `BilboxNumber` | BbxNum | šířka `120` |

Tyto sloupce mají `DataRowEditorVisible=false`, takže se při editaci neupravují.

## Editovatelné sloupce

### Komponent

Používá `DxComboBox` s daty `komponentVady`.

- Text i hodnota jsou `KomponentName`.
- Placeholder: `Vyberte komponent...`.
- Po výběru se ověří platnost aktuální vady.
- Pokud vada nepatří k nové komponentě, vymaže se.
- Hodnota se může přenést do následujících řádků.

### Vada

Používá `DxComboBox` s vadami vybrané komponenty.

- Data získává metoda `GetVadyForKomponent`.
- Placeholder: `Vyberte vadu...`.
- Je povolen pouze po výběru komponenty.
- Změna se může přenést do následujících řádků.

### Vznik vady operace

V režimu zobrazení:

- hodnota `-10` se zobrazí jako `Neznámá`,
- ostatní hodnoty se zobrazí jako číslo operace,
- prázdná hodnota zůstane prázdná.

V režimu editace se zobrazí combobox s položkami:

1. `Neznámá` s ID `-10`,
2. seznam předchozích operací pro danou zakázku a operaci.

Po změně se hodnota zapíše do aktuálního řádku a přenese se do následujících řádků. Přenos se zastaví při změně zakázky.

### Operátoři

Combobox zobrazuje operátory pro vybranou předchozí operaci.

- Placeholder: `Všichni operátoři`.
- Obsahuje tlačítko pro vymazání hodnoty.
- Pokud není vybraná předchozí operace, seznam je prázdný.
- Data se cachují podle dvojice `OrderNumber` a `PreviousOperation`.

### Podrobný popis vady

Používá `DxMemo`:

- pět řádků,
- placeholder `Popis vady...`,
- CSS třída `cw-480`,
- aktualizace při každém vstupu pomocí `oninput`.

Text se může přenést do následujících editovatelných řádků.

## Řádky z předchozí směny

Řádky s `IsFromPreviousShift == true` mají speciální stav:

- jsou zvýrazněné třídami `highlighted-item` a `highlighted-item-cell`,
- nelze je vybrat,
- nelze je editovat,
- editovací ovládací prvky se nevykreslují,
- při ukládání se jim nastaví `Komponent = "ps"` a `Vada = "prev_shift"`.

Toto chování je nutné při replikaci zachovat.

## Neuložené změny

Změny se evidují ve slovníku `UnsavedChanges`. Každá položka obsahuje typ změny a seznam změněných polí.

Změněné buňky dostanou CSS třídu `grid-modified-cell`. Stejná vizualizace se použije pro nové záznamy.

## Chování při kliknutí na řádek

Kliknutí mimo checkbox:

1. ignoruje řádky z předchozí směny,
2. načte lookup data pro daný řádek,
3. uloží předchozí editovaný řádek do lokálního modelu, pokud je potřeba,
4. otevře inline editaci kliknutého řádku,
5. nastaví dropdown pro vznik vady operace jako otevřený.

Kliknutí na checkbox pouze mění výběr a neotevírá editaci.

## Uložení retestu

Kliknutí na `Retest`:

1. uloží právě editovaný řádek do lokálního gridového modelu,
2. získá vybrané řádky,
3. ověří komponentu, vadu a vznik vady operace,
4. doplní výchozí operátory,
5. doplní speciální hodnoty řádkům z předchozí směny,
6. zobrazí loading overlay,
7. zavolá `BilboxService.SetRetestMode`,
8. po úspěchu znovu načte data,
9. při chybě zobrazí `DxPopup`.

Validace se týká pouze vybraných řádků, které nejsou z předchozí směny.

## Persistence rozložení gridu

Rozložení gridu se ukládá do browserového `localStorage` pod klíčem:

```text
Grid-LayoutPersistence-Data
```

Ukládá se například viditelnost, pořadí a šířka sloupců. Při načtení stránky se rozložení obnoví. Pokud čtení selže, použije se výchozí stav.

## Stav potřebný pro replikaci

Minimální stav komponenty:

```text
halls
rows
avaibleBilboxes
komponentVady
bilboxInformation
RetestCount
nokList
SelectedNok
DelayedRetest
loadVisible
loadText
ErrorPopupVisible
ErrorPopupText
UnsavedChanges
```

Dále jsou potřeba cache pro předchozí operace, operátory a připravené položky comboboxu.

## Klíčové požadavky na věrnou repliku

1. Použít tři kaskádové comboboxy.
2. Zachovat kartu s informacemi o Bilboxu.
3. Použít `DxGrid` s `GridEditMode.EditRow`.
4. Oddělit výběr řádku od otevření editace.
5. Zachovat závislost `Komponent → Vada`.
6. Zvýraznit řádky z předchozí směny.
7. Zakázat editaci a výběr řádků z předchozí směny.
8. Zvýrazňovat neuložené změny.
9. Přidat loading overlay a chybový popup.
10. Uložit nastavení gridu do `localStorage`.

# API používané stránkou

## Klientská API služba

Stránka komunikuje se serverem přes `RetestWebAssembly.Services.BilboxService`. Služba používá `HttpClient` a jako základní adresu API má:

```text
https://czvrsrvweb01.intranet.zkw.at:8965/
```

Konkrétní adresa je nastavená přímo v klientské službě `BilboxService`. Při replikaci je vhodnější přesunout ji do konfigurace aplikace, aby nebyla pevně zapsaná ve zdrojovém kódu.

## Obecné chování HTTP klienta

Pro GET požadavky služba používá:

- timeout 15 sekund,
- maximálně dva pokusy,
- prodlevu 400 ms před druhým pokusem,
- návrat prázdného seznamu nebo výchozí hodnoty při chybě.

Pro POST požadavky se používá timeout 15 sekund bez opakování požadavku. Při chybě se vrací výchozí hodnota, obvykle `false`.

To znamená, že některé síťové chyby se na úrovni komponenty projeví jako prázdný seznam nebo nulová hodnota. Uživatelské chybové hlášení na stránce se zobrazuje zejména při neúspěšném načtení Bilboxu nebo při neúspěšném uložení retestu.

## Přehled endpointů

API používá dva controllery:

- `BilboxController` s routou `api/Bilbox`.
- `SseController` s routou `api/Sse`.

Přestože se `SseController` používá také pro Server-Sent Events, výběrové a stavové operace stránky volají jeho běžné JSON endpointy.

| HTTP | Endpoint | Klientská metoda | Odpověď |
|---|---|---|---|
| GET | `/api/Bilbox/getHalls` | `GetHalls()` | `List<string>` |
| GET | `/api/Bilbox/getAllRows` | `GetRows()` | `List<string>` |
| GET | `/api/Bilbox/getRowsByHall?hallNumber={hall}` | `GetRows(string hall)` | `List<string>` |
| GET | `/api/Sse/getAllBilboxes` | `GetAvaibleBilboxes()` | `List<string>` |
| GET | `/api/Sse/getBilboxesByHall?hallNumber={hall}` | `GetAvaibleBilboxes(string hall)` | `List<string>` |
| GET | `/api/Sse/getBilboxesByRow?row={row}` | `GetAvaibleBilboxesByRow(string row)` | `List<string>` |
| GET | `/api/Sse/getAvaibleBilboxes?hallNumber={hall}&row={row}` | `GetAvaibleBilboxes(string hall, string row)` | `List<string>` |
| GET | `/api/Bilbox/getBilboxInformation/{bilboxName}` | `GetBilboxDetail(string bilboxName)` | `StandsList` |
| GET | `/api/Bilbox/getStandsHistory?stand={stand}` | `GetStandRetest(string stand)` | `int` |
| GET | `/api/Bilbox/getActualShiftNOK?standLoc={bilbox}` | `GetNokList(string bilbox)` | `List<QualityCardBilbox>` |
| GET | `/api/Bilbox/getKomponentVady` | `GetKomponentVady()` | `List<RetestKomponentVadyDTO>` |
| GET | `/api/Bilbox/getOperationPredecessors?orderNumber={orderNumber}&operationNumber={operationNumber}` | `GetOperationPredecessors(...)` | `List<int?>` |
| GET | `/api/Bilbox/getOperatorNumbersByOrderOperation?orderNumber={orderNumber}&operationNumber={operationNumber}` | `GetOperatorNumbersByOrderOperation(...)` | `List<string>` |
| POST | `/api/Sse/retestMode/{bilbox}/{delayedRetest}` | interně v `SetRetestMode(...)` | HTTP status |
| POST | `/api/Bilbox/addRetestNok` | interně v `SetRetestMode(...)` | HTTP status |

Parametry v cestě a query stringu je nutné při replikaci URL-enkódovat. Zvláštní pozornost je potřeba věnovat hodnotám `bilbox`, `hall` a `row`, protože jsou vkládány přímo do URL.

## Načtení výběrových dat

### Načtení hal

```http
GET /api/Bilbox/getHalls
```

Server vrací seznam unikátních čísel hal jako `List<string>`. Klient do seznamu následně doplňuje položku `VŠE`, která není součástí databázové odpovědi.

### Načtení řad

Pokud je zvoleno `VŠE`, použije se:

```http
GET /api/Bilbox/getAllRows
```

Při konkrétní hale se použije:

```http
GET /api/Bilbox/getRowsByHall?hallNumber={hall}
```

Server vrací unikátní řady. Klient opět přidává položku `VŠE`.

### Načtení Bilboxů

Podle kombinace vybraných filtrů se volá právě jeden z těchto endpointů:

```http
GET /api/Sse/getAllBilboxes
GET /api/Sse/getBilboxesByHall?hallNumber={hall}
GET /api/Sse/getBilboxesByRow?row={row}
GET /api/Sse/getAvaibleBilboxes?hallNumber={hall}&row={row}
```

Odpovědí je seznam identifikátorů Bilboxů ve formátu `List<string>`.

## Načtení detailu a dat Bilboxu

Po výběru Bilboxu stránka provádí několik samostatných volání.

### Detail Bilboxu

```http
GET /api/Bilbox/getBilboxInformation/{bilboxName}
```

Odpověď má typ `StandsList`:

| Pole | Význam |
|---|---|
| `Stand` | identifikátor Bilboxu |
| `Hall` | číslo haly |
| `StandRow` | řada |
| `Section` | úsek |
| `StandStatus` | stav stojanu/Bilboxu |
| `OwnerStand` | vlastník nebo přiřazení stojanu |
| `DateTimeUpdate` | čas poslední aktualizace |
| `Remarks` | poznámka |
| `StandLocation` | umístění |
| `QualityCounter` | počítadlo kvality doplněné serverem |

Controller před vrácením detailu doplňuje `QualityCounter` pomocí databázové procedury `getQualityCounterSP`.

Při nenalezení Bilboxu endpoint vrací `404 Not Found`. Klient v takovém případě vrací `null`.

### Počet retestů

```http
GET /api/Bilbox/getStandsHistory?stand={stand}
```

Odpovědí je celé číslo. Server počítá záznamy aktuální směny pro daný stojan, které mají vyplněné `ProductionDataNokId`. Hodnota se zobrazuje v informační kartě jako `Retesty`.

Poznámka: signatura serverové metody `GetStandsHistory` deklaruje `ActionResult<List<StandsHistoryDTO>>`, ale skutečná implementace vrací přes `Ok(...)` celočíselný výsledek. Klientská služba proto správně očekává JSON číslo (`int`). Při nové implementaci API je vhodné signaturu controlleru sjednotit s reálnou odpovědí.

### Aktuální NOK záznamy

```http
GET /api/Bilbox/getActualShiftNOK?standLoc={bilbox}
```

Odpovědí je `List<QualityCardBilbox>`. Server:

1. určí začátek předchozí směny,
2. načte NOK výrobní data pro daný Bilbox,
3. vyloučí záznamy, které už existují v `QualityCardBilboxes`,
4. označí záznamy z předchozí směny,
5. namapuje výrobní data na `QualityCardBilbox`.

Řádek může být označen jako `IsFromPreviousShift`. Tato hodnota následně řídí zvýraznění, výběr a editaci v gridu.

## Lookup data pro editaci gridu

### Komponenty a vady

```http
GET /api/Bilbox/getKomponentVady
```

Odpovědí je seznam `RetestKomponentVadyDTO`:

```text
RetestKomponentVadyDTO
 ├── KomponentId
 ├── KomponentName
 └── Vady: List<RetestVadaDTO>
	   ├── VadaId
	   ├── KomponentId
	   └── VadaName
```

Server načítá komponenty a vady z databáze, řadí je podle názvu a vrací je v jediném volání. UI proto nemusí volat samostatný endpoint pro vady konkrétní komponenty.

### Předchozí operace

```http
GET /api/Bilbox/getOperationPredecessors?orderNumber={orderNumber}&operationNumber={operationNumber}
```

Odpovědí je seznam předchozích operací `List<int?>`. Server získává data pomocí uložené procedury:

```text
dbo.GetOperationPredecessors
```

Parametry databázové procedury jsou:

```text
@Order
@Operation
```

Klient výsledek cachuje podle dvojice `OrderNumber` a `OperationNumber`. Do zobrazovaného seznamu navíc přidává položku `Neznámá` s ID `-10`.

### Operátoři předchozí operace

```http
GET /api/Bilbox/getOperatorNumbersByOrderOperation?orderNumber={orderNumber}&operationNumber={operationNumber}
```

Odpovědí je `List<string>` s čísly operátorů. Server používá uloženou proceduru:

```text
dbo.GetOperatorNumbersByOrderOperation
```

Parametry procedury jsou:

```text
@BWAUNR  = orderNumber
@BWAGNR  = operationNumber
```

Klient ukládá výsledky do cache podle dvojice `OrderNumber` a `PreviousOperation`. Po změně předchozí operace se lookup načítá asynchronně na pozadí.

## Uložení retestu

Uložení používá metodu `BilboxService.SetRetestMode`. Ta provádí dvě serverová volání v daném pořadí.

### 1. Přepnutí Bilboxu do režimu Retest

```http
POST /api/Sse/retestMode/{bilbox}/{delayedRetest}
Content-Type: application/json
```

Tělo požadavku je seznam vybraných `QualityCardBilbox` záznamů:

```json
[
  {
	"productionDataNokId": 12345,
	"productNumber": "...",
	"orderNumber": 100001,
	"operationNumber": 20,
	"operatorNumber": 12,
	"workingPlaceNumber": "...",
	"deviceNumber": "...",
	"bilboxNumber": "...",
	"nokDateTime": "2024-01-01T08:00:00"
  }
]
```

Controller převede `QualityCardBilbox` na `ProductionDataDTO` a nastaví stav připojeného Bilboxu na `Retest`. Volba `delayedRetest` se nepřenáší v JSON těle; server ji získává z parametru v URL a zapíše ji do výsledného `ProductionDataDTO`.

Endpoint vrací:

- `200 OK`, pokud byl stav Bilboxu změněn,
- `400 Bad Request`, pokud Bilbox není nalezen nebo se stav nepodařilo změnit.

Server hledá připojený Bilbox až ve třech pokusech s prodlevami. Pokud Bilbox není registrován v SSE spojení, přepnutí selže.

### 2. Uložení NOK karet

Pokud první volání uspěje, klient zavolá:

```http
POST /api/Bilbox/addRetestNok
Content-Type: application/json
```

Tělem je celý seznam `nokList`, nikoli pouze vybrané řádky. Před odesláním jsou řádkům z předchozí směny doplněny hodnoty:

```text
Komponent = "ps"
Vada      = "prev_shift"
```

Controller každému záznamu doplní `DatetimeServer` a uloží seznam do tabulky `QualityCardBilboxes`.

Výsledkem je:

- `200 OK` při úspěšném uložení,
- neúspěch je klientem vyhodnocen jako `false`.

Metoda `SetRetestMode` vrací `true` pouze tehdy, pokud uspěje změna režimu Bilboxu i uložení NOK seznamu.

## Datový kontrakt `QualityCardBilbox`

Grid používá model `SharedLib.Models.QualityCardBilbox`. Pro tuto stránku jsou nejdůležitější tato pole:

| Pole | Použití |
|---|---|
| `ProductionDataNokId` | klíč řádku gridu |
| `ProductNumber` | sloupec Svazek |
| `OperationNumber` | aktuální operace |
| `OrderNumber` | zakázka a cache lookupů |
| `OperatorNumber` | aktuální operátor |
| `WorkingPlaceNumber` | stojan/Bilbox location |
| `DeviceNumber` | zařízení |
| `BilboxNumber` | číslo Bilboxu |
| `NokDateTime` | datum NOK záznamu |
| `Komponent` | vybraná komponenta vady |
| `Vada` | vybraná vada |
| `PreviousOperation` | operace, ve které vznikla vada |
| `OperatorNumbers` | operátoři předchozí operace |
| `DefectDescription` | podrobný popis vady |
| `DateTimeRecord` | čas záznamu při ukládání |
| `IsFromPreviousShift` | pouze klientské označení řádku z předchozí směny |
| `IsInserted` | pouze klientské označení nového řádku |

## Datový tok stránky

```text
OnInitializedAsync
 ├── GET /api/Bilbox/getHalls
 └── GET /api/Bilbox/getKomponentVady

Výběr haly
 └── GET /api/Bilbox/getAllRows
	 nebo GET /api/Bilbox/getRowsByHall

Výběr řady
 └── jeden z endpointů /api/Sse/get*Bilboxes

Výběr Bilboxu
 ├── GET /api/Bilbox/getBilboxInformation/{bilbox}
 ├── GET /api/Bilbox/getStandsHistory?stand={bilbox}
 └── GET /api/Bilbox/getActualShiftNOK?standLoc={bilbox}

Editace řádku
 ├── GET /api/Bilbox/getOperationPredecessors
 └── GET /api/Bilbox/getOperatorNumbersByOrderOperation

Kliknutí na Retest
 ├── POST /api/Sse/retestMode/{bilbox}/{delayedRetest}
 └── POST /api/Bilbox/addRetestNok
```

## Poznámka k SSE spojení

Endpoint `POST /api/Sse/retestMode/...` mění stav pouze u Bilboxu, který je registrovaný v serverové kolekci připojených zařízení. Registrace probíhá přes dlouhodobé SSE spojení:

```http
GET /api/Sse/getBilboxState/{bilboxNum}
```

Samotná stránka `Index.razor` toto SSE spojení přímo nevytváří. Pro úspěšné přepnutí do režimu Retest musí být Bilbox registrovaný jinou částí systému nebo příslušným klientem zařízení.

# Zadání pro detail incidentu

## Cíl

V aplikaci existuje seznam incidentů. Po kliknutí na incident se má otevřít detail incidentu, který zobrazí jeho jednotlivé NOK položky. Vizuálně i funkčně má detail odpovídat retestové stránce `RetestWebAssembly/Pages/Index.razor`.

Detail incidentu má být navržen jako samostatná Blazor WebAssembly stránka nebo komponenta, která znovu použije stejné principy jako retestový grid:

- horní informační část incidentu,
- seznam NOK v `DxGrid`,
- výběr řádků checkboxy,
- inline editaci v režimu `GridEditMode.EditRow`,
- závislé editory `Komponent` → `Vada`,
- výběr `PreviousOperation` a `OperatorNumbers`,
- textový popis vady,
- zvýraznění speciálních nebo needitovatelných řádků,
- loading overlay a chybový popup,
- zvýraznění neuložených změn.

## Navigace ze seznamu incidentů

Po kliknutí na incident musí aplikace přejít na detail s identifikátorem incidentu v URL. Doporučený tvar je například:

```text
/incidents/{incidentId}
```

Detail nesmí spoléhat pouze na globální stav vybraného incidentu. Po přímém otevření URL, po obnovení stránky nebo po použití odkazu v jiné části aplikace musí být možné incident načíst z `incidentId`.

Doporučený tok navigace:

```text
Seznam incidentů
	│
	└── kliknutí na incident
			│
			└── /incidents/{incidentId}
					│
					├── načtení incidentu
					├── určení Bilboxu / stojanu
					├── načtení NOK položek
					└── zobrazení detailního gridu
```

## Hlavička detailu incidentu

Detail má v horní části zobrazit kontext incidentu, minimálně:

- identifikátor incidentu,
- stav incidentu,
- datum a čas incidentu,
- Bilbox nebo stojan,
- hala a řada, pokud jsou dostupné,
- úsek,
- počet NOK položek,
- počet vybraných NOK položek.

Konkrétní pole incidentu se musí převzít z existujícího incidentového API. V aktuálním řešení nebyl nalezen incidentový controller, DTO ani endpoint, proto není možné z tohoto repozitáře určit přesný název incidentového identifikátoru nebo strukturu incidentu.

## Vztah incidentu k existujícímu NOK API

Existující retestové API načítá NOK podle Bilboxu/stojanu:

```http
GET /api/Bilbox/getActualShiftNOK?standLoc={bilbox}
```

Tento endpoint nepřijímá `incidentId`. Vrací NOK záznamy podle lokace a časového rozsahu směny. Z toho vyplývají dvě možné integrační varianty:

### Varianta A: Incident obsahuje Bilbox a odpovídá aktuálnímu směnovému seznamu

Pokud incident obsahuje hodnotu použitelnou jako `standLoc` a jeho NOK položky jsou součástí aktuálního směnového seznamu, detail může:

1. načíst incident z incidentového API,
2. získat z incidentu Bilbox/stojan,
3. zavolat `GET /api/Bilbox/getActualShiftNOK?standLoc={bilbox}`,
4. zobrazit vrácené `QualityCardBilbox` v gridu.

Tato varianta je použitelná pouze tehdy, pokud je jednoznačně zaručeno, že vrácený seznam patří právě danému incidentu. Nestačí pouze shoda Bilboxu, pokud může existovat více incidentů pro stejný Bilbox ve stejné směně.

### Varianta B: Incident má vlastní přesný seznam NOK

Pokud incident obsahuje konkrétní NOK položky nebo historická data, musí incidentové API vracet vazbu podle incidentu, například:

```http
GET /api/incidents/{incidentId}/nok
```

Přesný endpoint je potřeba převzít z existující aplikace incidentů. V tomto repozitáři takový endpoint není přítomný a nesmí být při implementaci považován za existující bez ověření.

Ideální odpověď by měla obsahovat buď přímo `List<QualityCardBilbox>`, nebo mapovatelná pole:

```text
incidentId
productionDataNokId
productNumber
orderNumber
operationNumber
operatorNumber
workingPlaceNumber
deviceNumber
bilboxNumber
nokDateTime
komponent
vada
previousOperation
operatorNumbers
defectDescription
```

## Replikace retestového gridu

Grid v detailu incidentu má zachovat stejné nastavení jako retestový grid:

| Vlastnost | Požadavek |
|---|---|
| Komponenta | `DxGrid` |
| Datový zdroj | seznam NOK incidentu |
| Editace | `GridEditMode.EditRow` |
| Klíč řádku | `ProductionDataNokId`, případně stabilní NOK ID |
| Výběr | `DxGridSelectionColumn` |
| Velikost stránky | `100` |
| Řazení | vypnuté, pokud má být zachováno původní chování |
| Editace | inline v řádku |
| Sloupce | stejné jako v `Index.razor` |
| Toolbar | uložit/retestovat, obnovit, vybrat sloupce |

Zobrazovací sloupce:

- `ProductNumber` — Svazek,
- `OperationNumber` — Operace,
- `OrderNumber` — Zakázka,
- `OperatorNumber` — Operátor,
- `NokDateTime` — Datum,
- `DeviceNumber` — Device,
- `WorkingPlaceNumber` — Stojan,
- `BilboxNumber` — BbxNum.

Editovatelné sloupce:

- `Komponent`,
- `Vada`,
- `PreviousOperation`,
- `OperatorNumbers`,
- `DefectDescription`.

## Chování editace

Editace má odpovídat stránce `Index.razor`:

1. Kliknutí na běžnou část řádku otevře inline editaci.
2. Kliknutí na checkbox pouze změní výběr a neotevře editaci.
3. Při přechodu na jiný řádek se aktuální lokální změny nejprve commitnou do modelu gridu.
4. Výběr komponenty omezí seznam vad.
5. Změna komponenty smaže neplatnou dříve vybranou vadu.
6. Změna předchozí operace obnoví seznam operátorů.
7. Změny vybraných hodnot se mohou přenášet do následujících řádků stejným fill-down mechanismem.
8. Změněné buňky dostanou třídu `grid-modified-cell`.

Pokud incident obsahuje historické nebo uzavřené NOK položky, musí dokumentace incidentového API určit, zda jsou tyto řádky editovatelné. Výchozí pravidlo má být:

- otevřený incident: editace povolena podle oprávnění,
- uzavřený incident: pouze čtení,
- řádek z předchozí směny: zvýrazněný a bez výběru/editace stejně jako v retestu.

## Lookup API pro grid

Pro editory lze znovu použít existující endpointy:

```http
GET /api/Bilbox/getKomponentVady
GET /api/Bilbox/getOperationPredecessors?orderNumber={orderNumber}&operationNumber={operationNumber}
GET /api/Bilbox/getOperatorNumbersByOrderOperation?orderNumber={orderNumber}&operationNumber={operationNumber}
```

Lookup data mají být cachována stejně jako na retestové stránce podle dvojic:

```text
OrderNumber + OperationNumber
OrderNumber + PreviousOperation
```

## Uložení změn v detailu incidentu

Před implementací je nutné rozlišit dva scénáře.

### Detail slouží pouze jako další vstup do retestu

V tomto případě lze znovu použít existující:

```http
POST /api/Sse/retestMode/{bilbox}/{delayedRetest}
POST /api/Bilbox/addRetestNok
```

Chování bude stejné jako na stránce `Index.razor`: nejprve se Bilbox přepne do režimu Retest a následně se uloží NOK seznam.

### Detail slouží k úpravě NOK v rámci incidentu

Existující `POST /api/Bilbox/addRetestNok` ukládá seznam do tabulky `QualityCardBilboxes`; není to endpoint pro aktualizaci incidentu podle `incidentId`. Pokud mají změny zůstat součástí incidentu, musí existující incidentové API poskytovat vlastní update endpoint, například:

```http
PUT /api/incidents/{incidentId}/nok
```

Uvedený endpoint je pouze požadovaný kontraktický tvar. Před implementací jej musí agent nahradit skutečnou routou existujícího API.

Payload by měl obsahovat incidentový identifikátor a aktualizované NOK položky. Není vhodné ukládat změny pouze lokálně v gridu nebo volat `addRetestNok`, pokud cílem není založit retestové záznamy.

## Požadovaný datový tok detailu

```text
Kliknutí na incident
 └── navigace na /incidents/{incidentId}

Inicializace detailu
 ├── načtení incidentu z existujícího incidentového API
 ├── získání Bilboxu/stojanu nebo incidentových NOK ID
 ├── načtení NOK položek
 ├── GET /api/Bilbox/getKomponentVady
 └── zobrazení hlavičky a gridu

Kliknutí na NOK řádek
 ├── načtení předchozích operací
 ├── načtení operátorů
 └── otevření inline editace

Uložení
 ├── validace vybraných řádků
 ├── commit editovaného řádku
 ├── uložení přes správný incidentový nebo retestový endpoint
 ├── obnovení detailu
 └── zobrazení úspěchu nebo chyby
```

## Instrukce pro implementujícího agenta

1. Nejdříve najdi existující incidentovou stránku, incidentovou službu a API kontrakt.
2. Nepředpokládej názvy `Incident`, `IncidentId` ani endpointy `/api/incidents`, dokud nejsou potvrzené ve zdrojovém kódu.
3. Po kliknutí na incident naviguj na detail s jeho ID.
4. Znovu použij vzhled a chování gridu z `RetestWebAssembly/Pages/Index.razor`.
5. Použij existující `QualityCardBilbox`, pokud incidentové API vrací kompatibilní data.
6. Pokud incidentové API vrací jiný model, vytvoř mapovací model pouze v klientské vrstvě.
7. Pro komponenty, vady, předchozí operace a operátory použij existující endpointy `BilboxController`.
8. Ověř, zda `getActualShiftNOK` vrací přesně NOK daného incidentu; samotná shoda Bilboxu nemusí být dostatečná.
9. Pro uložení použij `SetRetestMode` pouze tehdy, pokud je detail určen k zahájení retestu.
10. Pokud se mají upravovat data incidentu, použij skutečný update endpoint incidentového API, nikoli automaticky `addRetestNok`.
11. Zachovej loading overlay, chybový popup, inline editaci, fill-down a zvýraznění neuložených buněk.
12. Přidej test alespoň pro navigaci na detail, načtení NOK, validaci a uložení změn.
