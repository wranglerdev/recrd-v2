using System.Reflection;
using Recrd.Application.Compilation;
using Recrd.Domain.Entities;

namespace Recrd.Application.Tests;

// PRD §18 (segurança): nenhuma comunicação externa. Guarda arquitetural — falha
// se Domain/Application passarem a referenciar HTTP/rede.
public class OfflineGuardTests
{
    [Theory]
    [InlineData(typeof(ScriptAction))]   // Recrd.Domain
    [InlineData(typeof(RobotCompiler))]  // Recrd.Application
    public void Core_layers_do_not_reference_networking(Type marker)
    {
        var referenced = marker.Assembly.GetReferencedAssemblies().Select(a => a.Name);

        referenced.Should().NotContain(name =>
            name!.Contains("System.Net.Http", StringComparison.OrdinalIgnoreCase) ||
            name!.Contains("System.Net.Sockets", StringComparison.OrdinalIgnoreCase));
    }
}
