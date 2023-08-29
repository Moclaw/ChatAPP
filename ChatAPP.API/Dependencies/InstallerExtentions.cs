namespace ChatAPP.API.Dependencies
{
    public static class InstallerExtensions
    {
       public static void InstallServicesAssembly(this IServiceCollection services, IConfiguration configuration)
        {
            var installer = typeof(Program).Assembly.ExportedTypes.Where(x =>
                  typeof(IInstaller).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
           .Select(Activator.CreateInstance).Cast<IInstaller>().ToList();

            installer.ForEach(x => x.InstallServices(services, configuration));
        }
    }
}
