using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WpfDR.Data;
using WpfDR.Model;
using WpfDR.Service;
using WpfDR.ViewModels;

namespace WpfDR
{
    public partial class App
    {
        private static IHost __hosting;

        public static IHost Hosting => __hosting
            ??= CreateHostBulder(Environment.GetCommandLineArgs()).Build();
        public static IServiceProvider Services => Hosting.Services;

        public static IHostBuilder CreateHostBulder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureServices(ConfigureServices);

        private static void ConfigureServices(HostBuilderContext host, IServiceCollection services)
        {
            services.AddDbContext<WpfDb>(opt => opt.UseSqlite(host.Configuration.GetConnectionString("SqLite")));
            services.AddScoped<IRepository<MailItemDb>,DbRepository<MailItemDb>>();

            services.AddSingleton<MainWindowViewModel>();
            services.AddScoped<FileListWindowViewModel>();
            services.AddTransient<FileRepackWindowViewModel>();
            services.AddTransient<ParserService>();
        }
    }
}
