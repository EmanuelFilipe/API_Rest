using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

namespace DevIO.Api.Configuration
{
    public static class ApiConfig
    {
        public static IServiceCollection WebApiConfig(this IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ReportApiVersions = true;
            });

            services.AddVersionedApiExplorer(options =>
            {
                // agrupar versões da Api: v de versão ... VVV aceita até três parâmetros
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true; // suprimindo validação da viewmodel automática
            });

            services.AddCors(options =>
            {
                // para ambiente Development
                options.AddPolicy("Development",
                    builder => builder.AllowAnyOrigin()
                                      .AllowAnyMethod()
                                      .AllowAnyHeader()
                                      .AllowCredentials());

                // Política Padrão
                //options.AddDefaultPolicy(builder => builder.AllowAnyOrigin()
                //                                           .AllowAnyMethod()
                //                                           .AllowAnyHeader()
                //                                           .AllowCredentials());

                // para ambiente Production
                options.AddPolicy("Production",
                    builder => builder.WithMethods("GET") // Pode-se adicionar mais verbos, separando-os por vírgula
                                      .WithOrigins("http://desenvolvedor.io")// Pode-se adicionar mais domínios,  separando-os por vírgula
                                      .SetIsOriginAllowedToAllowWildcardSubdomains()
                                      //.WithHeaders(HeaderNames.ContentType, "x-custom-header")
                                      .AllowAnyHeader());
            });

            return services;
        }

        public static IApplicationBuilder UseMvcConfiguration(this IApplicationBuilder app)
        {
            app.UseCors("Development");
            app.UseHttpsRedirection();
            app.UseMvc();

            return app;
        }
    }
}
