using ZauberCMS.RTE.Components.Internal;

namespace ZauberCMS.RTE.Tests.Components.Internal;

public sealed class ContentSyncTests
{
    [Fact]
    public void InitialValueIsKnownAndNotWrittenAgain()
    {
        var sync = new ContentSync("<p>a</p>");

        sync.KnownHtml.ShouldBe("<p>a</p>");
        sync.Revision.ShouldBe(0);
        sync.ParameterChanged("<p>a</p>").ShouldBeFalse();
    }

    [Fact]
    public void EchoOfAValueStillBeingReportedIsNotWritten()
    {
        var sync = new ContentSync("<p>a</p>");
        Change(sync, "<p>ab</p>").ShouldBeTrue();

        sync.BeginEcho("<p>ab</p>");

        sync.ParameterChanged("<p>ab</p>").ShouldBeFalse();
        sync.KnownHtml.ShouldBe("<p>ab</p>");
        sync.Revision.ShouldBe(0);
    }

    [Fact]
    public void StaleEchoArrivingWhileANewerValueIsReportedIsNotWritten()
    {
        var sync = new ContentSync(string.Empty);
        Change(sync, "abc");
        sync.BeginEcho("abc");
        Change(sync, "abcd");
        sync.BeginEcho("abcd");

        sync.ParameterChanged("abc").ShouldBeFalse();
        sync.ParameterChanged("abcd").ShouldBeFalse();
        sync.KnownHtml.ShouldBe("abcd");
    }

    [Fact]
    public void AcknowledgedValueSetAgainByTheParentIsWritten()
    {
        var sync = new ContentSync("<p>original</p>");
        Change(sync, "<p>edited</p>");
        sync.BeginEcho("<p>edited</p>");
        sync.ParameterChanged("<p>edited</p>").ShouldBeFalse();
        sync.EndEcho("<p>edited</p>");

        sync.ParameterChanged("<p>original</p>").ShouldBeTrue();
        sync.KnownHtml.ShouldBe("<p>original</p>");
    }

    [Fact]
    public void OneWayBindingRestoringAnEarlierReportedValueIsWritten()
    {
        var sync = new ContentSync("<p>start</p>");
        foreach (var html in new[] { "A", "B", "C" })
        {
            Change(sync, html);
            sync.BeginEcho(html);
            sync.EndEcho(html);
        }

        sync.ParameterChanged("A").ShouldBeTrue();
        sync.KnownHtml.ShouldBe("A");
    }

    [Fact]
    public void GenuineParentChangeIsWrittenAndStartsANewRevision()
    {
        var sync = new ContentSync("<p>a</p>");
        Change(sync, "<p>typed</p>");

        sync.ParameterChanged("<p>from parent</p>").ShouldBeTrue();

        sync.KnownHtml.ShouldBe("<p>from parent</p>");
        sync.Revision.ShouldBe(1);
    }

    [Fact]
    public void NotificationSentBeforeAParameterPushIsDropped()
    {
        var sync = new ContentSync("<p>a</p>");
        sync.ParameterChanged("<p>pushed</p>");

        sync.Receive(0).ShouldBeNull();

        sync.KnownHtml.ShouldBe("<p>pushed</p>");
    }

    [Fact]
    public void NotificationSentAfterAParameterPushIsAccepted()
    {
        var sync = new ContentSync("<p>a</p>");
        sync.ParameterChanged("<p>pushed</p>");

        var notification = sync.Receive(1).ShouldNotBeNull();

        sync.Accept(notification, "<p>pushed and typed</p>").ShouldBeTrue();
        sync.KnownHtml.ShouldBe("<p>pushed and typed</p>");
    }

    [Fact]
    public void PulledContentIsDroppedWhenTheParentPushedMeanwhile()
    {
        var sync = new ContentSync("<p>a</p>");
        var pulling = sync.Receive(0).ShouldNotBeNull();

        sync.ParameterChanged("<p>pushed</p>");

        sync.Accept(pulling, "<p>old large content</p>").ShouldBeFalse();
        sync.KnownHtml.ShouldBe("<p>pushed</p>");
    }

    [Fact]
    public void PulledContentIsDroppedWhenANewerNotificationArrivedMeanwhile()
    {
        var sync = new ContentSync("<p>a</p>");
        var older = sync.Receive(0).ShouldNotBeNull();
        var newer = sync.Receive(0).ShouldNotBeNull();

        sync.Accept(newer, "<p>newer</p>").ShouldBeTrue();
        sync.Accept(older, "<p>older</p>").ShouldBeFalse();

        sync.KnownHtml.ShouldBe("<p>newer</p>");
    }

    [Fact]
    public void ParameterEqualToTheDomContentIsNotWritten()
    {
        var sync = new ContentSync("<p>a</p>");
        Change(sync, "<p>b</p>");

        sync.ParameterChanged("<p>b</p>").ShouldBeFalse();
        sync.Revision.ShouldBe(0);
    }

    [Fact]
    public void UnchangedParameterDoesNotOverwriteUnboundEdits()
    {
        var sync = new ContentSync("<p>a</p>");
        Change(sync, "<p>b</p>");

        sync.ParameterChanged("<p>a</p>").ShouldBeFalse();
        sync.KnownHtml.ShouldBe("<p>b</p>");
    }

    [Fact]
    public void ContentEqualToKnownHtmlIsNotReported()
    {
        var sync = new ContentSync("<p>a</p>");

        Change(sync, "<p>a</p>").ShouldBeFalse();
        sync.ParameterChanged("<p>b</p>").ShouldBeTrue();
    }

    [Fact]
    public void NullInitialValueIsKnownAsNull()
    {
        var sync = new ContentSync(null);

        sync.KnownHtml.ShouldBeNull();
        sync.ParameterChanged(null).ShouldBeFalse();
    }

    [Fact]
    public void NullParameterAfterContentClearsTheEditor()
    {
        var sync = new ContentSync(null);
        Change(sync, "<p>a</p>");
        sync.ParameterChanged("<p>a</p>").ShouldBeFalse();

        sync.ParameterChanged(null).ShouldBeTrue();
        sync.KnownHtml.ShouldBeNull();
    }

    private static bool Change(ContentSync sync, string html) =>
        sync.Receive(sync.Revision) is { } notification && sync.Accept(notification, html);
}
