/* SWAGGER 
 * Swagger için Swashbuckle.AspNetCore paketini yükleyeceðiz
 * public void ConfigureServices(IServiceCollection services) metoduna Swagger üreteceni ekleyeceðiz:
 *  services.AddSwaggerGen(c =>...});
 *  
 * metoduna:
 */


/* JSON Web Token
 * Microsoft.IdentityModel.Tokens 5.6.0 Paketini yüklüyoruz
 * https://www.nuget.org/packages/Microsoft.IdentityModel.Tokens/5.6.0?_src=template
 * 
 * Microsoft.AspNetCore.Authentication.JwtBearer 3.0.0 Paketini 
 * .net core 3.0.0 sürümüyle uyumlu diye tercih ediyoruz
 * 
 * appsettings.json:
 
  "JwtSettings": {
    "Secret": "gizli_anahtar_yeterince_uzun_olmazsa_hata_verir_cok_uzun_olursa_perf_kaybi_yasatir",
    "Issuer": "Petronet",
    "Audience": "TCDD",
    "ExpirationTime": 30,
    "UygulamaAdi": "LokoServis"
  },
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Asp.Net.Core.Web.Api.Template.With.Swagger {
    public class Startup {
        #region Props & Fields
        IOrderedEnumerable<string> _versions;
        private IOrderedEnumerable<string> Versions {
            get {
                if (_versions == null) {
                    var types = this.GetType().Assembly.GetTypes();
                    var versionedControllers = types.Where(t => typeof(ControllerBase).IsAssignableFrom(t));

                    _versions = versionedControllers
                    .Select(ctrl => ctrl.GetCustomAttributes<ApiVersionAttribute>(false))
                    .SelectMany(f => f)
                    .SelectMany(v => v.Versions)
                    .Select(f => f.ToString())
                    .Distinct()
                    .OrderBy(s => s);
                }

                return _versions;
            }
        }

        public IConfiguration Configuration { get; }
        #endregion

        #region Constructors
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }
        #endregion

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            services.AddControllers();

            #region Api Versiyonlama
            services.AddApiVersioning(v => {
                v.ReportApiVersions = true;
                v.AssumeDefaultVersionWhenUnspecified = true;
                v.DefaultApiVersion = new ApiVersion(1, 0);
                /* Eðer versiyon bilgisi tarih olsun istersek: */
                //v.DefaultApiVersion = new ApiVersion(new DateTime(2016, 7, 1));

                /* Eðer header ile versiyon bilgisi göndermezsek 
                 *   - ya URL Segment 
                 *   - ya da Querystring 
                 * olarak versiyon bilgisini geçirebileceðiz.
                 * 
                 * Eðer versiyon bilgisini HTTP Header içinde göndermek istersek
                 * URL Segment içinde versiyon bilgisi alabilecek þekilde iþaretlenmemiþ
                 * denetleyiciler yani Querystring ile çalýþabilen denetleyiciler 
                 * header'dan gelen versiyon bilgisine göre çalýþabilir.
                 * Çalýþýr  > [Route("api/versioning")]
                 * ÇalýþMAZ > [Route("api/v{version:apiVersion}/versioning")]
                 * 
                 * Header içinde versiyonu taþýyan key'in ne olacaðýný burada belirtebiliriz
                 */
                v.ApiVersionReader = new HeaderApiVersionReader("verMEZsion");
            });
            #endregion

            #region Swagger 2.0 - OpenApi 3.0
            services.AddSwaggerGen(c => {

                /* new SwaggerDocument()
                 * Tüm Controller sýnýflarýnýn versiyon bilgilerini topla 
                 * ve her biri için SwaggerDocument yarat. Böylece açýlýr kutuda
                 * her versiyon için detay bilgi toplayabil.
                 */
                #region SwaggerDocument Nesnelerini Yarat
                foreach (var version in Versions) {
                    var swaggerDocument = new OpenApiInfo {
                        Version = version,
                        Title = "API Þablonu",
                        Description = "Örnek Web API swagger açýklamasý",
                        TermsOfService = new Uri("https://example.com/terms"),
                        Contact = new OpenApiContact {
                            Name = "Shayne Boyer",
                            Email = string.Empty,
                            Url = new Uri("https://twitter.com/spboyer"),
                        },
                        License = new OpenApiLicense {
                            Name = "Use under LICX",
                            Url = new Uri("https://example.com/license"),
                        }
                    };
                    c.SwaggerDoc(version, swaggerDocument);
                }
                #endregion

                /* SwaggerDocument <> Controller Eþleþmesi
                 * Versiyon numaralarýna göre oluþturulmuþ SwaggerDocument nesnelerine
                 * uygun düþecek Controller sýnýflarýný ekle. Bunun için: 
                 * denetleyicilerin ApiVersionAttribute niteliklerine bak, 
                 * eðer Controller'ýn  versiyon numarasýyla, SwaggerDocument nesnesinin versiyon numarasý aynýysa
                 * ilgili SwaggerDocument içinde görüntülemek için dahil et.
                 */
                #region SwaggerDocument nesneleriyle Controller tiplerini eþleþtir.
                c.DocInclusionPredicate((docName, apiDesc) => {

                    if (!apiDesc.TryGetMethodInfo(out MethodInfo methodInfo)) return false;

                    var versions = methodInfo.DeclaringType // Api içinde tanýmlý tipleri getir
                        .GetCustomAttributes(true) // Niteliklerini çek
                        .OfType<ApiVersionAttribute>() // ApiVersionAttribute tipindeki nitelikleri süz
                        .SelectMany(attr => attr.Versions); // ApiVersion niteliðindeki Versions özelliklerini flat dön

                    return versions.Any(v => v.ToString() == docName);
                });
                #endregion

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);

                #region Swagger için JSON Web Token Ayarlarý
                var securityScheme = new OpenApiSecurityScheme {
                    Description = "JWT Token gerekiyor!",
                    In = ParameterLocation.Header,
                    Name = "authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                };
                c.AddSecurityDefinition("Bearer", securityScheme);

                // Security Requirement
                var securityReq = new OpenApiSecurityRequirement();
                var value = new List<string>();
                securityReq.Add(securityScheme, value);

                c.AddSecurityRequirement(securityReq);
                #endregion
            });
            #endregion

            #region JSON Web Token ayarlarý
            var jwtSettings = new TokenSettings();
            this.Configuration.Bind("jwtSettings", jwtSettings);
            JwtBearerExtensions.AddJwtBearer(services.AddAuthentication("Bearer"), opt => {
                var param = new Microsoft.IdentityModel.Tokens.TokenValidationParameters {
                    RequireSignedTokens = true,
                    RequireExpirationTime = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = jwtSettings.IssuerSigningKey
                };

                opt.TokenValidationParameters = param;
            });
            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }

            #region Swagger 
            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c => {
                foreach (var v in Versions) {
                    // Her versiyon için bir uç nokta yaratmasýný Swagger'a söylüyoruz
                    c.SwaggerEndpoint($"/swagger/{v}/swagger.json", $"API Þablonu v{v}");
                }
            });
            #endregion

            #region JSON Web Token
            app.UseAuthentication();
            #endregion

            app.UseCors(c => {
                c.AllowAnyOrigin();
            });

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
            });
        }
    }
}
