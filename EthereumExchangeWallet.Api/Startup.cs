using EthereumExchangeWallet.Api.Data;
using EthereumExchangeWallet.Api.Data.Repositories;
using EthereumExchangeWallet.Api.Services;
using EthereumExchangeWallet.Api.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace EthereumExchangeWallet.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<DataContext>(options => {
                var settings = Configuration.GetSection(nameof(PostgresSettings)).Get<PostgresSettings>();
                options.UseNpgsql(settings.ConnectionString);
            });

            services.AddScoped(typeof(IRepository<>), typeof(PostgresRepository<>));
            // TODO: maybe add and a interface
            services.AddScoped<DepositAddressRepository>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAssetService, AssetService>();
            services.AddScoped<INethereumService, NethereumService>();

            services.AddMvc(options =>
            {
                options.SuppressAsyncSuffixInActionNames = false;
            });

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "EthereumExchangeWallet", Version = "v1" });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "EthereumExchangeWallet v1"));
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
