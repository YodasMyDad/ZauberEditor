# ZauberEditor

`ZauberCMS.RTE`: an extensible rich-text (WYSIWYG) editor component for Blazor, shipped as a NuGet package (MIT).
Targets **.NET 10 / C# 14**. The editing engine is JavaScript driven from C# via JS interop.

This file is the project's **standards document**. The 3.0 audit brought every file in line with the
[Standards](#standards) below (see [Audit status](#audit-status)); every change since must keep it that way. Where
code disagrees with this file, this file wins.

## Layout (current)

Namespace = folder path. Public consumer namespaces are `Components`, `Models` and `Services`; everything else is
internal.

- `ZauberCMS.RTE/` — the library (Razor class library, packed to NuGet)
  - `Components/ZauberRichTextEditor.razor(.cs)` — the editor component: parameters, lifecycle, content sync, panel
    and toast hosting. Orchestrates; the work lives in `Internal/`, `Interop/` and `Toolbar/`.
  - `Components/Internal/` — implementation details: `EditorToolbar`, `ToolbarButton`, `ToolbarDropdown`,
    `SlideOutPanel`, `ToastRegion`, the built-in `LinkPanel` / `ImagePanel` / `TablePanel` (Razor, see the
    [component exception](#blazor-components)), plus internal `EditorApi` (the `IEditorApi` implementation),
    `ContentSync` (echo tracking), `PanelRequest`, `ToolbarPresenter`, `UrlRules`, `EditorLog`
  - `Models/` — public API, one type per file: `IToolbarItem`, `ToolbarItemBase`, `ToolbarItemKind`, `ToolbarItemIds`,
    `IEditorApi`, `EditorState`, `SelectionInfo` / `LinkInfo` / `ImageInfo`, `EditorSettings`, `EditorDimensions`,
    `ImageConstraints`, `HtmlPolicy`, `ToolbarLayout` + `ToolbarLayoutEntry` / `ToolbarGroup` / `ToolbarItemRef` /
    `ToolbarSeparator` (+ internal source-generated `ToolbarLayoutJsonContext`), event args (`EditorChangeArgs`,
    `ChangeSource`, `CommandExecutedArgs`, `ImageResizedArgs`), `PanelBase`, `HtmlBuilder`, `Theme`, `TextAlignment`,
    `ToastType`
  - `Services/` — public `ZauberRteServiceCollectionExtensions` (`AddZauberRte`) and `ZauberRteOptions`; internal
    `ToolbarRegistry` (singleton, replays registrations), `ToolbarItemRegistration`, `ToolbarItemSet` (per-editor
    frozen item set), `ToolbarArrangement` / `ToolbarSegment` / `ToolbarSlot` (layout resolved against the item set)
  - `Toolbar/` — internal built-in items (`MarkItem`, `BlockItem`, `CodeItem`, `AlignItem`, `HeadingsDropdownItem`,
    `LinkItem`, `UnlinkItem`, `ImageItem`, `TableItem`, `ClearFormattingItem`, `UndoItem`, `RedoItem`,
    `ViewSourceItem`, `ThemeToggleItem`), `BuiltInToolbarItems` (the list), `Icons` (inline SVG), `KeyboardShortcut`
  - `Interop/` — internal C# ↔ JS boundary: `IEditorInteropFactory` / `EditorInteropFactory` (imports the module once
    per scope), `IEditorInterop` / `EditorInterop` (typed calls on the per-editor `IJSObjectReference`, one guarded
    invocation helper), `EditorCallbacks` (`[JSInvokable]` bridge → `IEditorEventSink`), `EditorInitOptions`,
    `EditorSnapshot`, `HtmlPolicyOptions`, `ImageOptions`, `ShortcutBinding`, `InteropLog`
  - `wwwroot/js/` — ~35 flat ES modules (each a few hundred lines at most): entry `zauber-rte.js` (`createEditor`),
    `editor.js` (the `Editor` class), and `selection`, `ranges`, `marks`, `blocks`, `lists`, `tables`, `insert`,
    `clipboard`, `office`, `sanitize`, `history`, `changes`, `state`, `keyboard`, `keys`, `shortcuts`, `links`,
    `images`, `resize`, `drag`, `panel`, `source-view`, `serialize`, `scheduler`, `channel`, `dom`, ...
  - `wwwroot/js/lib/purify.es.mjs` (+ `.map`, `purify.LICENSE`) — vendored DOMPurify ES build, refreshed with
    `npm run build` in `ZauberCMS.RTE/`
  - `wwwroot/css/zauber-editor.css` — all styles: tokens, themes, components, in `@layer zauber-rte`
- `ZauberCMS.RTE.Tests/` — xUnit v3 + bUnit + Shouldly + NSubstitute; folders mirror the library (`Components`,
  `Interop`, `Models`, `Services`, `Toolbar`) plus `TestSupport/`
- `ZauberRTE.Web/` — demo host (Interactive Server + WebAssembly): `Components/Pages/` (one page per scenario),
  `Toolbar/` (sample custom items, dropdown and panel, registered in `Program.cs`), `Components/Demo/ArticleForm`
- `ZauberRTE.Web.Client/` — the demo's WebAssembly client: `/webassembly` and `/auto` pages, its own `AddZauberRte()`
- `Directory.Build.props`, `Directory.Packages.props`, `.editorconfig` — shared build settings, central package
  versions, enforced code style
- `README.md` (also the NuGet readme, incl. "Migrating to 3.0"), `QUICK_START_EXTENDING.md`, `CHANGELOG.md`,
  `LICENSE` — consumer docs

## Build & run

```powershell
dotnet build ZauberRTE.sln                  # zero warnings (warnings are errors)
dotnet test --solution ZauberRTE.sln        # library tests
dotnet run --project ZauberRTE.Web          # demo app (Server, WebAssembly and Auto pages)
./build-nuget.ps1                           # Release build + tests + pack (.nupkg + .snupkg) into ./nupkgs/
```

**Releases are automatic.** `.github/workflows/ci.yml` builds and tests every push and pull request to `master`. On a
push to `master` it publishes `ZauberCMS.RTE` to NuGet.org via Trusted Publishing (`NuGet/login`, OIDC; no API key —
needs the nuget.org policy for this repo + `ci.yml` and the `NUGET_USER` secret), tags `vX.Y.Z` and commits the
stamped `<Version>` back with `[skip ci]`. Version rule: if no tag exists for the csproj `<Version>`, that exact version
is published; otherwise the patch of the highest version is bumped. So for a minor/major release, set `<Version>` in
`ZauberCMS.RTE.csproj` (e.g. `3.1.0`) before pushing; for a patch release, just push.

## How it works

- **Registration.** `AddZauberRte(options => ...)` is idempotent (`TryAdd*`, options pattern). Built-in items are
  registered first, then the consumer's registrations replay in order: `AddToolbarItem<T>()` (factory, trim-safe),
  `AddToolbarItem(instance)` (shared, stateless), `RemoveToolbarItem(id)`, and opt-in
  `AddToolbarItemsFromAssembly(...)` (`[RequiresUnreferencedCode]`). A later registration with the same `Id`
  (case-insensitive) replaces an earlier one. `ToolbarRegistry` (singleton) builds a `ToolbarItemSet` **per editor**:
  factory items are created with `ActivatorUtilities` from the scoped provider (so they can inject scoped services) and
  disposed with the editor.
- **Items.** `ExecuteAsync(IEditorApi api, CancellationToken cancellationToken)` performs the action; `IsActive`/`IsEnabled` read the immutable
  `EditorState`. `TrackedTags` / `PrimaryTag` drive active-mark detection and the canonical tag emitted (bold tracks
  `strong`,`b`; emits `strong`); the alias map and shortcuts are computed from the items in the editor's layout and
  sent to JS once. Dropdowns list `ChildItemIds`, resolved through the item set. An item failure is caught once in the
  component (logged + error toast).
- **Panels.** Items with UI call `api.OpenPanelAsync<TPanel>(parameters)` (`TPanel : PanelBase`, abstract `Title`).
  The panel renders in `SlideOutPanel` inside an `ErrorBoundary`; JS `openPanel` portals the overlay to
  `document.body`, traps focus and reports Escape; `closePanel` moves it back and restores the editor selection.
- **JS.** ES modules only. The scoped `EditorInteropFactory` imports `zauber-rte.js` once; the component calls
  `createEditor(root, content, source, toolbar, dotNetRef, options)` in `OnAfterRenderAsync(firstRender)` with
  `ElementReference`s and holds the returned per-instance `Editor` as an `IJSObjectReference`. JS owns the content
  DOM (Blazor renders no children into `.rte-content`). JS → .NET goes through one `DotNetObjectReference<EditorCallbacks>`
  per editor: `OnContentChanged`, `OnStateChanged` (rAF-coalesced, only when changed), `OnShortcut`,
  `OnFocusChanged`, `OnImageResized`, `OnPanelEscape`, `OnError` (replaces `console.*`). `DisposeAsync` calls
  `dispose()` (one `AbortController.abort()`, observers disconnected) and releases every reference.
- **Content sync.** JS debounces content notifications (`ChangeDelay`) and flushes them on blur, before commands and
  after paste/undo/redo/source edits; C# never reads HTML back after a notification. `ContentSync` keeps the HTML the
  DOM holds plus a bounded queue of values emitted through `ValueChanged` that the parent hasn't echoed back yet; a
  `Value` parameter that matches a pending echo (or the known HTML) is ignored, anything else is pushed to JS as
  `setHtml(html, 'parameter')` (sanitised, history reset, no notification). `EditContext` is notified when
  `ValueExpression` is set.
- **Large content.** Notifications carry the HTML inline up to 12,000 UTF-8 bytes; above that they carry `null` and
  C# pulls `getHtmlStream()` as an `IJSStreamReference`, so the default 32 KB SignalR limit is enough. Content over
  `EditorSettings.MaxContentBytes` is not transferred (logged).
- **Sanitising.** Every HTML entry point (parameter, `SetHtmlAsync`, `InsertHtmlAsync`, paste, drop, source view) is
  sanitised with a per-editor DOMPurify instance configured from `EditorSettings.HtmlPolicy`, with a URL scheme
  allow-list and a fixed deny-list of dangerous tags. `HtmlBuilder` encodes text and attribute values.

---

## Standards

### Universal rules

- **No comments in code**, with one exception. No `//`, `/* */`, `<!-- -->`, `@* *@`, or CSS comments, and no
  commented-out code — in C#, Razor, JS, CSS, csproj, props and scripts. Code explains itself through names, small
  methods and types. Anything that genuinely needs explaining goes in this file.
- **The exception: `///` XML docs on public API only** (public types and their public/protected members), because they
  ship in the NuGet package as IntelliSense. One or two lines (`<summary>`, plus `<param>`/`<returns>` only when not
  obvious) stating what it does for the consumer — not how it works. Never on `internal`/`private` members.
- **Don't repeat yourself.** Any pattern that appears twice becomes a shared helper, base type, extension, CSS token or
  JS module. No copy-pasted blocks that differ only by a string.
- **Small and simple.** Short methods, one responsibility per type, no speculative abstractions, no dead code, no unused
  members, parameters, usings, files or CSS rules. Delete rather than keep "just in case".
- **Correct lifetimes.** Everything that subscribes, observes, allocates a reference or registers a listener has a
  matching teardown.
- **Multi-instance safe.** Any number of editors must work on one page with no shared mutable state between them.
- **Render-mode agnostic.** The library must work in Interactive Server, WebAssembly and Auto; no assumptions about
  prerendering or a persistent circuit.
- **Zero warnings.** The solution builds with warnings as errors.

### Project & build

- `Directory.Build.props` at the root holds shared settings: `TargetFramework net10.0`, `LangVersion latest`,
  `Nullable enable`, `ImplicitUsings enable`, `TreatWarningsAsErrors true`, `AnalysisLevel latest-recommended`,
  `EnforceCodeStyleInBuild true`.
- The library sets `GenerateDocumentationFile true`, so the XML docs ship in the package and, with warnings as errors,
  any public member missing a doc (CS1591) fails the build. **Decided exception:** the test and demo projects also set
  `GenerateDocumentationFile true`, but with `<NoWarn>$(NoWarn);CS1591</NoWarn>`, because the build only enforces
  IDE0005 (unused usings) when documentation generation is on. They carry no `///` docs.
- `Directory.Packages.props` with central package management; no versions in individual csproj files.
- `.editorconfig` at the root encodes the C# style below so the build enforces it.
- The library declares `IsTrimmable` and `IsAotCompatible`; reflection is either removed or annotated
  (`[DynamicallyAccessedMembers]`, `[RequiresUnreferencedCode]`) so trimmed WebAssembly apps keep working.
- No `GeneratePackageOnBuild`; packing is explicit via `build-nuget.ps1`. Remove pack properties made redundant by RCL
  static web assets (`IncludeContentInPack`, `ContentTargetFolders`) and verify the `.nupkg` contents afterwards.
  Produce a symbols package (`snupkg`); SourceLink is on by default in the SDK.
- All assets are self-hosted inside the package. No CDN URLs. Built-in icons are inline SVG (`Toolbar/Icons.cs`);
  no icon font ships.
- Vendored JS libraries are committed ES-module builds; any npm script must be cross-platform (no `cp`/`mkdir -p`).
- Versioning is semver. Breaking changes need a major version and a "Migrating to X.0" section in `README.md` (3.0.0
  was the audit release). Every release gets a `CHANGELOG.md` entry (Keep a Changelog). For minor/major releases set
  `<Version>` and name the version in the commit message; CI publishes on push to `master` (see Build & run).

### C# (C# 14 / .NET 10)

- File-scoped namespaces matching the folder path. **One top-level type per file**, file named after the type.
- `sealed` by default; `internal` for anything that is not deliberate public API. Keep the public surface minimal.
- Primary constructors for DI-injected classes. Models use `required` / `init` properties; immutable data
  (selection/link/image info, event args, layout entries) are `sealed record`s.
- Interfaces are `I`-prefixed; implementations drop the prefix (`ISelectionInterop` → `SelectionInterop`, not `*Impl`).
- Collection expressions (`[]`, `[..a, ..b]`) for all collection creation. `FrozenDictionary` / `FrozenSet` for
  lookup tables built once (e.g. the toolbar registry after discovery).
- Pattern matching and switch expressions over `if` chains: `is null`, `is not null`, `is "a" or "b"`, property
  patterns. No `== null` / `!= null`.
- Use C# 14 features where they simplify: `field` keyword for properties needing a backing field, `extension` blocks
  for extension members, null-conditional assignment (`a?.B = c`), raw string literals for embedded HTML/CSS.
- Expression-bodied members for single expressions; `var` for locals; `_camelCase` private fields; PascalCase
  constants and static readonly fields.
- Guard public API inputs with `ArgumentNullException.ThrowIfNull` / `ArgumentException.ThrowIfNullOrWhiteSpace`.
- **Async:** `Async` suffix; never `.Result`, `.Wait()`, `async void` (except Blazor event handlers) or `Task.Delay`
  polling — await a JS promise or a `TaskCompletionSource` instead. Accept a `CancellationToken` on public async APIs
  that can take time.
- **Errors:** no blanket `catch (Exception)` + log + swallow per method. JS interop failures are handled in **one**
  shared invocation helper. Catch only what you can act on (`JSDisconnectedException` on teardown, `JSException`
  where a fallback exists). Use exceptions for exceptional cases only.
- **Logging:** source-generated `[LoggerMessage]` methods in one `partial` log class per area; no string-built logs.
- **DI:** `AddZauberRte` uses `TryAdd*` so it is idempotent, and the options pattern (`IOptions<ZauberRteOptions>`)
  for configuration. Lifetimes: registry singleton, anything touching JS scoped.
- Prefer explicit registration (`options.AddToolbarItem<T>()`) over assembly scanning; scanning, if kept, is an opt-in
  convenience and trim-annotated.

### Blazor components

- `.razor` files hold markup only; logic lives in a `partial class` in a collocated `.razor.cs`.
- **Decided exception to `internal`:** Razor generates a `public partial class` for every component, and an
  `internal` code-behind partial fails with CS0262. Components that are implementation details (toolbar, dropdown,
  slide-out, toasts, built-in panels) are therefore `public sealed partial` in `ZauberCMS.RTE.Components.Internal`,
  marked `[EditorBrowsable(EditorBrowsableState.Never)]`, with a one-line `///` saying they're not for direct use. Their
  parameters may only use public types.
- Components that use JS implement `IAsyncDisposable`: dispose every `DotNetObjectReference`, call the JS module's
  `dispose`, dispose the `IJSObjectReference`, and swallow `JSDisconnectedException` **only** there.
- JS interop only in `OnAfterRenderAsync` (first render) or later. Pass `ElementReference`s to JS, never ids for
  `getElementById`.
- `[EditorRequired]` on required parameters; `EventCallback` for outputs; never mutate a `[Parameter]` from inside.
- Call `StateHasChanged` only when a render would not otherwise happen; use `@key` on rendered lists; override
  `ShouldRender` where a component re-renders without changes.
- Large components are split into child components; the editor component orchestrates, it does not do everything.
- **Accessibility:** toolbar `role="toolbar"` with roving `tabindex` and arrow-key navigation; toggles use
  `aria-pressed`; icon-only buttons have `aria-label`; panels are `role="dialog"` with `aria-modal`, focus trap and
  Escape to close; dropdowns use `aria-expanded` / `aria-haspopup`.

### JavaScript

- **ES modules only** (ES2022+). No `window.*` globals. Each component loads what it needs through
  `IJSObjectReference` (`import('./_content/ZauberCMS.RTE/js/...')`), so consumers add **no** `<script>` tags.
  DOMPurify is imported as an ES module too.
- Split by concern into small modules (`selection`, `clipboard`, `images`, `history`, `panel`, `shortcuts`, ...)
  plus a shared `dom` utilities module. No module over a few hundred lines; no duplicated DOM helpers.
- **Per-instance state:** `createEditor(root, content, source, toolbar, dotNetRef, options)` returns an `Editor`
  instance (a `class` with `#private` fields) that C# holds as an `IJSObjectReference`. Nothing per-editor lives on
  module-level variables, including DOMPurify instances.
- Every instance has `dispose()`. All listeners are registered with one `AbortController` signal
  (`addEventListener(type, fn, { signal })`) and removed with a single `abort()`; observers are `disconnect()`ed.
- `const` by default, `let` when reassigned, never `var`. Arrow functions for callbacks, optional chaining,
  nullish coalescing, `===` only, `async`/`await` over promise chains.
- DOM: build with `createElement`, `textContent`, `append`, `replaceChildren`. `innerHTML` only for HTML that has just
  been passed through DOMPurify with the active `HtmlPolicy`. Use the Selection/Range APIs; never
  `document.execCommand`.
- No `console.*` in shipped code; surface failures to .NET through the instance's callbacks.
- Style: 4-space indent, semicolons, single quotes, named exports.

### CSS

- All colours, spacing, radii, shadows, fonts and timings are **custom properties** (design tokens), declared on
  `:where(.zauber-rte, .zauber-rte-portal, .rte-slideout-overlay)` so portalled panels get them too.
- **Themes use `color-scheme`, not duplicated tokens (decided).** Every colour token is one `light-dark(light, dark)`
  value; `.rte-theme-light` / `.rte-theme-dark` / `.rte-theme-auto` only set `color-scheme: light` / `dark` /
  `light dark`. No per-theme selector blocks and no `prefers-color-scheme` media query. This sets the browser baseline
  (Chrome/Edge 123+, Firefox 120+, Safari 17.5+), which the README states.
- Everything inside `@layer zauber-rte` and low-specificity selectors (`:where(...)`) so consumers can override without
  `!important`. **No `!important`.**
- **Decided exception:** unlayered host resets (Bootstrap Reboot, Tailwind Preflight) beat any layered rule, so the
  stylesheet ends with **one** small unlayered block, scoped to `:is(.zauber-rte, .zauber-rte-portal)` with class
  specificity and no `!important`. It re-asserts only the properties those resets set on `button`, `input`, `select`,
  `textarea`, `label`, headings, lists, links and table cells inside the editor. Nothing else goes there.
- Native CSS nesting, logical properties (`margin-inline`, `inset-block-start`), `prefers-reduced-motion` for panel and
  dropdown animations. Delete unused rules.

### Demo app & tests

- `ZauberRTE.Web` (+ `ZauberRTE.Web.Client`) contains only editor demos (no template pages). It has pages covering:
  the full toolbar with `EditForm`, custom toolbar items with a panel, multiple editors on one page, read-only mode,
  light/dark/auto themes, large content, and the WebAssembly and Auto render modes. Docs samples are copied from it.
- `ZauberCMS.RTE.Tests` uses **xUnit v3** (test framework), **bUnit** (Blazor component testing), **Shouldly**
  (assertions) and **NSubstitute** (mocking), covering registration, toolbar items' state logic, `EditorApi`, content
  sync, interop and component rendering/disposal. Interactive editing behaviour is verified in the demo app with
  Playwright.

### Definition of done (every change)

1. `dotnet build ZauberRTE.sln` passes with zero warnings; tests pass.
2. No comments in the changed files except `///` docs on public API, and every public member has one.
3. Checked in the demo app: the feature works, in light and dark themes, with two editors on one page.
4. `README.md` / `QUICK_START_EXTENDING.md` updated for any consumer-visible change, and a `CHANGELOG.md` entry added.

---

## Audit status

The 2026-11-25 audit (release 3.0.0) resolved all sixteen findings recorded against 2.x:

1. **Comments:** removed everywhere; only short `///` docs on public API remain, enforced by CS1591.
2. **Copy-pasted interop error handling:** one guarded invocation helper in `EditorInterop`, plus `[LoggerMessage]`
   log classes.
3. **Global JS state:** replaced by ES modules with a per-instance `Editor` from `createEditor`.
4. **Leaks:** one `DotNetObjectReference` per editor, `IAsyncDisposable`, and an `AbortController` for every listener.
5. **Polling:** `Task.Delay` replaced by awaited JS promises and `TaskCompletionSource`.
6. **Id-based DOM lookup:** replaced by `ElementReference`s and elements held on the instance.
7. **`innerHTML`:** assigned only DOMPurify output; everything else uses DOM APIs.
8. **CSS:** rebuilt on tokens with `light-dark()`, `@layer zauber-rte` and `:where()`, with no `!important`.
9. **CDN dependency:** Font Awesome removed, and built-in icons are inline SVG.
10. **Type organisation:** one type per file, `I`-prefixed interfaces, and `EditorApi` in `EditorApi.cs`.
11. **Examples in the package:** moved to `ZauberRTE.Web/Toolbar/` and the docs.
12. **Old C# idioms:** collection expressions, patterns, sealed types, a minimal public surface, and split components.
13. **Build and packaging:** `Directory.Build.props`, central package management and `.editorconfig` added; explicit
    pack with `snupkg`; cross-platform npm script; no consumer `<script>` tags.
14. **Demo app:** template pages removed, and scenario pages added, including multiple editors, WebAssembly and Auto.
15. **Accessibility:** toolbar, menu and dialog ARIA patterns, roving tabindex, and a focus trap.
16. **Tests:** `ZauberCMS.RTE.Tests` added.
