using Novolis.Http.Authentication.Basic;

namespace Novolis.Http.Unit;

public sealed class HttpAuthenticationArchitectureTests
{
    [Test]
    public async Task AuthenticationAssembly_DoesNotReferenceSecurity()
    {
        var names = typeof(BasicAuthenticationHttpClientBuilderExtensions).Assembly
            .GetReferencedAssemblies()
            .Select(assembly => assembly.Name!);
        await Assert.That(names.Any(name => name.StartsWith("Novolis.Security.", StringComparison.Ordinal))).IsFalse();
    }
}
