using System.Reflection;
using Barberia.Api.Endpoints;
using Barberia.Application.Ports;
using Barberia.Domain.Common;
using Barberia.Infrastructure;
using NetArchTest.Rules;

namespace Barberia.Architecture.Tests;

/// <summary>Verifica que cada capa del hexágono dependa solo de las capas de adentro.</summary>
public sealed class LayerTests
{
    private const string ApplicationNamespace = "Barberia.Application";
    private const string InfrastructureNamespace = "Barberia.Infrastructure";
    private const string ApiNamespace = "Barberia.Api";
    private const string EntityFrameworkNamespace = "Microsoft.EntityFrameworkCore";
    private const string AspNetCoreNamespace = "Microsoft.AspNetCore";

    private static readonly Assembly DomainAssembly = typeof(BusinessTime).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(IClock).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(DependencyInjection).Assembly;
    private static readonly Assembly ApiAssembly = typeof(HealthEndpoints).Assembly;

    /// <summary>El dominio no conoce casos de uso, adaptadores, base de datos ni HTTP.</summary>
    [Fact]
    public void Domain_DependsOnNothingOutside()
    {
        AssertNoDependency(DomainAssembly, ApplicationNamespace, InfrastructureNamespace, ApiNamespace,
            EntityFrameworkNamespace, AspNetCoreNamespace);
    }

    /// <summary>Los casos de uso solo conocen el dominio y sus propios puertos.</summary>
    [Fact]
    public void Application_DependsOnlyOnDomain()
    {
        AssertNoDependency(ApplicationAssembly, InfrastructureNamespace, ApiNamespace, EntityFrameworkNamespace,
            AspNetCoreNamespace);
    }

    /// <summary>Los adaptadores de salida no conocen el adaptador HTTP.</summary>
    [Fact]
    public void Infrastructure_DoesNotDependOnApi()
    {
        AssertNoDependency(InfrastructureAssembly, ApiNamespace);
    }

    /// <summary>La API no usa la base de datos directamente, solo a través de los casos de uso.</summary>
    [Fact]
    public void Api_DoesNotUseDatabaseDirectly()
    {
        AssertNoDependency(ApiAssembly, EntityFrameworkNamespace);
    }

    /// <summary>Falla si algún tipo del ensamblado depende de alguno de los espacios de nombres prohibidos.</summary>
    private static void AssertNoDependency(Assembly assembly, params string[] forbidden)
    {
        NetArchTest.Rules.TestResult result = Types.InAssembly(assembly).ShouldNot().HaveDependencyOnAny(forbidden).GetResult();
        IEnumerable<string> offenders = result.FailingTypeNames ?? [];
        Assert.True(result.IsSuccessful, string.Join(", ", offenders));
    }
}
