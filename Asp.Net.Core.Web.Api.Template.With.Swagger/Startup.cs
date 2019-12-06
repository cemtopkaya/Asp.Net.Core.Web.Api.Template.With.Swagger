/* SWAGGER 
 * Swagger i�in Swashbuckle.AspNetCore paketini y�kleyece�iz
 * public void ConfigureServices(IServiceCollection services) metoduna Swagger �reteceni ekleyece�iz:
 *  services.AddSwaggerGen(c =>...});
 *  
 * metoduna:
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace Asp.Net.Core.Web.Api.Template.With.Swagger
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            #region Api Versiyonlama
            services.AddApiVersioning(v =>
            {
                v.ReportApiVersions = true;
                v.AssumeDefaultVersionWhenUnspecified = true;
                v.DefaultApiVersion = new ApiVersion(1, 0);
                /* E�er versiyon bilgisi tarih olsun istersek: */
                //v.DefaultApiVersion = new ApiVersion(new DateTime(2016, 7, 1));

                /* E�er header ile versiyon bilgisi g�ndermezsek 
                 *   - ya URL Segment 
                 *   - ya da Querystring 
                 * olarak versiyon bilgisini ge�irebilece�iz.
                 * 
                 * E�er versiyon bilgisini HTTP Header i�inde g�ndermek istersek
                 * URL Segment i�inde versiyon bilgisi alabilecek �ekilde i�aretlenmemi�
                 * denetleyiciler yani Querystring ile �al��abilen denetleyiciler 
                 * header'dan gelen versiyon bilgisine g�re �al��abilir.
                 * �al���r  > [Route("api/versioning")]
                 * �al��MAZ > [Route("api/v{version:apiVersion}/versioning")]
                 * 
                 * Header i�inde versiyonu ta��yan key'in ne olaca��n� burada belirtebiliriz
                 */
                v.ApiVersionReader = new HeaderApiVersionReader("verMEZsion");
            });
            #endregion

            #region Swagger 2.0 - OpenApi 3.0
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "API �ablonu",
                    Description = "�rnek Web API swagger a��klamas�",
                    TermsOfService = new Uri("https://example.com/terms"),
                    Contact = new OpenApiContact
                    {
                        Name = "Shayne Boyer",
                        Email = string.Empty,
                        Url = new Uri("https://twitter.com/spboyer"),
                    },
                    License = new OpenApiLicense
                    {
                        Name = "Use under LICX",
                        Url = new Uri("https://example.com/license"),
                    }
                });

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            #region Swagger 
            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "API �ablonu V1");
            });

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                c.RoutePrefix = string.Empty;
            });
            #endregion

            app.UseCors(c =>
            {
                c.AllowAnyOrigin();
            });

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
