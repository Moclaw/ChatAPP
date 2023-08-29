using ChatAPP.API.Services;

namespace ChatAPP.API.Dependencies.Injections
{
    public class ServicesInstaller : IInstaller
    {
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddHostedService<ProducerHostedService>();
            services.AddHostedService<KafkaConsumerHostedService>();

            services.AddTransient<UserServices>();
            services.AddTransient<FileUploadServices>();
        }
    }
}
