# Asp.Net.Core.Web.Api.Template.With.Swagger
Şablon olarak kullanabileceğim SWAGGER ayarları yapılmış ASP.NET Core Web API uygulaması
### Controller Bilgileri
[Adding Swagger to ASP.NET Core Web API using XML Documentation](https://exceptionnotfound.net/adding-swagger-to-asp-net-core-web-api-using-xml-documentation/)

* Web API yazıyorsak `api/[controller]` diye gitmek isteriz bu yüzden controller üstünde Route özelliğine `api` ekleyelim:

```csharp
[Route("api/[controller]")]
[ApiController]
public class EmployeeController : ControllerBase
{
```

* Eğer action'a urlden parametre olarak veri geçirmek istiyorsak:
```csharp
[HttpGet]
[Route("byname/{firstName}/{lastName}")]
public ActionResult<string> GetByName(string firstName, string lastName)
{
```

##### Gönderilen Formun Büyüklüğünü Kısıtlamak
![image](https://user-images.githubusercontent.com/261946/70238375-32907b80-177a-11ea-9b74-bb32c839264f.png)

- Bir action Http'nin `POST` metoduyla çalışabilir ve URL adresi `[controller]/{predefinedId}` şeklinde olacaksa `[HttpPost("{predefinedId}")]` yeterlidir.
- Ancak bu action'a gelecek formun boyutunu 100M byte ile sınırlamak istersek `[RequestSizeLimit(100_000_000)]` yeterlidir.
- Eğer formun boyunu sınırsız hale getirmek istersek action metodunun üstüne aşağıdaki etiketi yazmak yeterlidir
```csharp
[HttpPost("{predefinedId}"), DisableRequestSizeLimit, RequestFormLimits(MultipartBodyLengthLimit = Int32.MaxValue, ValueLengthLimit = Int32.MaxValue)]
```
- Eğer sınırlamayı web.config veya startup.cs içinde yapmak istersek https://stackoverflow.com/questions/38698350/increase-upload-file-size-in-asp-net-core

### Routing
Kaynak: https://docs.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-3.1

Bir rota oluşturma prensibini MVC uygulamasında şu şekilde belirtiyoruz:
```csharp
app.UseMvc(routes =>
{
    routes.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
});
```

Bu yapıda bir adres oluşturmak için: 
```csharp
public class Url {
  .
  ..
  public virtual string Action (string actionName, string controllerName, object routeValues);
  ....
}

var link = Url.Action("ReadPost", "blog", new { id = 17, });
// Oluşan Adres: /blog/ReadPost/17
```

**cshtml Sayfalarımız** çağırıldığında rota bilgilerini sayfa içinde kullanabilmek için doğru parse etmemiz gerekiyor. 
Eğer uygun URL bilgisiyle bu sayfaları çağırmazsak hata alırız. 
Aşağıdaki `Product.cshtml` sayfasına `id` bilgisi mecburen  olmalı: `/Store/Product/18`

![image](https://user-images.githubusercontent.com/261946/70247037-baca4d00-1789-11ea-9fdb-c57753127c9a.png)

#### Rota Kısıtları
Rotamızın 
- int tipinde 
- en küçük 20 olması 
gerektiği gibi kısıtları birden fazla kısıtı : ile ayırarak ayarlayabiliriz.
```csharp
[Route("users/{id:int:min(20)}")]
public User GetUserById(int id) { }
```

String tipini belirtmeden doğrudan kısıtını yazabiliriz:
```csharp
[HttpGet]
[Route("myroute/{id:minlength(2)}")]
public IHttpActionResult KullaniciAdiBaslayan(string ileBaslayan) {...
```

###### Kısıtlar
![image](https://user-images.githubusercontent.com/261946/70248355-cf0f4980-178b-11ea-9400-b2a58234c1da.png)

###### Düzenli ifadeler kullanabiliriz
![image](https://user-images.githubusercontent.com/261946/70248486-07af2300-178c-11ea-9670-5292b1abc91e.png)


### Doğrulama (Validation) 
#### Entity Üstünde Validasyon
Bir MVC uygulamasındaki varlık sınıfı (entity) için validasyonu şu şekilde yazabiliriz:
```csharp
public class CinematicItem
{
   public int Id { get; set; }

   [Range(1,100)]
   public int Score { get; set; }

   [RegularExpression(@"^.{5,}$", ErrorMessage = "Minimum 3 characters required")]
   [Required(ErrorMessage = "Required")]
   [StringLength(30, MinimumLength = 3, ErrorMessage = "Invalid")]
   public string Title { get; set; }

   [StringLength(255)]
   public string Synopsis { get; set; }
  
   [DataType(DataType.Date)]
   [DisplayName("Available Date")]
   public DateTime AvailableDate { get; set; }


   /** 
    * https://stackoverflow.com/a/34238826/104085
    * RESOURCE üstünden hata mesajı okunduğunda ErrorMessageResourceType bilgisi verilir.
    * Resource şöyledir:
    *   "ThePasswordMustBeAtLeastCharactersLong" | "The password must be {1} at least {2} characters long"
    */
   [StringLength(16, 
                 ErrorMessageResourceName= "PasswordMustBeBetweenMinAndMaxCharacters", 
                 ErrorMessageResourceType = typeof(Resources.Resource), 
                 MinimumLength = 6)]
   [Display(Name = "Password", ResourceType = typeof(Resources.Resource))]
   public string Password { get; set; }

   [Required]
   [DisplayName("Movie/Show/etc")]
   public CIType CIType { get; set; }
}
```

Sunucu tarafında doğrulamak için:
```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(...)
{
   if (ModelState.IsValid)
   {
      // ... 
      return RedirectToAction(nameof(Index));
   }
   return View(cinematicItem);
}
```
ModelState.IsValid öğesinin, bir MVC denetleyicisinin (controller) `Create()` eylem yönteminde IsValid `true` ise, istediğiniz gibi eylemleri gerçekleştirir `false` ise, geçerli görünümü/sayfayı olduğu gibi yeniden yükler.

#### Özelleştirilmiş Doğrulayıcı (Custom Validator)

```csharp
public class MyStringLengthAttribute : StringLengthAttribute
{
    public MyStringLengthAttribute(int maximumLength)
        : base(maximumLength)
    {
    }

    public override bool IsValid(object value)
    {
        string val = Convert.ToString(value);
        if (val.Length < base.MinimumLength)
            base.ErrorMessage = "Minimum length should be 3";
        if (val.Length > base.MaximumLength)
            base.ErrorMessage = "Maximum length should be 6";
        return base.IsValid(value);
    }
}

public class MyViewModel
{
    [MyStringLength(6, MinimumLength = 3)]
    public String MyProperty { get; set; }
}
```
https://stackoverflow.com/a/18276949/104085


### Swagger Kurulumu
1. `Swashbuckle.AspNetCore` Paketini yükleyerek başlayalım.
2. `Startup.cs` dosyası aşağıdaki şekilde olacak:
```csharp
public class Startup
{
    //...

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

        //This line adds Swagger generation services to our container.
        services.AddSwaggerGen(c =>
        {
            //The generated Swagger JSON file will have these properties.
            
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1",
                Title = "Swagger XML Api Demo",
                Description = "A simple example ASP.NET Core Web API",
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
            
            //Locate the XML file being generated by ASP.NET...
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.XML";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            
            //... and tell Swagger to use those XML comments.
            c.IncludeXmlComments(xmlPath);
        });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseMvc();

        //This line enables the app to use Swagger, with the configuration in the ConfigureServices method.
        app.UseSwagger();
        
        //This line enables Swagger UI, which provides us with a nice, simple UI with which we can view our API calls.
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Swagger XML Api Demo v1");
        });
    }
}
```

3. Projenin özelliklerinden XML dosyası oluşturmayı açmamız gerekiyor. XML yorumlar ekleyerek SWAGGER UI'ın daha yardımcı görüntüleme yapabilmesini istiyoruz çünkü.
![https://exceptionnotfound.net/content/images/2018/07/xml-doc-file-build.png](https://user-images.githubusercontent.com/261946/70221619-c00fa380-1759-11ea-9f77-a5295e136cb7.png)

4. Ancak XML oluşturma nedeniyle bazı uyarılar alacağız aşağıdaki gibi
![image](https://user-images.githubusercontent.com/261946/70222307-e97cff00-175a-11ea-9a87-ffdc50a0f8a0.png)

Bu uyarıları baskılamak için iki yolumuz var

4.1. Proje ayarlarından:
![Suppres warnings 1591 eklenir](https://user-images.githubusercontent.com/261946/70222369-0a455480-175b-11ea-8d44-6b2ba8775492.png)

4.2 Proje dosyasından:
![image](https://user-images.githubusercontent.com/261946/70222960-d28adc80-175b-11ea-86f8-17fe5825f8e3.png)

### Swagger XML Yorumlarını Nasıl Yorumluyor?
XML düğümlerinden şunlar kullanılıyor:
* *summary:* method/class/field gibi üyeler için özet bilgi yazabildiğimiz XML düğümü
* *remarks:* method/class/field gibi üyeler için FAZLADAN bilgi girebileceğimiz düğüm
* *param:* Metotların parametreleri hakkında bilgi yazabileceğimiz düğüm
* *returns:* Metodun dönüş verisiyle ilgili bilgi yazabileceğimiz düğüm
Maalesef returns düğümünü Swagger UI görüntülemiyor. Dönen değer tipine bakarak varsayılan olarak yazabiliyor veya `[ProducesResponseType]` özelliğine bakarak değerlendiriyor.
* *see:* Bir tipi referans verebilir onun tam adıyla görüntülenmesini sağlayabilirsiniz
![image](https://user-images.githubusercontent.com/261946/70236627-5a7de000-1776-11ea-8931-5de0fd154f81.png)

Bu ekran çıktısını genel olarak güzel bir gösterimi olduğu yapıştırıyorum. Ama daha detaylı halini aşağıda ekleyeceğim.
![image](https://user-images.githubusercontent.com/261946/70224760-df5cff80-175e-11ea-89b5-d1c45b111ba4.png)

### PARAMETRELER
Bir metodun parametre listesinde mecburi (required) ve seçimli (optional) parametreler olabilir. 

#### Mecburi (required) Parametre

```csharp
public Response Post(int predefinedId, int? oylesine, [FromBody] WeatherForecast wcast) {
    return new Response() { };
}
```
Mecburi parametre olan `predefinedId` Swagger tarafından **required* olarak görüntüleniyor.
![image](https://user-images.githubusercontent.com/261946/70237216-bb59e800-1777-11ea-91b1-4e2b42ce14cb.png)
`http://localhost:63270/api/WeatherForecast/123` Adresindeki 123 değeri metoda `predefinedId` isimli parametre olarak geçecek.

#### Seçimli (optional) Parametre

Seçimli parametre olan `userId` URL satırında Query olarak girilebilecek. Zorunlu olmadığı için **required* görünmeyecek.
![image](https://user-images.githubusercontent.com/261946/70237545-84380680-1778-11ea-8ae6-d8ae870b8c31.png)

#### `[FromBody]` Özelliğiyle Gövdede Taşınan Veri
HTTP Paketinin gövdesinde (body) gelen verinin içeriğini yine metodun parametre listesinden okur: 
```csharp
[FromBody] WeatherForecast wcast
```

`WeatherForecast` Tipini metodun alabileceği formatlara göre görüntüler. Metodun alabileceği formatları `[Consumes]` özelliğinden okur. 
`[Consumes]` İle belirtilen formatları açılır kutuda listeleyerek istenilen formata göre `WeatherForecast` tipini serileştirir.
```csharp
[Consumes("application/json", "application/xml", "application/x-www-form-urlencoded")]
```

1. ###### `application/json` İçin:
![image](https://user-images.githubusercontent.com/261946/70239788-032f3e00-177d-11ea-82e2-2b24f060fe51.png)

2. ###### `application/xml` İçin:
![image](https://user-images.githubusercontent.com/261946/70239836-1c37ef00-177d-11ea-9411-a9d03b04cd84.png)

3. ###### `application/x-www-form-urlencoded` İçin:
![image](https://user-images.githubusercontent.com/261946/70239898-3eca0800-177d-11ea-9a0e-9d83a99b9947.png)


### Dönüş Tipleri (`<returns>`) & Cevaplar (Responses)
`<returns>` XML düğümüne girilen bilgileri Swagger görüntülemiyor! 
Ancak [ProducesResponseType] ve `/// <response code="401">Yetkisiz erişim.</response>` açıklamalarını görüntüler.
#### 500 - Internal Server Error
Dönebilecek cevaplar listesinde 500 hata kodunu XML düğümü olarak şöyle yazmıştık:

```csharp
/// <response code="500">Sunucu hatası dönüş tipine bakmaz doğrudan string tipinde sonuç döner.</response>
```



Swagger UI 500 kodunda dönecek mesajı görüntülesin diye `ProducesResponseType` özelliğine 
`[ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]` yazmıştık.
![image](https://user-images.githubusercontent.com/261946/70238213-e6453b80-1779-11ea-9e45-8613f04f14f0.png)


String dönecek diye işaretlediğimiz 500 kodlu hatanın response hali:
![image](https://user-images.githubusercontent.com/261946/70237964-57d0ba00-1779-11ea-9edd-e3b261a8e84e.png)
