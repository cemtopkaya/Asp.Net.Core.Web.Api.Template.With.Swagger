/* SWAGGER 
 * Swagger i�in Swashbuckle.AspNetCore paketini y�kleyece�iz
 * public void ConfigureServices(IServiceCollection services) metoduna Swagger �reteceni ekleyece�iz:
 *  services.AddSwaggerGen(c =>...});
 *  
 * metoduna:
 */


/* JSON Web Token
 * Microsoft.IdentityModel.Tokens 5.6.0 Paketini y�kl�yoruz
 * https://www.nuget.org/packages/Microsoft.IdentityModel.Tokens/5.6.0?_src=template
 * 
 * Microsoft.AspNetCore.Authentication.JwtBearer 3.0.0 Paketini 
 * .net core 3.0.0 s�r�m�yle uyumlu diye tercih ediyoruz
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
using Asp.Net.Core.Web.Api.Template.With.Swagger.Model.appsettings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
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
            services.AddSwaggerGen(c => {

                /* new SwaggerDocument()
                 * T�m Controller s�n�flar�n�n versiyon bilgilerini topla 
                 * ve her biri i�in SwaggerDocument yarat. B�ylece a��l�r kutuda
                 * her versiyon i�in detay bilgi toplayabil.
                 */
                #region SwaggerDocument Nesnelerini Yarat
                foreach (var version in Versions) {
                    var swaggerDocument = new OpenApiInfo {
                        Version = version,
                        Title = "API �ablonu",
                        Description = "�rnek Web API swagger a��klamas�",
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

                /* SwaggerDocument <> Controller E�le�mesi
                 * Versiyon numaralar�na g�re olu�turulmu� SwaggerDocument nesnelerine
                 * uygun d��ecek Controller s�n�flar�n� ekle. Bunun i�in: 
                 * denetleyicilerin ApiVersionAttribute niteliklerine bak, 
                 * e�er Controller'�n  versiyon numaras�yla, SwaggerDocument nesnesinin versiyon numaras� ayn�ysa
                 * ilgili SwaggerDocument i�inde g�r�nt�lemek i�in dahil et.
                 */
                #region SwaggerDocument nesneleriyle Controller tiplerini e�le�tir.
                c.DocInclusionPredicate((docName, apiDesc) => {

                    if (!apiDesc.TryGetMethodInfo(out MethodInfo methodInfo)) return false;

                    var versions = methodInfo.DeclaringType // Api i�inde tan�ml� tipleri getir
                        .GetCustomAttributes(true) // Niteliklerini �ek
                        .OfType<ApiVersionAttribute>() // ApiVersionAttribute tipindeki nitelikleri s�z
                        .SelectMany(attr => attr.Versions); // ApiVersion niteli�indeki Versions �zelliklerini flat d�n

                    return versions.Any(v => v.ToString() == docName);
                });
                #endregion

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
            #endregion

            #region Swagger i�in JSON Web Token Ayarlar�
            services.AddSwaggerGen(c => {
                var securityScheme = new OpenApiSecurityScheme {
                    Reference = new OpenApiReference {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    },
                    Description = "JWT Token gerekiyor!",
                    In = ParameterLocation.Header, // Header i�inde Bearer token key gelecek
                    Name = "Authorization", // Header i�indeki token'�n header key bilgisi> Authorization: Bearer xlaskdfjsdf...
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                };
                c.AddSecurityDefinition("Bearer", securityScheme);

                // Security Requirement
                c.AddSecurityRequirement(new OpenApiSecurityRequirement() {
                        { securityScheme, Array.Empty<string>() }
                    });
            });
            #endregion

            #region JSON Web Token ayarlar�
            var jwtSettings = new TokenSettings();
            this.Configuration.Bind("jwtSettings", jwtSettings);

            services.AddAuthentication(opt => {
                /**
                 * Bir Controller niteli�inde [Authorize] kulland���n�zda, 
                 * ilk yetkilendirme sistemine varsay�lan olarak ba�lamas� i�in
                 */
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;

                /**
                 * E�er `services.AddIdentity>()` ile bir kimlik kullan�yorsan�z, 
                 * DefaultChallengeScheme sizi bir giri� sayfas�na y�nlendirmeye �al��acakt�r, 
                 * e�er bir kimlik mevcut de�ilse, 404 d�ner. E�er var ve yetkisiz ise 401 "yetkisiz eri�im" hatas� alacaks�n�z.
                 */
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(opt => {
                Microsoft.IdentityModel.Tokens.TokenValidationParameters param;

                param = new TokenValidationParameters {
                    // Require Bilgileri yoksa ge�ersiz k�l!
                    RequireSignedTokens = true,
                    RequireExpirationTime = true,

                    // Neleri do�rulamas�n� istiyorsak
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,

                    // Bekledi�imiz Issuer ve Audience bilgilerini veririz
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,

                    /**
                     * Gelen token bilgisiyle, g�nderdi�imizin imzas� ayn� m� diye 
                     * geleni imzalayacak ve kontrol edece�iz
                     */
                    IssuerSigningKey = jwtSettings.IssuerSigningKey,
                };

                opt.RequireHttpsMetadata = false; // Production ortam� ise true yani https olsun diyebiliriz
                opt.SaveToken = true;
                opt.TokenValidationParameters = param;

                opt.Events = new JwtBearerEvents() {
                    OnAuthenticationFailed = context => {
                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json; charset=utf-8";
                        var message = context.Exception.ToString();
                        var result = JsonConvert.SerializeObject(new { message });
                        
                        Console.WriteLine("HATA >>>> " + message);
                        return context.Response.WriteAsync(result);
                    }
                };
            });
            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            } else {
                app.UseExceptionHandler("/Error/Exception");

                app.UseStatusCodePagesWithReExecute("/Error/{0}");

                app.UseHsts();

                app.UseHttpsRedirection();
            }

            #region Swagger 
            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c => {
                foreach (var v in Versions) {
                    // Her versiyon i�in bir u� nokta yaratmas�n� Swagger'a s�yl�yoruz
                    c.SwaggerEndpoint($"/swagger/{v}/swagger.json", $"API �ablonu v{v}");
                }
            });
            #endregion

            #region CORS - Cross Origin Resource Sharing Ayarlar�
            /* AllowAnyOrigin: Herhangi bir domain ad�ndan
             * AllowAnyMethod: Herhangi bir HTTP metoduyla (GET, POST, PATH vs)
             * AllowAnyHeader: Herhangi bir HTTP Header bilgisiyle 
             * Yani herkesle her durumda her �eyi payla�
             */
            app.UseCors(confPolicy => {
                confPolicy.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
            });
            #endregion

            app.UseRouting();

            #region JSON Web Token
            /* app.UseAuthorization() metodu app.UseRouting() ile app.UseEndpoints() aras�nda olmal�!
             * 
             * Configure your application startup by adding app.UseAuthorization() 
             * inside the call to Configure(..) in the application startup code. 
             * The call to app.UseAuthorization() must appear between app.UseRouting() and app.UseEndpoints(...)
             */
            app.UseAuthentication();
            app.UseAuthorization();
            #endregion

            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
            });
        }
    }
}
