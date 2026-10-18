using Novolis.Http.Client;

namespace Novolis.Http.Unit;

public sealed class HttpClientKeyTests
{
    [Test]
    public async Task GetClientName_returns_the_derived_type_full_name()
    {
        var name = new GitApi().GetClientName();

        await Assert.That(name).IsEqualTo(typeof(GitApi).FullName);
        await Assert.That(HttpClientKey.For<GitApi>()).IsEqualTo(typeof(GitApi).FullName);
        await Assert.That(name).IsNotEqualTo(nameof(GitApi));
    }

    [Test]
    public async Task GetClientName_is_stable_across_instances()
    {
        var first = new GitApi().GetClientName();
        var second = new GitApi().GetClientName();
        var other = new BillingApi().GetClientName();

        await Assert.That(first).IsEqualTo(second);
        await Assert.That(first).IsNotEqualTo(other);
    }

    [Test]
    public async Task GetClientName_separates_the_same_short_name_in_different_namespaces()
    {
        var github = new GitHub.GitApi().GetClientName();
        var gitlab = new GitLab.GitApi().GetClientName();

        await Assert.That(github).IsEqualTo(typeof(GitHub.GitApi).FullName);
        await Assert.That(gitlab).IsEqualTo(typeof(GitLab.GitApi).FullName);
        await Assert.That(github).IsNotEqualTo(gitlab);
        await Assert.That(github).IsNotEqualTo(nameof(GitHub.GitApi));
    }

    [Test]
    public async Task GetClientName_rejects_the_base_type()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => Task.FromResult(HttpClientKey.GetClientName(typeof(HttpClientKey))));

        await Assert.That(exception!.Message).Contains("HttpClientKey");
    }

    [Test]
    public async Task GetClientName_rejects_generic_types()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => Task.FromResult(new GenericApi<int>()));

        await Assert.That(exception!.Message).Contains("generic");
    }

    [Test]
    public async Task GetClientName_rejects_abstract_types()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => Task.FromResult(HttpClientKey.GetClientName(typeof(AbstractApi))));

        await Assert.That(exception!.Message).Contains("abstract");
    }

    public sealed class GitApi() : HttpClientKey;

    public sealed class BillingApi() : HttpClientKey;

    public sealed class GenericApi<T>() : HttpClientKey;

    public abstract class AbstractApi() : HttpClientKey;

    public static class GitHub
    {
        public sealed class GitApi() : HttpClientKey;
    }

    public static class GitLab
    {
        public sealed class GitApi() : HttpClientKey;
    }
}
