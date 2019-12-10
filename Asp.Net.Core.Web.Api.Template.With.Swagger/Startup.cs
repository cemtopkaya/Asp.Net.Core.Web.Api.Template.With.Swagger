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
            });
            #endregion

            #region Swagger için JSON Web Token Ayarlarý
            services.AddSwaggerGen(c => {
                var securityScheme = new OpenApiSecurityScheme {
                    Reference = new OpenApiReference {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    },
                    Description = "JWT Token gerekiyor!",
                    In = ParameterLocation.Header, // Header içinde Bearer token key gelecek
                    Name = "Authorization", // Header içindeki token'ýn header key bilgisi> Authorization: Bearer xlaskdfjsdf...
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

            #region JSON Web Token ayarlarý
            var jwtSettings = new TokenSettings();
            this.Configuration.Bind("jwtSettings", jwtSettings);

            services.AddAuthentication(opt => {
                /**
                 * Bir Controller niteliðinde [Authorize] kullandýðýnýzda, 
                 * ilk yetkilendirme sistemine varsayýlan olarak baðlamasý için
                 */
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;

                /**
                 * Eðer `services.AddIdentity>()` ile bir kimlik kullanýyorsanýz, 
                 * DefaultChallengeScheme sizi bir giriþ sayfasýna yönlendirmeye çalýþacaktýr, 
                 * eðer bir kimlik mevcut deðilse, 404 döner. Eðer var ve yetkisiz ise 401 "yetkisiz eriþim" hatasý alacaksýnýz.
                 */
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(opt => {
                Microsoft.IdentityModel.Tokens.TokenValidationParameters param;

                param = new TokenValidationParameters {
                    // Require Bilgileri yoksa geçersiz kýl!
                    RequireSignedTokens = true,
                    RequireExpirationTime = true,

                    // Neleri doðrulamasýný istiyorsak
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,

                    // Beklediðimiz Issuer ve Audience bilgilerini veririz
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,

                    /**
                     * Gelen token bilgisiyle, gönderdiðimizin imzasý ayný mý diye 
                     * geleni imzalayacak ve kontrol edeceðiz
                     */
                    IssuerSigningKey = jwtSettings.IssuerSigningKey,
                };

                opt.RequireHttpsMetadata = false; // Production ortamý ise true yani https olsun diyebiliriz
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
                    // Her versiyon için bir uç nokta yaratmasýný Swagger'a söylüyoruz
                    c.SwaggerEndpoint($"/swagger/{v}/swagger.json", $"API Þablonu v{v}");
                }
            });
            #endregion

            #region CORS - Cross Origin Resource Sharing Ayarlarý
            /* AllowAnyOrigin: Herhangi bir domain adýndan
             * AllowAnyMethod: Herhangi bir HTTP metoduyla (GET, POST, PATH vs)
             * AllowAnyHeader: Herhangi bir HTTP Header bilgisiyle 
             * Yani herkesle her durumda her þeyi paylaþ
             */
            app.UseCors(confPolicy => {
                confPolicy.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
            });
            #endregion

            app.UseRouting();

            #region JSON Web Token
            /* app.UseAuthorization() metodu app.UseRouting() ile app.UseEndpoints() arasýnda olmalý!
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
