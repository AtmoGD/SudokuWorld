# Umbau-Plan: SudokuWorld → Puzzle-Plattform

**Stand:** 2026-07-27
**Grundlage:** `puzzle-plattform-spec.md` v1.0 + vier Grundentscheidungen (siehe §2)
**Status:** Entwurf zur Abnahme

---

## 1. Kontext

Es existiert ein Sudoku-Prototyp (36 Commits, Aug–Sep 2024, seither dormant, im Juli 2026 auf Unity 6 hochgezogen). Er soll zu einer Rätsel-/Spiele-Plattform für iOS und Android werden: mehrere Spieltypen, empirisch kalibrierte Schwierigkeit, Daily Puzzles, Backend mit Google-/Apple-Login, und durchgängig spürbares Feedback („Juice").

Der Prototyp trägt davon **einen** Baustein: die Zahleneingabe. Alles andere ist entweder nicht vorhanden, nicht auf Mobile lauffähig oder blockiert die Zielarchitektur aktiv.

Dieses Dokument beantwortet drei Fragen: Was muss weg, was bleibt, in welcher Reihenfolge wird gebaut.

### Ausgangsbefund in einem Absatz

Das Projekt ist am HEAD **nicht lauffähig** (`Vibration.Manager` wird nie zugewiesen, `Cell.cs:90` löst bei jeder Zellauswahl eine NullReferenceException aus). Der Generator hat **keinen Solver und keine Eindeutigkeitsprüfung** — bei niedriger Clue-Zahl entstehen mehrdeutige Puzzles, die der Spieler korrekt lösen kann und die trotzdem als falsch gelten. Das Save-System schreibt per `BinaryFormatter` auf einen **relativen Pfad** und funktioniert auf keinem Zielgerät. `IsSolved()` wirft ~6.500 gefangene Exceptions pro Tastendruck. Es gibt kein einziges `asmdef`, keinen Test, keine Audiodatei, keine Bundle-ID und keine Signierung.

---

## 2. Grundentscheidungen

Vier Weichenstellungen, die alles Folgende tragen:

| # | Entscheidung | Konsequenz |
|---|---|---|
| **G1** | **Scope offen halten** | v1 nur Logik-Rätsel, aber die Verträge werden genre-neutral geschnitten: `IGameModule` **über** `IPuzzleType`. Kostet ~1–2 Tage jetzt, spart einen Kernumbau später. |
| **G2** | **Spec-Reihenfolge** | M0 → M1 Engine → M2 Pipeline → M2b Content Studio → M3 App. Kein Vertical Slice, keine Wegwerf-Arbeit. Die erste vollständig spielbare Version kommt entsprechend spät. |
| **G3** | **Feel (MoreMountains) als Juice-Stack** | MMFeedbacks als deklarative Feedback-Ketten, inkl. Nice Vibrations und DOTween. Bewusst eine dicke Fremd-Abhängigkeit — deshalb strikt hinter einer eigenen Schicht gekapselt (§4.3). |
| **G4** | **Firebase, aber später** | `ITelemetrySink` / `IRemoteConfigSource` ab Tag 1 mit lokaler Implementierung. Firebase wird vor Store-Release angeschlossen. Die Telemetrie-Events aus Spec §6.7 werden trotzdem ab M3 **vollständig** geschrieben. |

### Die fünfte Entscheidung: neues Unity-Projekt

Die Spec (§12.5, D7) nennt genau ein Kriterium dafür, im Bestand aufzuräumen statt neu anzufangen — *„nennenswert konfigurierte Projekteinstellungen: Build-Profile, Signierung, funktionierende iOS/Android-Pipeline, eingerichtetes Input-System"*. Nachgeprüft:

| Kriterium | Ist-Zustand |
|---|---|
| Bundle Identifier | leer (`applicationIdentifier: {}`) |
| Android Signing | kein Keystore |
| iOS Signing | keine Team-ID, kein Auto-Signing |
| Android-Architektur | **ARMv7-only** — seit 2019 Play-Store-Blocker |
| Scripting Backend | nicht gesetzt (Android damit Mono statt IL2CPP) |
| Input System | Legacy (`activeInputHandler: 0`), Package nicht installiert |
| Icons / Splash | keine, Unity-Splash aktiv |
| Gradle-Templates | keine — Firebase braucht sie zwingend |

**Kein einziges Kriterium erfüllt.** Das Projekt ist als WebGL-Einzelspiel gebaut (verrät sich am `Vibration.jslib` und am `docs/`-Deploy), nicht als Mobile-App.

Zusätzlich: Das Repo ist **399 MB** groß, weil `docs/` einen eingecheckten 36-MB-WebGL-Build enthält — bei ~100 KB Quellcode. Ein neues Repo löst das gratis; im Bestand bräuchte es `git filter-repo` und einen History-Rewrite.

→ **Neues Unity-Projekt, neues Repo.** Der Bestand wird als `prototype-v0` getaggt und als Referenz behalten.

---

## 3. Was bleibt, was geht

### 3.1 Behalten — Assets, kein Code

| Element | Behandlung |
|---|---|
| `Assets/Dark UI/` (65 UI-Icons) | Übernehmen, neu strukturieren, **Sprite Atlas anlegen** (aktuell 68 lose PNGs = unnötige Draw Calls) |
| `Assets/Fonts/Odin Rounded` (6 OTF + SDF) | Übernehmen. **Web-Font-Reste löschen** (`.eot`, `.woff`, `.svg` — haben in Unity nichts verloren, ~5 MB). SDF-Fallback-Kette neu aufbauen |
| `Assets/Sprites/Circle.png`, `FloatingInputBackground.png` | Übernehmen — Wedge und Ringe der Eingabe |
| `Assets/Fonts/Painting_With_Chocolate` | Nur wenn gestalterisch gewollt, sonst streichen |

### 3.2 Übernehmen als Wissen — neu implementiert

**Die Zahleneingabe.** Der Kern sind **~95 Zeilen** in `FloatingInput.cs`. Es ist ein richtungsbasiertes **Marking Menu**: Finger auf Zelle → wegziehen → beim Überschreiten eines Schwellwerts fächert sich ein Bogen aus 10 Elementen (X, 1–9) **in Zugrichtung** auf → seitwärts entlang des Bogens wischen wählt → loslassen schreibt.

Die getunten Parameter sind das eigentliche Kapital (Ergebnis der Commits „Bigger Floating Input", „Smoother", „Bigger distance"):

| Parameter | Wert | Bedeutung |
|---|---|---|
| `circleRadius` | 327.5 | ≈ 1.8 Zellbreiten (`cellSize` = 180) |
| `stepSize` | 10.2° | Winkelabstand pro Element |
| Bogen gesamt | ±45° um die Zugrichtung | `angle_i = θ + 45 − 10.2·i` |
| `distanceThreshold` | 250 (DPI-normalisiert `× Screen.width / 1800`) | Auslöseschwelle |
| `inputTimeout` | 0.5 s | Tap-vs-Drag-Grenze |
| Show-Animation | 0.4166 s, Scale 0 → **1.1** → 1.0, gestaffelt | Overshoot-Pop |

**Die drei Designentscheidungen, die es gut machen** — und die beim Port erhalten bleiben müssen:

1. **Die Richtung wird genau einmal gelatcht**, beim Überschreiten des Schwellwerts. Danach dreht der Fächer nicht mehr mit; man wischt nur noch *entlang* des Bogens. Ohne dieses Latch zappelt das Menü.
2. **Zurückziehen zur Zelle bricht ab** — unterhalb des Schwellwerts wird das Menü versteckt und die Auswahl auf −1 gesetzt. Es gibt immer einen Ausweg ohne Eingabe.
3. **Die Zelle bleibt Event-Owner der ganzen Geste.** uGUI feuert `IPointerUpHandler` immer auf dem gedrückten Objekt, auch wenn der Finger weit außerhalb loslässt. Genau darauf baut die Mechanik.

### 3.3 Bewusst *nicht* übernehmen — obwohl es naheliegt

| Element | Warum nicht |
|---|---|
| **Die Animator-Clips der Eingabe** | Der Juice steckt komplett in `.anim`-Dateien, die **pro Objektpfad** gekeyframed sind (`Numbers/Input (0)` … `Input (9)`). Bei anderer Elementzahl müssen alle Clips neu gebaut werden. Das ist der eine Blocker für N ≠ 10 — und der Grund, warum die Animation beim Port durch code-getriebene Tweens ersetzt wird. Die Clips dienen nur noch als **Timing-Referenz**. |
| `FixedInput.cs` / `FixedInputCell.cs` | **Ist keine Eingabemethode.** Nachgeprüft: `SelectValue()` schreibt nirgends in eine Zelle. Die untere Leiste ist ein Ziffern-*Filter* plus Fortschrittsring. Die Idee ist gut, der Code hängt per `GetComponentInParent` am GameField und rechnet hart `/9f`. Neu bauen, wenn gewünscht. |
| `Theme.cs` + `BaseTheme.asset` | Leere Hülle — die Farbfelder wurden in Commit `6ee848f "Dunno"` gelöscht, das Asset enthält verwaiste YAML-Keys. Theming lief stattdessen über **vorgefärbte, per Animator ein-/ausskalierte Layer**. Heißt: Farben sind in Prefabs und Clips eingebacken, ein Dark/Light-Switch würde das Duplizieren aller Prefabs erfordern. Das neue Projekt braucht Runtime-Tokens. |
| `Assets/Prefabs/*` | Alle sudoku-spezifisch und mit eingebackenen Farben. Nur `Icon Button.prefab` ist generisch — und trivial neu gebaut. |
| `Assets/ScreenLogger/` | MIT-Fremdcode von 2016 mit `OnGUI` und `#if UNITY_4_5`-Zweigen, bereits aus der Szene entfernt. Zieht einen `Resources/`-Ordner ins Projekt, was der Schichtung entgegensteht. |
| `Assets/TextMesh Pro/` | Veralteter Legacy-Import (4.7 MB). TMP kommt heute aus `com.unity.ugui`. Der Doppelbestand ist die Quelle von 23 der 32 aktuell modifizierten Dateien. |
| `game.sudoku`, `docs/` | Runtime-Speicherstand bzw. 36-MB-WebGL-Build. Beide eingecheckt. |

### 3.4 Löschen — der gesamte Gameplay-Code

1.670 Zeilen eigener Code, keine Datei über 3/5, kein Test, kein `asmdef`, keine Logik/UI-Trennung.

| Datei | Z. | Warum weg |
|---|---|---|
| `GameField.cs` | 623 | God-Object: Datenmodell + Generator + View-Factory + Layout + Timer + Input + Highlights + Persistenz. `IsSolved()` (`:403`) iteriert `field.Length` statt `GetLength(0)` → 6.561 Durchläufe, ~6.480 gefangene Exceptions mit je zwei `Debug.Log`, **pro Eingabe**. |
| `SudokuGenerator.cs` | 146 | Kein Solver, **keine Eindeutigkeitsprüfung** (`:96`). Statischer, veränderlicher Zustand → nicht thread-safe, obwohl aus `Task.Run` aufgerufen. `[BurstCompile]` auf Code mit `List<T>` und Rekursion greift nicht. |
| `DataManager.cs` | 70 | `BinaryFormatter` auf relativem Pfad. Obsolet, RCE-Vektor, typnamen-gekoppelt → **bricht beim ersten asmdef-Refactoring**. Löscht bei Schreibfehler die Speicherdatei. |
| `Game.cs` | 125 | `FindObjectOfType`-Singleton mit wirkungsloser Awake-Prüfung. Spielstand-Matching über `displayName`-String (`:96`) — Umbenennen einer Difficulty verwaist alle Speicherstände. |
| `Cell.cs` | 175 | 12 Wrapper um `animator.SetBool`, hartkodiertes 3×3-`CellSubgrid`-Enum, `GetCellSubgrid` ist toter Code. |
| `Vibration.cs` | 18 | `Manager` nie zugewiesen → NRE. Nur WebGL. `Vibrate(1000)` = 1 Sekunde für ein „Plop". |
| `Menu.cs`, `GameTopMenu.cs`, `GameSelection.cs`, `GameSelectionEntry.cs` | 99 | Drei Animator-Bools sind kein Navigationssystem; die Spieleliste ist szenen-verdrahtet statt datengetrieben. |
| `FloatingInputValueController.cs` | 21 | Toter Code — `UpdateSlider()` nur in `Awake`, alle 10 Instanzen `active: 0`. |
| `PerformanceTester.cs` | 40 | `Debug.Log`-Benchmark, misst über `await Task.Run` den Frame-Sync mit. |
| `Theme.cs` | 9 | Leere Hülle. |
| `Editor/*` | 72 | `GameFieldEditor` ist als Debug-Grid nützlich — Idee mitnehmen, Code nicht (der „Generate Field"-Button schreibt Speicherstände ins Repo). |

**Zwei Erkenntnisse aus dem Bestand, die in die neue Architektur einfließen:**

- Ohne Solver ist der Generator wertlos. Eindeutigkeitsprüfung, Difficulty-Rating und Hints fallen aus demselben Baustein ab — deshalb ist die Engine M1 und nicht später.
- Theming muss zur Laufzeit über Tokens laufen. Sonst passiert wieder, was `6ee848f` dokumentiert: Man gibt auf und backt die Farben in Animationen ein.

---

## 4. Zielarchitektur

Grundlage ist Spec §6.2. Zwei Ergänzungen, die dort fehlen.

### 4.1 Assembly-Schichtung

```
Assets/_Project/
  Engine/       Puzzle.Engine.asmdef          ← KEINE UnityEngine-Referenz
      Core/       Grid, CandidateGrid, Units, Peers
      Solvers/    UniqueSolver, LogicSolver
      Techniques/ eine Datei pro Technik
      Features/   FeatureExtractor, ScoringModel

  Domain/       Puzzle.Domain.asmdef          ← KEINE UnityEngine-Referenz
      Games/      IGameModule, IGameSession, GameEvent, IGameStats      ← NEU (G1)
      Puzzles/    IPuzzleType : IGameModule, PuzzleSession, Move, UndoStack, HintService

  Content/      Puzzle.Content.asmdef         ContentStore, PackLoader, PuzzleSelector
  Backend/      Puzzle.Backend.asmdef         ITelemetrySink, IRemoteConfigSource + Local*  (G4)
  Feedback/     Puzzle.Feedback.asmdef        FeedbackRouter, Feel-Bindung                  ← NEU (G3)
  UI/           Puzzle.UI.asmdef              Screens/, Widgets/RadialValuePicker, Theme/
  App/          Puzzle.App.asmdef             Bootstrap, SceneFlow, Settings
  Types/Sudoku/ Puzzle.Types.Sudoku.asmdef    SudokuModule, SudokuRenderer, SudokuHintPresenter

  Editor/ContentStudio/  Puzzle.Editor.ContentStudio.asmdef   includePlatforms: ["Editor"]

Assets/Tests/
  EditMode/     Puzzle.Tests.EditMode.asmdef  + GoldenFiles/ (200 Fixtures)
  PlayMode/     Puzzle.Tests.PlayMode.asmdef
```

**Abhängigkeitsrichtung:** `Engine ← Domain ← {Content, Backend, Feedback, UI, Types} ← App`. Rückwärts nie. `Engine` und `Domain` kompilieren gegen .NET Standard 2.1 ohne Unity und laufen damit unverändert im Pipeline-CLI.

### 4.2 Ergänzung 1 — `IGameModule` über `IPuzzleType` (G1)

Die Spec kennt nur Rätsel. Damit später ein Wort- oder Merkspiel nicht die halbe Infrastruktur sprengt, sitzt der Vertrag eine Ebene höher:

```csharp
// Puzzle.Domain/Games — reines C#
public interface IGameModule {
    string ModuleId { get; }
    IGameSession CreateSession(GameStartRequest request);
}

public interface IGameSession {
    string ModuleId { get; }
    TimeSpan ElapsedActive { get; }          // Hintergrundzeit zählt NICHT
    GameOutcome Outcome { get; }
    event Action<GameEvent> Emitted;
    void Pause(); void Resume(); void Finish(GameOutcome outcome);
    ReadOnlySpan<StatValue> Snapshot();      // generischer Statistik-Vertrag
}

public interface IPuzzleType : IGameModule {
    IPuzzleState Deserialize(string data);
    string Serialize(IPuzzleState state);
    bool IsComplete(IPuzzleState state);
    IReadOnlyList<int> GetConflicts(IPuzzleState state);
    Hint? GetHint(IPuzzleState state, HintLevel level);
}
```

`PuzzleSession` aus Spec §6.3 implementiert `IGameSession`. **Telemetrie, Persistenz, Statistik und Juice binden ausschließlich an `IGameSession`** — nie an `PuzzleSession`. Damit steckt ein Nicht-Rätsel-Spiel ohne Bruch ein.

### 4.3 Ergänzung 2 — die Juice-Schicht (G3)

Die Spec sagt zu Juice praktisch nichts. Feel ist `MonoBehaviour`-lastig, `Domain` darf `UnityEngine` nicht referenzieren. Auflösung: **ein Event-Strom, zwei Konsumenten.**

```csharp
// Puzzle.Domain/Games — reines C#, keine Unity-Referenz
public enum GameEventKind {
    SessionStarted, SessionPaused, SessionResumed, SessionFinished,
    CellSelected, ValuePlaced, ValueCleared, InvalidAttempt,
    GroupCompleted, HintRequested, HintRevealed, UndoPerformed,
    ProgressMilestone, Completed
}

public readonly struct GameEvent {
    public GameEventKind Kind;
    public int TargetIndex;      // Zelle, Gruppe, o.ä.
    public byte Value;
    public float Intensity;      // 0..1 — Eskalationsstufe für Feedback
}
```

`Puzzle.Feedback.FeedbackRouter` (Unity-Seite) abonniert `IGameSession.Emitted` und mappt `GameEventKind` → `MMFeedbacks`-Asset. `Puzzle.Backend.TelemetryQueue` abonniert denselben Strom und formt daraus die Events aus Spec §6.7.

Drei Regeln, die das tragen:
1. **Nur `Puzzle.Feedback` referenziert Feel.** Sonst nichts. Ein Wechsel des Juice-Stacks kostet dann eine Assembly, nicht das Projekt.
2. **Feedback ist datengetrieben, nicht gekeyframed.** Ein `ScriptableObject` mappt Event → Feedback-Kette. Genau deshalb wird der Animator der Eingabe beim Port ersetzt: sonst bleibt die Eingabe für immer auf 10 Elemente festgenagelt.
3. **Die Telemetrie hängt am selben Strom.** Damit ist ausgeschlossen, dass Juice-Events und Telemetrie-Events auseinanderlaufen.

> Vor M0.5 prüfen, ob die eingesetzte Feel-Version eigene `asmdef`s mitbringt. Falls nicht, landet sie in `Assembly-CSharp` und reißt die Schichtung ein — dann müssen `asmdef`s nachgezogen werden, bevor irgendetwas darauf aufbaut.

---

## 5. Meilensteine

### M0 — Übernahme & Fundament

Der einzige Meilenstein, der den Altbestand berührt. Aufwand: **3–5 Tage** (die Spec schätzt 1–2, unterschätzt aber das Juice-Fundament und die Animator-Ablösung).

| # | Sub-Task | Ergebnis |
|---|---|---|
| **M0.1** | **Bestand sichern** — *teilweise erledigt* | ✅ Upgrade-Diff als ein Block committet (`2723059`) · ✅ `game.sudoku` untrackt, `.gitignore` um Secrets/`*.slnx`/`.idea` ergänzt (`c320fdf`) · ✅ NRE in `Vibration.cs` an der Ursache behoben (`3c24494`) · ✅ Tag `prototype-v0` gesetzt und gepusht.<br>⬜ **Offen: Kompilierprüfung** des Fixes im Editor.<br>⬜ **Offen: Bildschirmaufnahme der Eingabe** (Shot-Liste in §5.1) — die Referenz, gegen die M0.4 abgenommen wird. Ohne sie merkt man erst Wochen später, dass eine Animation 80 ms zu träge geworden ist. |
| **M0.2** | **Neues Projekt** | Unity-Version fixieren (→ §7 D-A), 2D-URP-Template, Struktur §4.1, alle `asmdef`s leer angelegt, `.gitignore` ergänzt (`.idea/`, `*.slnx`, `*.keystore`, `*.jks`, `google-services.json`, `GoogleService-Info.plist`), neues Repo. Ungenutzte Packages **nicht** mitnehmen (ai.assistant, ai.inference, multiplayer.center, visualscripting, timeline, collab-proxy). Input System statt Legacy. Zusätzlich `.gitattributes` mit `* text=auto` und Binär-Markern anlegen — der Altbestand erzeugt bei jedem `git add` LF/CRLF-Warnungen, das will man im neuen Repo nicht wieder haben. |
| **M0.3** | **Asset-Transfer** | Dark UI + Sprite Atlas, Odin Rounded ohne Web-Font-Reste, die zwei Sprites. Sorting Layers und Quality Tiers anlegen (aktuell: 1 Sorting Layer, 6 Quality-Stufen ohne URP-Zuordnung — effektiv eine einzige Stufe). |
| **M0.4** | **`RadialValuePicker` neu bauen** | Die ~95 Zeilen Mechanik, N-agnostisch. Sechs Änderungen: (1) Ziel ist `RectTransform` + `Action<int>` statt `GameField`; (2) `stepSize` → `totalArc/(n−1)`, damit der Bogen bei jedem N gleich breit bleibt; (3) Elemente zur Laufzeit instanziieren statt 10 von Hand im Prefab; (4) `/1800` → `CanvasScaler.referenceResolution` lesen; (5) `eventData.position` + `pointerId` statt `Input.mousePosition` (Multitouch); (6) Animator → Code-Tween. **Drei Bugs beim Port fixen**, sonst wandern sie mit: fehlendes `return` in `PointerUp` (verschluckt schnelle Flicks), `inputDirection` wird bei PointerUp nicht zurückgesetzt, kein Multitouch. |
| **M0.5** | **Juice-Fundament** | `GameEvent`-Contract in `Domain`, `FeedbackRouter` + Feel in `Puzzle.Feedback`, Event→Feedback-Mapping als ScriptableObject. Die Show/Hide-Animation der Eingabe als MMFeedbacks-Kette mit Code-Stagger nachbauen (Referenz: 0.4166 s, Scale 0 → 1.1 → 1.0, versetzte Starts). |
| **M0.6** | **Isolationsszene** | Leere Szene, nur das Eingabefeld, gespeist von einem Dummy-`IBoardDisplay`. Akzeptanz: läuft ohne **jede** Referenz auf Solver, Generator, Speichern, Timer, Schwierigkeit. Gegen die Aufnahme aus M0.1 prüfen und Timing nachjustieren. |
| **M0.7** | **Haptik-Baseline** | Nice Vibrations auf einem **echten Android-Gerät** verifizieren (Emulator taugt nicht). Der alte Code vibrierte 1000 ms pro Tap — die Zielgröße für ein „Plop" sind 10–20 ms. |

#### 5.1 Shot-Liste für die Referenzaufnahme (M0.1)

Im Editor aufnehmen, 60 fps, eine Aufnahme von 45–90 s. Es geht um **Timing und Gefühl**, nicht um Bildqualität. Jede Interaktion einmal langsam, einmal in normalem Tempo:

1. Leere Zelle drücken, langsam in **vier verschiedene Richtungen** ziehen — zeigt, dass sich der Fächer an der Zugrichtung ausrichtet und danach stehen bleibt.
2. Auf dem Bogen von 1 bis 9 durchwischen — zeigt Lens-Tracking und Elementabstände.
3. Über den Schwellwert ziehen und **zurück zur Zelle** — der Abbruch-Weg.
4. Schneller Flick auf eine bereits selektierte Zelle (der Fall, den der alte Code verschluckt).
5. Element 0 („X", Löschen) wählen.
6. Bereits selektierte Zelle kurz antippen → Deselect.
7. Feste Zelle (Given) drücken → kein Menü.
8. Beide Lens-Modi (`LensFixed` an/aus, Settings-Toggle im Spiel).
9. Zum Schluss: das Auf- und Zublenden des Menüs mehrfach hintereinander — dafür ist die Aufnahme eigentlich da.

Ablage **außerhalb des Repos** (z. B. `D:\Projects\_reference\`) — ein Video gehört nicht in die Versionierung.

**Die eine Regel, die M0 trägt:** Die Eingabeoberfläche entscheidet **niemals**, ob eine Eingabe zulässig ist. Sie meldet einen `InputIntent`; `PuzzleSession` entscheidet und schickt einen aktualisierten `IBoardDisplay` zurück. Im Prototyp ist das anders gelöst — genau das muss raus, sonst sind Hints, Undo und Fehlerzählung später nicht sauber baubar.

**Abbruchkriterium:** Lässt sich die Eingabe nicht referenzfrei herauslösen, war sie nicht der Teil, den man behalten wollte — dann ist Neuschreiben schneller als Entflechten.

### M1 — Engine (Sudoku)

`UniqueSolver`, `LogicSolver` Tier 0–3, `FeatureExtractor` (37 Features), `Scorer`, Golden-File-Tests. Spec Teil A.

Akzeptanz: > 20.000 Uniqueness-Checks/s single-thread · serate-Vergleich ≥ 90 % innerhalb ±0.2 ER · **0 Fälle**, in denen die eigene Engine löst, `serate` aber > 8.5 ratet · 200 Fixtures grün · kompiliert ohne `UnityEngine`.

> Ab hier ist die Engine ein eigenes .NET-Projekt mit Unit-Tests — unabhängig von Unity entwickelbar und deshalb der Teil mit der besten Iterationsgeschwindigkeit im ganzen Vorhaben.

### M2 — Pipeline

Grid-Generator, Digging mit Unavoidable-Set-Optimierung, Vicinity Search, SQLite, `puzzlegen`-CLI. Spec Teil B/C. Zielmenge ~86.000 Puzzles über 5 Buckets.

### M2b — Content Studio

Unity-Editor-Fenster über der CLI (Kindprozess, nicht in-process — überlebt Domain Reload). Tabs Bestand, Generieren, Prüfen; Step-Log-Replay. Spec Teil I.

> Direkt nach M2, nicht später. Ab dem Moment, wo ein Bestand existiert, ist die Sichtungsoberfläche gleichzeitig das beste Debugging-Instrument für die Engine.

### M3 — App v1

Client, ein Typ, feste Gewichte, Starter-Pack, Hint-System, **vollständige Telemetrie**, Session-Wiederherstellung, Juice-Vollausbau. Spec Teil D.

Hier fällt die Firebase-Entscheidung (G4): Interfaces stehen seit M0, jetzt kommt die echte Implementierung dahinter.

> Der wichtigste Punkt in M3 ist die Telemetrie. Sie ist funktional unsichtbar und wird deshalb gern verschoben. Ohne sie ist die gesamte Kalibrierung (M5) wertlos und die Daten der ersten Monate sind unwiederbringlich verloren.

### M4 — Chains & obere Stufen
### M5 — Kalibrierung & Adaptive Selection
### M6 — Daily Puzzles & zweiter Rätseltyp

Unverändert nach Spec §13.

---

## 6. Querschnittsthemen mit Vorlauf

Diese laufen **parallel** und blockieren keinen Meilenstein — aber sie haben Beschaffungszeit und werden deshalb früh angestoßen.

| Thema | Warum früh |
|---|---|
| **Sounddesign** | Im Projekt existiert **keine einzige Audiodatei**, kein Mixer, kein Bus-Setup. Bei „sehr viel Juice" als Kernziel ist das der größte unsichtbare Posten. Sounddesign ist Beschaffung, nicht Programmierung — Asset-Pack kaufen oder Freelancer beauftragen, beides mit Vorlauf. Ab M0.5 kann der `FeedbackRouter` bereits gegen Platzhalter arbeiten. |
| **Apple Developer Program** | Für iOS-Signierung und Sign-in-with-Apple zwingend, jährlich kostenpflichtig, Freischaltung dauert. Nicht erst in M3 anfangen. |
| **App-Identität** | Bundle-ID, Produktname, Icons, Splash. Der Produktname „Sudoku World" trägt eine Multi-Spiel-Plattform nicht. |
| **Localization** | Package fehlt, Font-Abdeckung fehlt (keine CJK/Kyrillisch-Fallbacks), kein String extrahiert. Je später, desto teurer — ab M3 sollten Strings nicht mehr hartkodiert entstehen. |

---

## 7. Offene Entscheidungen

| # | Frage | Empfehlung | Wann |
|---|---|---|---|
| **D-A** | **Unity-Version.** Installiert ist ausschließlich `6000.5.4f1`. Nach meinem Kenntnisstand ist das kein LTS-Zweig; die Spec verlangt Unity 6 LTS. | In der Hub die aktuelle **6000er-LTS** installieren und das neue Projekt darauf anlegen. Ein Versionswechsel nach M0 ist deutlich teurer. Bitte im Hub gegenprüfen, welche LTS aktuell angeboten wird. | vor M0.2 |
| **D-B** | Repo: neues Repo oder neuer Ordner im bestehenden? | **Neues Repo.** Löst die 399 MB ohne History-Rewrite. Der Bestand bleibt als `prototype-v0` erhalten. | vor M0.2 |
| **D-C** | Feel jetzt kaufen oder erst zu M0.5? | **Vor M0.4.** Wenn Feel/DOTween beim Neubau der Eingabe schon da ist, wird der Code-Tween direkt richtig gebaut statt zweimal. | vor M0.4 |
| **D-D** | Sound: Asset-Pack, Freelancer oder selbst? | Für v1 ein kuratiertes UI-Sound-Pack, später gezielt nachproduzieren. Entscheidung beeinflusst nur den Zeitpunkt, nicht die Architektur. | vor M3 |
| **D-E** | Produktname / Bundle-ID | Wird für Apple-/Google-Konten und Store-Listing gebraucht — sobald festgelegt, ändert er sich nur noch teuer. | vor M0.2 |

---

## 8. Verifikation

**Nach M0 — die Übernahme ist gelungen, wenn:**

- [ ] Die Isolationsszene läuft und `Puzzle.UI` / `Puzzle.Types.Sudoku` referenzieren weder `Puzzle.Engine` noch einen Legacy-Namespace.
- [ ] Die Eingabe meldet ausschließlich `InputIntent` und trifft **keine** Regelentscheidung (Prüfung: nach `SetCellValue`, `IsSolved`, Konfliktprüfung im UI-Code grep-en — es darf keinen Treffer geben).
- [ ] Der Picker funktioniert mit N = 4, 9 und 16 Elementen ohne Code- oder Clip-Änderung. Das ist der Beweis, dass die Animator-Ablösung geglückt ist.
- [ ] Zwei Finger gleichzeitig erzeugen kein Chaos (der alte Code kannte kein `pointerId`).
- [ ] Schnelle Flicks auf eine bereits selektierte Zelle schreiben den Wert (der alte Code verschluckte sie).
- [ ] Seite-an-Seite-Vergleich gegen die Aufnahme aus M0.1 zeigt kein spürbar verändertes Timing.
- [ ] Haptik auf einem echten Android-Gerät geprüft, Impulslänge im 10–20-ms-Bereich.
- [ ] Build für Android **und** iOS läuft durch — mit IL2CPP und ARM64. Nicht auf M3 verschieben; ein IL2CPP-Problem, das man in M0 in einer Stunde findet, kostet in M3 eine Woche.
- [ ] `Engine` und `Domain` kompilieren als reines .NET-Projekt ohne Unity.

**Laufend ab M1:** Golden-File-Tests grün, `serate`-Vergleich in Toleranz, Feature-Drift-Prüfung nach jeder Engine-Änderung.

**Ab M3:** die Performance-Budgets aus Spec §6.8 (Kaltstart < 2.5 s, Frame-Time < 8 ms, GC < 1 KB/s, Hint < 100 ms).
