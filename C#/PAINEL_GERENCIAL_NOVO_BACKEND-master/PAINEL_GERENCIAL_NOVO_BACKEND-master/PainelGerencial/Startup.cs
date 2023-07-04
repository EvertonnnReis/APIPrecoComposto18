using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using PainelGerencial.Controller;
using PainelGerencial.Repository;
using PainelGerencial.Services;
using PainelGerencial.Utils;
using Repository;

namespace PainelGerencial
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IPainelGerencialRepository, PainelGerencialRepository>();
            services.AddSingleton<INovosNegociosRepository, NovosNegociosRepository>();
            services.AddSingleton<IAuditoriaProdutosRepository, AuditoriaProdutosRepository>();
            services.AddSingleton<IAbacosRepository, AbacosRepository>();
            services.AddSingleton<IDropshippingRepository, DropshippingRepository>();
            services.AddSingleton<IVtexRepository, VtexRepository>();
            services.AddSingleton<IWsAbacos, WsAbacos>();
            services.AddSingleton<VtexController>();
            services.AddSingleton<IProdutoConversao, ProdutoConversao>();
            services.AddSingleton<IPrecificacaoCadastroRepository, PrecificacaoCadastroRepository>();
            services.AddSingleton<ControleAcessoController>();
            services.AddSingleton<IFenixServicos, FenixServicos>();
            services.AddSingleton<ISolicitacaoTemporariaServicos, SolicitacaoTemporariaServicos>();
            services.AddSingleton<HubController>();
            services.AddSingleton<IVtexServicos, VtexServicos>();
            services.AddSingleton<IAbacosServicos, AbacosServicos>();
            services.AddSingleton<IAuditoriaProdutosServicos, AuditoriaProdutosServicos>();

            services.AddMvcCore();

            services.AddControllers();


            // Para acessar o endpoint de um local diferente
            // Cors
            services.AddCors(o => o.AddPolicy("MyPolicy", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            }));

            // Swagger
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc(
                    "v1",
                    new OpenApiInfo
                    {
                        Title = "Painel Gerencial",
                        Version = "v1",
                        Description = "Bem vindo à API do Painel Gerencial"
                    }
                );
            });
            // Swagger
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }


            app.UseCors("MyPolicy");

            // Para acessar o endpoint de um local diferente
            // Cors
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                //endpoints.MapGet("/", async context =>
                //{
                //    //"Access-Control-Allow-Origin", "*"
                //    context.Response.Headers.Add("Content-Type", "text/json; charset=utf-8");
                //    context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                //    context.Response.Headers.Add("Access-Control-Request-Headers", "*");
                //    context.Response.Headers.Add("Access-Control-Allow-Methods", "*");
                //});

                endpoints.MapControllers();
            });

            // Swagger
            app.UseSwagger();
            app.UseSwaggerUI(opt =>
            {
                opt.RoutePrefix = "";
                opt.SwaggerEndpoint("/swagger/v1/swagger.json", "Painel Gerencial");
            });
            // Swagger
        }
    }
}
