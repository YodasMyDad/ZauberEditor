# ZauberEditor

`ZauberCMS.RTE`: an extensible rich-text (WYSIWYG) editor component for Blazor, shipped as a NuGet package (MIT).
Targets **.NET 10 / C# 14**. The editing engine is JavaScript driven from C# via JS interop.

This file is the project's **standards document**. The codebase is old and is being audited against it: every file
must end up conforming to the [Standards](#standards) below, and the [Audit findings](#audit-findings) list the known
gaps. Where existing code disagrees with this file, this file wins.

## Layout (current)

- `ZauberCMS.RTE/` — the library (Razor class library, packed to NuGet)
  - `Components/ZauberRichTextEditor.razor` — the editor component (parameters, `[JSInvokable]` callbacks, state refresh)
  - `Components/` — `ToolbarHost`, `ToolbarButton`, `ToolbarDropdown`, `SlideOutPanel`, `Panels/` (Image, Link, Table)
  - `Models/` — public API: `IToolbarItem` + `ToolbarItemBase`, `IEditorApi` (impl: internal `EditorApi` in
    `EditorApiImpl.cs`), `EditorState`, `EditorSettings` (+ `HtmlPolicy`), `ToolbarLayout`, `PanelBase`, event args
  - `Models/ToolbarItems/` — one class per built-in toolbar item; `Models/DefaultToolbarItems.cs` lists them
  - `Services/ToolbarDiscoveryService.cs` — assembly scanning for `IToolbarItem`, plus `ZauberRteOptions` and the
    `AddZauberRte(...)` DI extensions; `ToolbarItemFactory` creates items per scope via `ActivatorUtilities`
  - `Services/IZauberJsRuntime.cs` / `ZauberJsRuntime.cs` — typed C# wrappers over the JS
  - `Examples/` — sample custom items/dropdowns/layouts/panels (currently compiled into the package)
  - `wwwroot/js/zauber-rte.js` — ~3.6k lines, global `window.ZauberRTE` (`selection`, `clipboard`, `image`,
    `mutation`, `panel`, `resize`, `history`, `portal`, `shortcuts`)
  - `wwwroot/css/zauber-editor.css` — all styles incl. dark theme; `wwwroot/css/fontawesome.min.css` — icons
  - `wwwroot/js/lib/purify.min.js` — vendored DOMPurify
- `ZauberRTE.Web/` — Blazor Server demo/test host (`Components/Pages/Home.razor` hosts the editor)
- `README.md` (also the NuGet readme) and `QUICK_START_EXTENDING.md` — consumer docs

## Build & run

```powershell
dotnet build ZauberRTE.sln
dotnet run --project ZauberRTE.Web          # demo app
./build-nuget.ps1                           # Release build + pack into ./nupkgs/
```

## How it works

- **Toolbar items** implement `IToolbarItem` (normally via `ToolbarItemBase`), keyed by `Id` (case-insensitive).
  Discovered from the entry assembly plus assemblies passed to `AddZauberRte(...)`; with `AllowOverrides = true`
  (default) a user item with a built-in's `Id` replaces it. Items can inject scoped services.
- `ExecuteAsync(IEditorApi api)` performs the action; `IsActive`/`IsEnabled` read `EditorState`. `TrackedTags` /
  `PrimaryTag` drive active-mark detection and the canonical tag emitted (bold tracks `strong`,`b`; emits `strong`).
- Items with UI set `PanelComponent` to a `PanelBase` component, rendered in `SlideOutPanel`, which is portalled to
  `document.body` to escape containing blocks.
- C# → JS goes through `ZauberJsRuntime`; JS → C# hits `[JSInvokable]` methods on `ZauberRichTextEditor`.
- HTML is sanitised with DOMPurify according to `EditorSettings.HtmlPolicy`.

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
  any public member missing a doc (CS1591) fails the build. Test and demo projects don't generate docs.
- `Directory.Packages.props` with central package management; no versions in individual csproj files.
- `.editorconfig` at the root encodes the C# style below so the build enforces it.
- The library declares `IsTrimmable` and `IsAotCompatible`; reflection is either removed or annotated
  (`[DynamicallyAccessedMembers]`, `[RequiresUnreferencedCode]`) so trimmed WebAssembly apps keep working.
- No `GeneratePackageOnBuild`; packing is explicit via `build-nuget.ps1`. Remove pack properties made redundant by RCL
  static web assets (`IncludeContentInPack`, `ContentTargetFolders`) and verify the `.nupkg` contents afterwards.
  Produce a symbols package (`snupkg`); SourceLink is on by default in the SDK.
- All assets are self-hosted inside the package. No CDN URLs (Font Awesome webfonts currently load from cdnjs).
- Vendored JS libraries are committed ES-module builds; any npm script must be cross-platform (no `cp`/`mkdir -p`).
- Versioning is semver. The audit is a **major** release (3.0.0): breaking changes are allowed and must be listed in a
  "Migrating to 3.0" section of `README.md`. Bump `<Version>` and name the version in the release commit message.

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
- Split by concern into small modules (`selection`, `clipboard`, `image`, `history`, `mutation`, `panel`, `portal`,
  `shortcuts`) plus a shared `dom` utilities module. No module over a few hundred lines; no duplicated DOM helpers.
- **Per-instance state:** `createEditor(element, dotNetRef, options)` returns an instance (a `class` with `#private`
  fields) that C# holds as an `IJSObjectReference`. Nothing per-editor lives on module-level variables.
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

- All colours, spacing, radii, shadows, fonts and timings are **custom properties** (design tokens) on `.zauber-rte`.
- Themes only redefine tokens: `.rte-theme-dark` sets dark tokens and `Theme.Auto` uses
  `@media (prefers-color-scheme: dark)`. No duplicated per-theme selector blocks.
- Everything inside `@layer zauber-rte` and low-specificity selectors (`:where(...)`) so consumers can override without
  `!important`. **No `!important`.**
- Native CSS nesting, logical properties (`margin-inline`, `inset-block-start`), `prefers-reduced-motion` for panel and
  dropdown animations. Delete unused rules.

### Demo app & tests

- `ZauberRTE.Web` contains only editor demos (no template pages). It has pages covering: the full toolbar, a custom
  toolbar item with a panel, two editors on one page, read-only mode, and light/dark/auto themes.
- Add `ZauberCMS.RTE.Tests` using **xUnit v3** (test framework), **bUnit** (Blazor component testing),
  **Shouldly** (assertions) and **NSubstitute** (mocking), covering discovery/registration, toolbar items' state logic,
  `EditorApi`, and component rendering/disposal. Interactive editing behaviour is verified in the demo app with
  Playwright.

### Definition of done (every change)

1. `dotnet build ZauberRTE.sln` passes with zero warnings; tests pass.
2. No comments in the changed files except `///` docs on public API, and every public member has one.
3. Checked in the demo app: the feature works, in light and dark themes, with two editors on one page.
4. `README.md` / `QUICK_START_EXTENDING.md` updated for any consumer-visible change.

---

## Audit findings

Known gaps against the standards (counts as of 2026-11-25). Work through these in phases, building and verifying after
each.

1. **Comments everywhere** — ~1,460 comment lines in C#/Razor (1,203 of them `///`) and ~480 in `zauber-rte.js`, plus
   CSS/csproj comments. Remove all of them except `///` on public API; trim those to the short consumer-facing form
   and add any that are missing. Remove `///` from internal/private members.
2. **Copy-pasted interop error handling** — every method in `Services/ZauberJsRuntime.cs` repeats try/catch/log
   (~50 copies). Replace with one invocation helper + `[LoggerMessage]` logging.
3. **Global, shared JS state** — `window.ZauberRTE` is a singleton; e.g. `clipboard` stores one `_pasteCallback`
   (`zauber-rte.js:2537`) for the whole page, so multiple editors clash. Rebuild as ES modules with per-instance state.
4. **Leaks** — `DotNetObjectReference.Create(this)` three times in `ZauberRichTextEditor.razor:230-244`, never
   disposed; `Dispose()` (`:635`) doesn't stop the mutation observer (the call is commented out at `:640`). JS adds 20
   event listeners but removes 8. Move to `IAsyncDisposable` + `AbortController`.
5. **Polling** — `Task.Delay(50)` loops in `ZauberRichTextEditor.razor:123, 581, 607`.
6. **Id-based DOM lookup** — 45 `getElementById` calls; pass `ElementReference`s / hold elements on the instance.
7. **`innerHTML`** — 39 uses; each must be sanitised HTML or replaced with DOM APIs.
8. **CSS** — no custom properties, 265 `!important`, dark theme implemented by duplicating selectors.
9. **CDN dependency** — `fontawesome.min.css` loads webfonts from `cdnjs.cloudflare.com`. Self-host (or replace the
   icon font with inline SVG icons if that removes the dependency more simply).
10. **Type organisation** — multi-type files (`EventArgs.cs` 8 types, `IZauberJsRuntime.cs` 7, `ToolbarLayoutItem.cs`
    6, `EditorState.cs` 5, `EditorSettings.cs` 4, `ToolbarDiscoveryService.cs` 4, …); interop interfaces lack the `I`
    prefix; `EditorApi` lives in `EditorApiImpl.cs`.
11. **Examples shipped in the package** — `Examples/` compiles concrete `ToolbarItemBase` subclasses (with `TODO`
    stubs) into the library, where discovery can pick them up. Move them to the demo app or the docs.
12. **Old C# idioms** — `new List<T>()` / `new HashSet<T>()` locals, 43 `== null` / `!= null`, `@"..."` CSS string in
    `CustomDropdownExamples.cs`, unsealed classes, everything public, `@code` blocks of 600+ lines.
13. **Build/packaging** — no `Directory.Build.props`, central package management or `.editorconfig`;
    `GeneratePackageOnBuild` on; legacy pack properties; `package.json` copy script isn't cross-platform; consumers must
    add `<script>` tags manually.
14. **Demo app** — template leftovers (`Counter.razor`, `Weather.razor`), no multi-editor page.
15. **Accessibility** — `ToolbarHost.razor` has no ARIA/roles; the other components have only a few attributes.
16. **No tests.**
