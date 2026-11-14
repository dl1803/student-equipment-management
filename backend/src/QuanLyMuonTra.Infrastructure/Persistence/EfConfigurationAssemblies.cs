using System.Reflection;

namespace QuanLyMuonTra.Infrastructure.Persistence;

public sealed record EfConfigurationAssemblies(IReadOnlyCollection<Assembly> Assemblies);

