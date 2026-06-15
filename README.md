# BaobaTesterBox — Documentation Technique Complète

> Framework de test automatisé mobile pour l'application Baoba Maestro (Android / WebView MAUI)
> Stack : C# / .NET 9 / NUnit / Appium / UiAutomator2

---

## Table des matières

1. [Vue d'ensemble](#1-vue-densemble)
2. [Architecture du projet](#2-architecture-du-projet)
3. [Couche Core](#3-couche-core)
   - [Driver](#31-driver)
   - [JsExecutor](#32-jsexecutor)
   - [Navigation](#33-navigation)
   - [Locators](#34-locators)
   - [Interactions](#35-interactions)
   - [Waiter](#36-waiter)
   - [Loggings](#37-loggings)
4. [Couche Domain](#4-couche-domain)
   - [Models](#41-models)
   - [Configuration](#42-configuration)
5. [Tests](#5-tests)
   - [Infrastructure](#51-infrastructure)
   - [SanityChecks](#52-sanitychecks)
   - [Tests de navigation](#53-tests-de-navigation)
6. [Scripts JavaScript](#6-scripts-javascript)
7. [Configuration (appsettings.json)](#7-configuration-appsettingsjson)
8. [Flux d'exécution complets](#8-flux-dexécution-complets)
9. [Conventions et règles d'architecture](#9-conventions-et-règles-darchitecture)
10. [Dépendances NuGet](#10-dépendances-nuget)

---

## 1. Vue d'ensemble

BaobaTesterBox est un framework de test E2E (End-to-End) conçu pour automatiser les tests de l'application mobile **Baoba Maestro** (Android). L'application cible est une WebView MAUI qui charge un moteur XSL/XSLT. La quasi-totalité des interactions avec l'application se fait via **injection JavaScript** dans la WebView plutôt que via les sélecteurs natifs Appium, car le DOM est généré dynamiquement par le moteur XSL.

### Contexte technique important

- L'application Baoba Maestro est une **SPA XSL/XSLT** embarquée dans une WebView Android (MAUI).
- La navigation entre pages se fait via deux fonctions JavaScript exposées par l'application :
  - `go(actionId, rubriqueId, p1, p2, p3, p4)` — Navigation standard sans module
  - `openPoultryModal(moduleId, rubriqueId, null, batchId)` — Navigation vers une fiche avec module (ex : Fiche ICA, rubrique 542)
- Le `PageSource` retourné par Appium est souvent périmé ; on utilise `document.documentElement.outerHTML` via JS pour obtenir le DOM réel.
- Les éléments DOM injectés dynamiquement ne sont pas détectables par le protocole W3C standard d'Appium ; on passe par des scripts JS.

---

## 2. Architecture du projet

```
BaobaTesterBox/
├── BaobaTesterBox.csproj           # Projet unique (mono-assembly)
│
├── src/
│   ├── BaobaTesterBox.Cli/         # Point d'entrée CLI (stub, non utilisé)
│   │
│   ├── BaobaTesterBox.Core/        # Logique technique pure (pas de dépendance métier)
│   │   ├── Driver/                 # Gestion du driver Appium (Singleton + Service)
│   │   ├── Interactions/           # Clic et saisie sur la WebView
│   │   ├── JsExecutor/             # Exécuteur de scripts JS déportés
│   │   │   └── ScriptJS/           # Fichiers .js autonomes
│   │   ├── Locators/               # Scanner du DOM (cache d'éléments)
│   │   ├── Loggings/               # Service de log (console + fichier)
│   │   ├── Navigation/             # Service de navigation entre pages
│   │   └── Waiter/                 # Attente de conditions DOM/JS
│   │
│   └── BaobaTesterBox.Domain/      # Modèles et configuration
│       ├── Configuration/          # AppConfigProvider (lecture appsettings.json)
│       └── Models/                 # DTOs / Options
│
└── Tests/
    ├── BaobaTesterBox.Tests/
    │   ├── Infrastructure/         # BaseTest (SetUp/TearDown commun)
    │   └── SanityChecks/           # Tests unitaires et d'intégration par module
    ├── FicheICA(542)/              # Tests E2E de la Fiche ICA (commentés, WIP)
    ├── TestNavigation.cs           # Test de navigation brut (référence fonctionnelle)
    └── TestNavigation2.0.cs        # Test de navigation via le service (version cible)
```

### Principe de séparation des langages

> **Règle absolue** : Aucun code JavaScript n'est écrit en dur dans les fichiers C#.
> Tout script JS est externalisé dans un fichier `.js` dédié sous `ScriptJS/`,
> et exécuté via `AppiumJsExecutor` qui charge le fichier à la volée.

---

## 3. Couche Core

### 3.1 Driver

#### `AppiumDriverManager`
**Fichier** : `src/BaobaTesterBox.Core/Driver/AppiumDriverManager.cs`
**Namespace** : `BaobaTesterBox.Core.Driver`

Singleton thread-safe qui possède l'instance de l'`AndroidDriver`. C'est le seul endroit du projet où le driver est instancié.

```csharp
// Accès à l'instance unique
AppiumDriverManager.Singleton

// Initialisation
AppiumDriverManager.Singleton.Initialize(serverUri, appiumOptions, timeoutSeconds, implicitWaitSeconds);

// Accès au driver actif
AppiumDriverManager.Singleton.Instance

// Libération
AppiumDriverManager.Singleton.Dispose();
```

**Comportement** :
- Pattern Singleton avec double-check locking (`_lock`)
- Si `Dispose()` a été appelé, `Singleton` recrée automatiquement une nouvelle instance propre
- `Instance` lève `ObjectDisposedException` si appelé après `Dispose()`
- `Instance` lève `InvalidOperationException` si appelé avant `Initialize()`

---

#### `AppiumDriverService`
**Fichier** : `src/BaobaTesterBox.Core/Driver/AppiumServiceManager.cs`
**Namespace** : `BaobaTesterBox.Core.Driver`

Façade de haut niveau qui orchestre l'initialisation complète du driver à partir de la configuration. C'est ce service qui est utilisé dans les tests via `BaseTest`.

```csharp
var service = new AppiumDriverService();

// Initialise le driver avec toute la config Appium
service.InitializeService();

// Expose le driver actif (via le Singleton interne)
AndroidDriver driver = service.Driver;

// Bascule du contexte NATIVE_APP vers la WEBVIEW active (avec wait de 30s)
service.BasculeVersWebView();

// Redémarre l'application à chaud (Terminate + Activate) sans détruire la session
service.RelaunchApp("baoba.maestro.mobileapp");

// Ferme la session driver proprement
service.TerminateService();
```

**Logique de `InitializeService()`** :
- Si `ApkPath` est vide → configure les options `noReset`, `dontStopAppOnReset`, `enforceAppInstall: false` (mode "attache sans réinstaller")
- Si `ApkPath` est renseigné → installe/relance l'APK avec `autoGrantPermissions`
- Ajoute toujours `ensureWebviewsHavePages: true` et `chromedriverArgs: --whitelisted-ips=127.0.0.1`

**`BasculeVersWebView()`** :
- Utilise un `WebDriverWait` de 30 secondes
- Attend que `Driver.Contexts` contienne un contexte avec "WEBVIEW"
- Sélectionne le premier contexte WEBVIEW trouvé

---

### 3.2 JsExecutor

#### `AppiumJsExecutor`
**Fichier** : `src/BaobaTesterBox.Core/JsExecutor/AppiumJsExecutor.cs`
**Namespace** : `BaobaTesterBox.Core.ScriptExecutor`

Exécuteur central de scripts JavaScript. Charge les fichiers `.js` depuis le disque, les prépare (injection automatique du `return` si absent), et les envoie au driver Appium.

```csharp
var executor = new AppiumJsExecutor();

// Exécution sans argument
object? result = executor.Execute(JsScriptType.AppiumInputFetchVisible);

// Exécution avec arguments
object? result = executor.Execute(JsScriptType.AppiumNavigationModuleGo, moduleId, rubriqueId, batchId);
```

**Chemin de recherche des scripts** :
```
{AppDomain.CurrentDomain.BaseDirectory}/src/BaobaTesterBox.Core/JsExecutor/ScriptJS/{NomEnum}.js
```

**Logique de préparation du script** :
1. Si le script contient `window.jsResult` → ajoute `return window.jsResult;` à la fin
2. Sinon, si le script ne commence pas par `return` et ne contient pas `return ` → préfixe avec `return `
3. Les scripts IIFE `(function() { ... })()` sont transparents car le `return` est injecté devant la parenthèse ouvrante

**⚠️ Règle critique** : Les scripts qui déclenchent un changement de page (navigation) ne doivent **pas** utiliser une IIFE avec un `return` interne, car ChromeDriver crashe si la page change pendant qu'il attend la valeur de retour. Pour la navigation, les scripts sont des appels directs sur une seule ligne.

---

#### `JsScriptType` (enum)
**Fichier** : `src/BaobaTesterBox.Core/JsExecutor/enum.cs`
**Namespace** : `BaobaTesterBox.Core.ScriptExecutor.Enum`

```csharp
public enum JsScriptType
{
    AppiumInputFetchVisible,       // → AppiumInputFetchVisible.js
    AppiumInputFillSequentially,   // → AppiumInputFillSequentially.js
    AppiumClickElement,            // → AppiumClickElement.js
    AppiumNavigationGo,            // → AppiumNavigationGo.js
    AppiumNavigationModuleGo       // → AppiumNavigationModuleGo.js
}
```

Le nom de chaque valeur d'enum correspond **exactement** au nom du fichier `.js` associé (sans l'extension).

---

### 3.3 Navigation

#### `AppiumNavigationService`
**Fichier** : `src/BaobaTesterBox.Core/Navigation/AppiumNavigationService.cs`
**Namespace** : `BaobaTesterBox.Core.Navigation`

Service de navigation entre les pages de l'application. Expose deux méthodes raccourcies et une méthode générique.

```csharp
var nav = new AppiumNavigationService();

// Navigation standard (sans module) — appelle go()
nav.Go(rubriqueId: 100);
nav.Go(rubriqueId: 100, actionId: 2, p1: "val1");

// Navigation avec module (Fiche ICA, etc.) — appelle openPoultryModal()
nav.ModuleGo(rubriqueId: 542, moduleId: 5, batchId: 409);

// Navigation via modèle générique
nav.Naviguer(new AppiumNavigationOptions
{
    RubriqueId = 542,
    ModuleId = 5,
    BatchId = 409
});
```

**Aiguillage interne de `Naviguer()`** :
- Si `ModuleId` et `BatchId` sont renseignés → délègue à `ModuleGo()` → `AppiumNavigationModuleGo.js`
- Sinon → délègue à `Go()` → `AppiumNavigationGo.js`

**Ordre des arguments envoyés aux scripts JS** :

| Méthode | Script JS | arguments[0] | arguments[1] | arguments[2] |
|---------|-----------|-------------|-------------|-------------|
| `Go()` | `AppiumNavigationGo.js` | actionId | rubriqueId | p1..p4 |
| `ModuleGo()` | `AppiumNavigationModuleGo.js` | moduleId | rubriqueId | batchId |

---

### 3.4 Locators

#### `AppiumElementScanner`
**Fichier** : `src/BaobaTesterBox.Core/Locators/AppiumElementScanner.cs`
**Namespace** : `BaobaTesterBox.Core.Locators`

Scanner statique du DOM. Analyse la page HTML via HtmlAgilityPack, cartographie tous les éléments interactifs (`button`, `input`, `a`) et les stocke dans un cache interne.

```csharp
// Initialisation (obligatoire avant tout scan)
AppiumElementScanner.Init(driver);

// Scan complet de la page active
AppiumElementScanner.AnalyzeCurrentPage();

// Récupération de la liste des IDs cartographiés
List<string> ids = AppiumElementScanner.GetLastDetectedIds();

// Récupération d'un élément par son SmartId
IWebElement? element = AppiumElementScanner.GetElement("button_submit_1");

// Récupération des inputs visibles uniquement (via script JS)
List<IWebElement> inputs = AppiumElementScanner.GetVisibleInputsOnly();
```

**Format du SmartId** : `{tagName}_{attribute}_{compteur}`
- Exemple : `button_submit_1`, `input_text_3`, `a_element_2`

**Logique de `AnalyzeCurrentPage()`** :
1. Vide le cache
2. Bascule vers la WebView si nécessaire
3. Récupère le HTML via `document.documentElement.outerHTML` (pas `PageSource` car périmé)
4. Parse avec HtmlAgilityPack
5. Attend jusqu'à 10 secondes que le nœud racine (`body` par défaut) apparaisse
6. Pour chaque nœud interactif :
   - Construit le SmartId et le XPath absolu
   - Tente `driver.FindElement(By.XPath(...))` 
   - En cas d'échec W3C, fallback via `document.evaluate(XPath)` en JS
7. Stocke dans le cache `Dictionary<string, IWebElement>`

---

#### `AppiumInputScanner`
**Fichier** : `src/BaobaTesterBox.Core/Locators/AppiumInputScanner.cs`
**Namespace** : `BaobaTesterBox.Core.Locators`

Scanner dédié aux champs de saisie. Complément léger d'`AppiumElementScanner` pour extraire rapidement les inputs visibles sans faire un scan complet de la page.

```csharp
AppiumInputScanner.Init(driver);
List<IWebElement> inputs = AppiumInputScanner.GetVisibleInputsOnly();
```

Utilise `AppiumInputFetchVisible.js` via `AppiumJsExecutor`.

---

### 3.5 Interactions

#### `AppiumElementActionsService`
**Fichier** : `src/BaobaTesterBox.Core/Interactions/AppiumElementActionService.cs`
**Namespace** : `BaobaTesterBox.Core.Interactions`

Service d'actions sur les éléments du DOM. Récupère les éléments depuis le cache du scanner et effectue des actions robustes.

```csharp
var actions = new AppiumElementActionsService(driver);

// Clic (avec cascade de stratégies)
actions.Click("button_submit_1");

// Saisie dans un champ (avec fallback JS)
actions.FillField("input_text_1", "ma valeur");

// Saisie séquentielle de tous les inputs visibles dans l'ordre du DOM
actions.FillFieldsInOrder("email@test.com", "monPassword");

// Clic via sélecteur CSS (via script JS)
actions.ClickOnElement("#btnValidateSynchro");
```

**`FillFieldsInOrder()`** : Délègue entièrement à `AppiumInputFillSequentially.js`. Passe le tableau de valeurs comme `arguments[0]` au script.

**`ClickOnElement()`** : Délègue à `AppiumClickElement.js`. Renvoie `ERR_ELEMENT_NOT_FOUND` si l'élément n'est pas dans le DOM.

---

#### `AppiumClickerService`
**Fichier** : `src/BaobaTesterBox.Core/Interactions/AppiumClickerService.cs`
**Namespace** : `BaobaTesterBox.Core.Interactions`

Stratégies de clic en cascade utilisées par `AppiumElementActionsService`.

**Ordre des stratégies** :
1. `arguments[0].click()` — JS direct
2. `dispatchEvent(new MouseEvent('click', {bubbles: true, cancelable: true}))` — Événement JS
3. Tap par coordonnées — via `PointerInputDevice` (Appium W3C Actions)
4. `element.Click()` — Fallback Selenium natif

---

### 3.6 Waiter

#### `AppiumWaiterService`
**Fichier** : `src/BaobaTesterBox.Core/Waiter/AppiumWaiterService.cs`
**Namespace** : `BaobaTesterBox.Core.Waiter`

Service d'attente basé sur un polling manuel (pas de `WebDriverWait`). Toutes les méthodes retournent un `bool` (succès ou timeout).

```csharp
var waiter = new AppiumWaiterService(driver, options);

// Attend qu'une variable JS globale soit définie et non vide
bool ok = waiter.WaitForJsVariable("globalPoultryBatchId");

// Attend d'être sur une page spécifique (via oldrubriqueid)
bool ok = waiter.WaitForPage("542");

// Attend qu'un élément CSS soit visible
bool ok = waiter.WaitForElement("#btnValidateSynchro");

// Alias de WaitForElement pour les inputs
bool ok = waiter.WaitForInputVisible("#password");
```

**Logique de polling** :
- Boucle `while (DateTime.Now < deadline)`
- Intervalle par défaut : 500ms (`PollingIntervalMs`)
- Timeout par défaut : 30s (`TimeoutSeconds`)
- Les exceptions dans la condition sont silencieusement ignorées (DOM pas encore prêt)

---

### 3.7 Loggings

#### `AppiumLoggerService`
**Fichier** : `src/BaobaTesterBox.Core/Loggings/AppiumLoggerService.cs`
**Namespace** : `BaobaTesterBox.Core.Loggings`

Logger statique double sortie : console colorée + fichier texte daté.

```csharp
AppiumLoggerService.LogDebug("message", "NomOrigine");   // Gris console
AppiumLoggerService.LogInfo("message", "NomOrigine");    // Cyan console
AppiumLoggerService.LogWarn("message", "NomOrigine");    // Jaune console
AppiumLoggerService.LogError("message", "NomOrigine", exception); // Rouge console
```

**Format de ligne de log** :
```
[2026-06-15 09:18:35] [INFO] [AppiumNavigationService] : [Navigation] ModuleGo → rubrique 542
```

**Fichier de sortie** : `{LogDirectory}/log_{yyyy-MM-dd}.txt`

**Filtrage par niveau** : Configuré via `MinimumLevel` dans `appsettings.json` (valeurs : `Debug`, `Info`, `Warn`, `Error`).

---

## 4. Couche Domain

### 4.1 Models

#### `AppiumDriverManagerOptions`
**Namespace** : `BaobaTesterBox.Core.Driver`

Options de connexion au driver Appium, mappées depuis la section `AppiumConfig` du JSON.

| Propriété | Type | Description |
|-----------|------|-------------|
| `ServerUrl` | string | URL du serveur Appium (ex: `http://127.0.0.1:4723`) |
| `ApkPath` | string | Chemin vers l'APK à installer. Vide = mode "attache sans réinstaller" |
| `DeviceName` | string | Identifiant physique du device Android |
| `PlatformVersion` | string | Version Android du device |
| `AppPackage` | string | Package Android (ex: `baoba.maestro.mobileapp`) |
| `AppActivity` | string | Activity principale de l'app |
| `ImplicitWaitSeconds` | int | Timeout implicite Selenium en secondes |
| `CommandTimeoutSeconds` | int | Timeout de commande Appium en secondes |
| `AutoGrantPermissions` | bool | Accorde automatiquement les permissions Android |
| `Chromedriver_autodownload` | bool | Téléchargement automatique du ChromeDriver |

---

#### `AppiumNavigationOptions`
**Namespace** : `BaobaTesterBox.Domain.Models`

Modèle générique pour les deux types de navigation.

| Propriété | Type | Défaut | Usage |
|-----------|------|--------|-------|
| `RubriqueId` | int | — | Commun — ID de la page cible |
| `ActionId` | int | 1 | `go()` — premier paramètre |
| `Param1..4` | string | `""` | `go()` — paramètres 3 à 6 |
| `ModuleId` | int? | null | `openPoultryModal()` — premier param |
| `BatchId` | int? | null | `openPoultryModal()` — quatrième param |

Si `ModuleId` et `BatchId` sont non-null → navigation module. Sinon → navigation standard.

---

#### `AppiumWaiterOptions`
**Namespace** : `BaobaTesterBox.Domain.Models`

| Propriété | Type | Défaut |
|-----------|------|--------|
| `TimeoutSeconds` | int | 30 |
| `PollingIntervalMs` | int | 500 |

---

#### `AppiumLoggerOptions`
**Namespace** : `BaobaTesterBox.Core.Config.Models`

| Propriété | Type | Défaut |
|-----------|------|--------|
| `LogDirectory` | string | `C:\Temp\BaobaLogs\` |
| `MinimumLevel` | string | `"Debug"` |
| `LogToConsole` | bool | true |

---

#### `AppiumElementScannerOptions`
**Namespace** : `BaobaTesterBox.Core.Config.Models`

| Propriété | Type | Défaut |
|-----------|------|--------|
| `RootContainerTagName` | string | `"body"` |

---

### 4.2 Configuration

#### `AppConfigProvider`
**Fichier** : `src/BaobaTesterBox.Domain/Configuration/AppConfigProvider.cs`
**Namespace** : `BaobaTesterBox.Domain.Configuration`

Provider statique de configuration. Charge `appsettings.json` au premier accès (constructeur statique).

```csharp
AppConfigProvider.GetDriverOptions()      // → AppiumDriverManagerOptions
AppConfigProvider.GetLogOptions()         // → AppiumLoggerOptions
AppConfigProvider.GetScannerOptions()     // → AppiumElementScannerOptions
AppConfigProvider.GetWaiterOptions()      // → AppiumWaiterOptions
AppConfigProvider.GetNavigationOptions()  // → AppiumNavigationOptions (valeurs globales par défaut)
```

**Stratégie de recherche du fichier JSON** :
1. Cherche d'abord `{BaseDirectory}/Tests/BaobaTesterBox.Tests/appsettings.json`
2. Si absent, utilise `{BaseDirectory}/appsettings.json`

Toutes les méthodes `Get*()` ont un fallback avec des valeurs par défaut si la section correspondante n'existe pas dans le JSON (évite les crashes en l'absence de config).

---

## 5. Tests

### 5.1 Infrastructure

#### `BaseTest`
**Fichier** : `Tests/BaobaTesterBox.Tests/Infrastructure/BaseTest.cs`
**Namespace** : `BaobaTesterBox.Tests.Infrastructure`

Classe de base abstraite dont héritent tous les tests NUnit. Gère le cycle de vie complet de la session Appium.

```csharp
// Propriétés disponibles dans les classes de test
AppiumService    // AppiumDriverService — accès complet au service
Driver           // AndroidDriver — raccourci vers le driver actif
Waiter           // AppiumWaiterService — service d'attente
Actions          // AppiumElementActionsService — service d'actions
Js               // IJavaScriptExecutor — raccourci pour exécuter du JS
```

**`[SetUp]` (`BaseSetUp`)** :
1. Crée une nouvelle instance de `AppiumDriverService`
2. Appelle `InitializeService()` (démarre Appium, connect au device)
3. Instancie `AppiumWaiterService` et `AppiumElementActionsService`

**`[TearDown]` (`BaseTearDown`)** :
- Appelle `AppiumService.TerminateService()` → `AppiumDriverManager.Dispose()` → `driver.Quit()`

---

### 5.2 SanityChecks

#### `LoggerTests`
**Fichier** : `SanityChecks/Loggings/AppiumLoggerTest.cs`

Test de smoke du logger. Vérifie que les 4 niveaux s'écrivent sans exception.

---

#### `AppiumDriverServiceTests`
**Fichier** : `SanityChecks/Drivers/AppiumDriverServiceTest.cs`

Vérifie que le service démarre, expose un driver non-null, et bascule vers la WebView.

---

#### `AppiumDriverManagerTests`
**Fichier** : `SanityChecks/Drivers/AppiumDriverManagerTest.cs`

Tests unitaires du Singleton `AppiumDriverManager` :
- Initialisation avec options → driver non-null
- Accès à `Instance` avant init → `InvalidOperationException`
- Accès à `Instance` après `Dispose()` → `ObjectDisposedException`

---

#### `AppiumElementScannerTests`
**Fichier** : `SanityChecks/AppiumElementScannerTest/AppiumElementScannerTest.cs`

Test système complet du scanner : init, scan de la page active, récupération du cache, vérification que chaque élément indexé est non-null.

---

#### `AppiumHotScannerStandaloneTest`
**Fichier** : `SanityChecks/AppiumElementScannerTest/ScanCurrentStageOnly.cs`

Test `[Explicit]` (ne s'exécute pas dans une suite normale). Permet de scanner l'écran courant **sans redémarrer l'application**. Technique : vide `ApkPath` dans les options pour forcer le mode "attache" (`noReset`), restaure le chemin dans le `finally`.

---

#### `DiagnosticPageTests`
**Fichier** : `SanityChecks/testInput.cs`

Outil de diagnostic du DOM. Extrait et affiche tous les `<input>` de la page avec leurs attributs (`id`, `name`, `type`, `placeholder`) et leurs propriétés Appium (`Displayed`, `Enabled`, `Size`). Utile pour identifier les sélecteurs corrects.

---

#### `NavigationDirecteTestsS` (Espion de contexte)
**Fichier** : `SanityChecks/AppiumElementScannerTest/ScanParametterPage.cs`

Lit les variables globales JS de la page (`farmId`, `globalPoultryBatchId`, `oldrubriqueid`, `globalPoultryModuleid`, `rubriqueid`) et les affiche de manière lisible. Permet de connaître le contexte exact de navigation de l'application à un instant T.

```
==================================================
 🏠 FARM ID             : N/A
 📦 BATCH ID            : 409
 📄 ANCIENNE RUBRIQUE   : N/A
 🧩 MODULE ID           : 5
 📍 RUBRIQUE ACTUELLE   : 5599
==================================================
```

---

#### `LoginTests`
**Fichier** : `SanityChecks/TestRefactor/LoginTest.cs`

Test de connexion complet via l'API `AppiumElementActionsService` :
1. RAZ du contexte (`NATIVE_APP`)
2. Bascule vers la WebView
3. Ouvre la modale `#authenticateModal` via JS Bootstrap
4. Attend que `#password` soit interactif
5. Saisit les credentials via `FindElement(By.CssSelector(...))`
6. Clique sur `#btnValidateSynchro`

---

#### `AppiumWaiterServiceIsolatedTest`
**Fichier** : `SanityChecks/Waiter/WaiterServiceTest.cs`

Test d'intégration du waiter. Attend l'apparition de `#login-username` après le lancement de l'app.

---

### 5.3 Tests de navigation

#### `NavigationDirecteTests` (TestNavigation.cs) — Référence fonctionnelle
**Fichier** : `Tests/TestNavigation.cs`

Test brut de référence qui fonctionne. Exécute `openPoultryModal` directement via `IJavaScriptExecutor` sans passer par le service.

```csharp
string scriptRepare = "openPoultryModal(arguments[0], arguments[1], null, arguments[2]);";
jsExecutor.ExecuteScript(scriptRepare, 5, 542, 409);
```

Ce test est la **source de vérité** pour les paramètres et l'ordre des arguments de navigation.

---

#### `NavigationDirecteTests2` (TestNavigation2.0.cs) — Version via service
**Fichier** : `Tests/TestNavigation2.0.cs`

Version cible utilisant le service de navigation. Une seule ligne d'interaction :

```csharp
new AppiumNavigationService().ModuleGo(542, 5, 409);
```

---

## 6. Scripts JavaScript

Tous les scripts sont dans `src/BaobaTesterBox.Core/JsExecutor/ScriptJS/`.

### `AppiumInputFetchVisible.js`
Retourne la liste des `IWebElement` correspondant aux inputs visibles de la page (exclusion de `submit`, `button`, `checkbox`, `radio`, `hidden`). Utilise `getComputedStyle` pour vérifier la visibilité réelle.

**Retourne** : `IReadOnlyCollection<IWebElement>` (cast côté C#)

---

### `AppiumInputFillSequentially.js`
Remplit les inputs visibles dans l'ordre du DOM avec les valeurs reçues.

**Arguments** : `arguments[0]` = tableau de strings (ex: `["email@test.com", "password"]`)

**Retourne** : `"SUCCESS_FILLED_{n}"` ou `"ERR_NO_VALUES_PROVIDED"` / `"ERR_NO_VISIBLE_INPUTS_FOUND"`

Déclenche les événements `input` et `change` avec `bubbles: true` pour compatibilité avec les frameworks réactifs (Angular, Vue, MAUI Blazor).

---

### `AppiumClickElement.js`
Clique sur un élément via sélecteur CSS.

**Arguments** : `arguments[0]` = sélecteur CSS (ex: `"#btnValidateSynchro"`)

**Retourne** : `"SUCCESS_CLICKED"` ou `"ERR_ELEMENT_NOT_FOUND"` ou `"ERR_NO_SELECTOR_PROVIDED"`

Fallback spécifique : si le sélecteur contient `btnValidateSynchro` et que l'élément est introuvable, tente `input[value="Valider"]` et `input[onclick*="loginDuglu"]`.

---

### `AppiumNavigationGo.js`
Appel direct de `go()` avec les arguments Appium.

```js
go(arguments[0], arguments[1], arguments[2], arguments[3], arguments[4], arguments[5]);
```

**Arguments** : `[0]` = actionId, `[1]` = rubriqueId, `[2..5]` = p1..p4

---

### `AppiumNavigationModuleGo.js`
Appel direct de `openPoultryModal()` avec les arguments Appium.

```js
openPoultryModal(arguments[0], arguments[1], null, arguments[2])
```

**Arguments** : `[0]` = moduleId, `[1]` = rubriqueId, `[2]` = batchId

**⚠️ Pas d'IIFE** : Ce script est une ligne directe pour éviter le crash ChromeDriver lié au changement de page pendant l'attente du `return`.

---

### `AppiumNavigation.js` (archive / non utilisé activement)
Version précédente avec IIFE et `setTimeout`. Conservé pour référence mais remplacé par les deux fichiers séparés ci-dessus.

---

## 7. Configuration (appsettings.json)

**Fichier** : `Tests/BaobaTesterBox.Tests/appsettings.json`

```json
{
  "AppiumConfig": {
    "ServerUrl": "http://127.0.0.1:4723",
    "ApkPath": "C:\\Dev\\...\\baoba.maestro.mobileapp-Signed.apk",
    "DeviceName": "RZCY90ZKHQK",
    "PlatformVersion": "16.0",
    "AppPackage": "baoba.maestro.mobileapp",
    "AppActivity": "crc643d469d81e9bd1cea.MainActivity",
    "ImplicitWaitSeconds": 120,
    "CommandTimeoutSeconds": 180,
    "AutoGrantPermissions": true,
    "chromedriver_autodownload": true
  },
  "LoggerConfig": {
    "LogDirectory": "C:\\Logs\\",
    "MinimumLevel": "Debug"
  },
  "AppiumWaiterConfig": {
    "TimeoutSeconds": 30,
    "PollingIntervalMs": 500
  },
  "ScannerConfig": {
    "RootContainerTagName": "body"
  },
  "Database": {
    "SqliteDbName": "baoba.db",
    "PostgresConnectionString": "Host=192.168.1.85;Database=baoba;..."
  },
  "TestCredentials": {
    "Email": "baoba.uat.tessa1@yopmail.com",
    "Password": "@123456789Abc"
  }
}
```

**Mode "attache sans réinstaller"** : Mettre `ApkPath` à `""` pour se connecter à l'application déjà ouverte sur le device sans la redémarrer.

---

## 8. Flux d'exécution complets

### Flux : Test de navigation vers la Fiche ICA (rubrique 542)

```
[NUnit] Test démarre
    ↓
[BaseTest.BaseSetUp]
    → new AppiumDriverService()
    → InitializeService()
        → AppiumDriverManager.Singleton.Initialize(...)
        → new AndroidDriver(serverUri, options, commandTimeout)
    → new AppiumWaiterService(driver, options)
    → new AppiumElementActionsService(driver)
    ↓
[Test body]
    → AppiumService.BasculeVersWebView()
        → WebDriverWait(30s).Until(contexts.Any(WEBVIEW))
        → driver.Context = "WEBVIEW_baoba.maestro..."
    ↓
    → new AppiumNavigationService().ModuleGo(542, 5, 409)
        → _jsExecutor.Execute(JsScriptType.AppiumNavigationModuleGo, 5, 542, 409)
            → fileName = "AppiumNavigationModuleGo.js"
            → fullpath = "{BaseDir}/src/.../ScriptJS/AppiumNavigationModuleGo.js"
            → scriptContent = "openPoultryModal(arguments[0], arguments[1], null, arguments[2])"
            → Pas de "return " → inject → "return openPoultryModal(arguments[0], arguments[1], null, arguments[2])"
            → driver.ExecuteScript(script, [5, 542, 409])
                → [Application Android] : openPoultryModal(5, 542, null, 409)
                → Page change → Fiche ICA rubrique 542 s'affiche
    ↓
    → Thread.Sleep(3000) // Observation visuelle
    ↓
[BaseTest.BaseTearDown]
    → AppiumService.TerminateService()
    → AppiumDriverManager.Singleton.Dispose()
    → driver.Quit()
```

---

### Flux : Scan du DOM et récupération d'éléments

```
AppiumElementScanner.Init(driver)
    ↓
AppiumElementScanner.AnalyzeCurrentPage()
    → SwitchToWebViewContext()
    → GetFreshPageSource() → js.ExecuteScript("return document.documentElement.outerHTML;")
    → HtmlDocument.LoadHtml(html)
    → SelectNodes(".//button | .//input | .//a")
    → Pour chaque nœud :
        → Construit smartId ("button_submit_1") et XPath ("/body//button[1]")
        → driver.FindElement(By.XPath(xpath))  // Tente W3C
        → Si échec → js.ExecuteScript("return document.evaluate(xpath, ...)")  // Fallback JS
        → _cache.Add(smartId, webElement)
    ↓
AppiumElementScanner.GetElement("button_submit_1")
    → return _cache["button_submit_1"]
```

---

## 9. Conventions et règles d'architecture

### Règles absolues

| Règle | Raison |
|-------|--------|
| **Jamais de JS inline dans le C#** | Séparation des langages, maintenabilité |
| **Tout script JS = fichier .js dans ScriptJS/** | Lisibilité, possibilité de tester le JS isolément |
| **Pas d'IIFE pour les scripts de navigation** | ChromeDriver crashe si la page change avant le `return` |
| **`document.documentElement.outerHTML` via JS pour le DOM** | `PageSource` d'Appium est périmé sur les SPA XSL |
| **`AppiumDriverManager.Singleton` pour le driver** | Instance unique, évite les sessions multiples |

### Conventions de nommage

- Services : `Appium{Domaine}Service` (ex: `AppiumNavigationService`)
- Managers : `Appium{Domaine}Manager` (ex: `AppiumDriverManager`)
- Options/Models : `Appium{Domaine}Options` (ex: `AppiumNavigationOptions`)
- Scripts JS : `Appium{Action}.js`, correspondant exactement à la valeur de l'enum
- SmartIds : `{tagName}_{attribute}_{compteur}` (généré automatiquement par le scanner)

### Structure des tests

```csharp
[TestFixture]
public class MonTest : BaseTest  // Hérite de BaseTest
{
    private const string Origine = "MonTest";  // Pour les logs

    [Test]
    public void Test_Ce_Que_Ca_Fait()
    {
        AppiumLoggerService.LogInfo("=== DÉBUT ===", Origine);
        AppiumService.BasculeVersWebView();
        
        // ... logique de test ...
        
        AppiumLoggerService.LogInfo("=== FIN ===", Origine);
    }
}
```

---

## 10. Dépendances NuGet

| Package | Usage |
|---------|-------|
| `Appium.WebDriver` | Driver Appium pour Android (`AndroidDriver`) |
| `Selenium.WebDriver` | Base Selenium (By, WebDriverWait, IJavaScriptExecutor) |
| `Selenium.Support` | `WebDriverWait` |
| `NUnit` | Framework de test |
| `NUnit3TestAdapter` | Intégration avec le runner de tests |
| `HtmlAgilityPack` | Parsing HTML côté C# dans `AppiumElementScanner` |
| `Microsoft.Extensions.Configuration` | Lecture du `appsettings.json` |
| `Microsoft.Extensions.Configuration.Json` | Support fichier JSON |
| `Newtonsoft.Json` | Sérialisation JSON (scénarios de test) |
| `Npgsql` | Connexion PostgreSQL (prévu, non actif) |
| `Microsoft.Data.Sqlite` | Connexion SQLite (prévu, non actif) |
| `Serilog` | Logger alternatif (présent dans le build, non utilisé activement) |

---

*Documentation générée le 15 juin 2026 — BaobaTesterBox v1.0 (en développement)*
