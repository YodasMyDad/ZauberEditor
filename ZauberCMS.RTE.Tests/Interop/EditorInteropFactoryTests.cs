using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;
using ZauberCMS.RTE.Interop;
using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Tests.Interop;

public sealed class EditorInteropFactoryTests : IAsyncDisposable
{
    private static readonly EditorInitOptions Options = EditorInitOptions.Create("<p>x</p>", false, null, new EditorSettings(), []);

    private readonly IJSRuntime _js = Substitute.For<IJSRuntime>();
    private readonly IJSObjectReference _module = Substitute.For<IJSObjectReference>();
    private readonly IJSObjectReference _instance = Substitute.For<IJSObjectReference>();
    private readonly DotNetObjectReference<EditorCallbacks> _callbacks = DotNetObjectReference.Create(new EditorCallbacks(Substitute.For<IEditorEventSink>()));
    private readonly CancellationTokenSource _cancellation = new();
    private readonly EditorInteropFactory _factory;

    public EditorInteropFactoryTests()
    {
        _js.InvokeAsync<IJSObjectReference>(Arg.Any<string>(), Arg.Any<CancellationToken>(), Arg.Any<object?[]?>()).Returns(new ValueTask<IJSObjectReference>(_module));
        _module.InvokeAsync<IJSObjectReference>(Arg.Any<string>(), Arg.Any<CancellationToken>(), Arg.Any<object?[]?>()).Returns(new ValueTask<IJSObjectReference>(_instance));
        _factory = new(_js, NullLogger<EditorInterop>.Instance);
    }

    public async ValueTask DisposeAsync()
    {
        await _factory.DisposeAsync();
        _callbacks.Dispose();
        _cancellation.Dispose();
    }

    [Fact]
    public async Task ImportsTheModuleAndCreatesAnInstanceForTheEditorElements()
    {
        ElementReference[] elements = [new("root"), new("content"), new("source"), new("toolbar")];

        var interop = await CreateAsync(elements);

        interop.ShouldNotBeNull();
        var import = _js.ReceivedCalls().ShouldHaveSingleItem().GetArguments();
        import[0].ShouldBe("import");
        import[2].ShouldBeOfType<object?[]>().ShouldBe([EditorInteropFactory.ModulePath]);
        var create = _module.ReceivedCalls().ShouldHaveSingleItem().GetArguments();
        create[0].ShouldBe("createEditor");
        create[1].ShouldBe(CancellationToken.None);
        create[2].ShouldBeOfType<object?[]>().ShouldBe([.. elements.Cast<object?>(), _callbacks, Options]);
    }

    [Fact]
    public void ModuleIsAScriptThatShipsInTheLibrarysStaticAssets()
    {
        const string ContentRoot = "./_content/ZauberCMS.RTE/";
        var repository = new DirectoryInfo(AppContext.BaseDirectory);
        while (repository is not null && !File.Exists(Path.Combine(repository.FullName, "ZauberRTE.sln")))
        {
            repository = repository.Parent;
        }

        EditorInteropFactory.ModulePath.ShouldStartWith(ContentRoot);
        var asset = Path.Combine(repository.ShouldNotBeNull().FullName, "ZauberCMS.RTE", "wwwroot", EditorInteropFactory.ModulePath[ContentRoot.Length..]);
        File.ReadAllText(asset).ShouldContain("export function createEditor(");
    }

    [Fact]
    public async Task TheModuleIsImportedOncePerScope()
    {
        await CreateAsync();
        await CreateAsync();

        _js.ReceivedCalls().Count().ShouldBe(1);
        _module.ReceivedCalls().Count().ShouldBe(2);
    }

    [Fact]
    public async Task TheReturnedInteropDrivesTheCreatedInstance()
    {
        var interop = (await CreateAsync()).ShouldNotBeNull();

        await interop.UndoAsync(_cancellation.Token);

        _instance.ReceivedCalls().ShouldHaveSingleItem().GetArguments()[0].ShouldBe("undo");
    }

    [Fact]
    public async Task DisconnectedImportCreatesNothing()
    {
        _js.InvokeAsync<IJSObjectReference>(Arg.Any<string>(), Arg.Any<CancellationToken>(), Arg.Any<object?[]?>())
            .Returns(ValueTask.FromException<IJSObjectReference>(new JSDisconnectedException("Circuit gone.")));

        (await CreateAsync()).ShouldBeNull();
        _module.ReceivedCalls().ShouldBeEmpty();
    }

    [Fact]
    public async Task FailedImportIsRetriedByTheNextEditor()
    {
        _js.InvokeAsync<IJSObjectReference>(Arg.Any<string>(), Arg.Any<CancellationToken>(), Arg.Any<object?[]?>())
            .Returns(
                ValueTask.FromException<IJSObjectReference>(new TaskCanceledException("Timed out.")),
                new ValueTask<IJSObjectReference>(_module));

        await Should.ThrowAsync<TaskCanceledException>(() => CreateAsync());

        (await CreateAsync()).ShouldNotBeNull();
        _js.ReceivedCalls().Count().ShouldBe(2);
    }

    [Fact]
    public async Task DisconnectedImportIsRetriedByTheNextEditor()
    {
        _js.InvokeAsync<IJSObjectReference>(Arg.Any<string>(), Arg.Any<CancellationToken>(), Arg.Any<object?[]?>())
            .Returns(
                ValueTask.FromException<IJSObjectReference>(new JSDisconnectedException("Circuit gone.")),
                new ValueTask<IJSObjectReference>(_module));

        (await CreateAsync()).ShouldBeNull();

        (await CreateAsync()).ShouldNotBeNull();
    }

    [Fact]
    public async Task DisconnectedCreationCreatesNothing()
    {
        _module.InvokeAsync<IJSObjectReference>(Arg.Any<string>(), Arg.Any<CancellationToken>(), Arg.Any<object?[]?>())
            .Returns(ValueTask.FromException<IJSObjectReference>(new JSDisconnectedException("Circuit gone.")));

        (await CreateAsync()).ShouldBeNull();
    }

    [Fact]
    public async Task DisposeReleasesTheImportedModule()
    {
        await CreateAsync();

        await _factory.DisposeAsync();

        await _module.Received(1).DisposeAsync();
    }

    [Fact]
    public async Task DisposeWithoutImportDoesNothing()
    {
        await _factory.DisposeAsync();

        _module.ReceivedCalls().ShouldBeEmpty();
    }

    [Fact]
    public async Task DisposeIgnoresADisconnectedCircuit()
    {
        _module.DisposeAsync().Returns(ValueTask.FromException(new JSDisconnectedException("Circuit gone.")));
        await CreateAsync();

        await Should.NotThrowAsync(_factory.DisposeAsync().AsTask());
    }

    private Task<IEditorInterop?> CreateAsync(ElementReference[]? elements = null)
    {
        var (root, content, source, toolbar) = elements is [var a, var b, var c, var d] ? (a, b, c, d) : (default, default, default, default);
        return _factory.CreateAsync(root, content, source, toolbar, _callbacks, Options);
    }
}
