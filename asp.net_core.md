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

***

### [FromRoute], [FromQuery] & [FromBody] İle [Parametre Bağlama](https://docs.microsoft.com/en-us/aspnet/web-api/overview/formats-and-model-binding/parameter-binding-in-aspnet-web-api)
Kaynaklar:
- https://andrewlock.net/model-binding-json-posts-in-asp-net-core/
- https://docs.microsoft.com/en-us/aspnet/core/mvc/models/model-binding?view=aspnetcore-3.1
- https://docs.microsoft.com/en-us/aspnet/web-api/overview/formats-and-model-binding/parameter-binding-in-aspnet-web-api
HTTP Paketinin adres bilgisinde  ve gövdesinde bilgi taşıyabiliriz.
- `[FromRoute]`, `[FromQuery]` ile adres (URL) bilgisinden parametreye veriyi bağlıyoruz
  - eski WEB API versiyonlarında `[FromUri]` ile yapıyorduk
- `[FromForm]`, `[FromBody]` Gövdedeki bilgiyi parametreye bağlamak için kullanıyoruz
  - `[FromForm]`: Eğer içeriğin formatı `application/x-www-url-formencoded` ise
  - `[FromBody]`: Eğer içeriğin formatı `application/json` ise

Çağırdığımız action metodu bir uç nokta (end-point) olarak çalışır. 
Bu metoda gelen parametreleri
- ilkel (**primitive**) tipler için metodun parametre listesine doğrudan yazarak
```csharp
public void Metot(int yas, string okul, bool cinsiyet) {...}
```
- Karmaşık (**complex**) tiplerin değerlerini adres bilgisinden metodun parametresine otomatik bağlayabilmek için 
```csharp
/* Enlem ve boylamı otomatik olarak metodun location parametresine bağlamak:
 * http://localhost/api/values/?Latitude=47.678558&Longitude=-122.130989
 */
public class GeoPoint {
    public double Latitude { get; set; } 
    public double Longitude { get; set; }
}

public ValuesController : ApiController {
    public HttpResponseMessage Get([FromUri] GeoPoint location) { ... }
}
```

#### HTTP Paketinin Gövdesinde Taşınan Bilgiyi Parametreye Bağlamak
> 1. Bunun için `[FromBody]` ve `[FromForm]` nitelikleri kullanılabilir.
> 2. Gövdedeki bilgiyi sadece 1 parametreye bağlayabiliriz. 
> 3. Consumes etiketiyle gövdedeki bilgi hangi formatlarda olursa kullanılabileceğini tayin ederiz.

![image](https://user-images.githubusercontent.com/261946/70319977-ac853b00-1834-11ea-90c3-2818fcc82c8d.png)

> Google Chrome'un [fetch](https://developers.google.com/web/updates/2015/03/introduction-to-fetch) API'sini kullanarak sorgulayacağız
    
###### *JSON* Formatındaki Gövdeyi Parametreye Bağlamak
```csharp
// Sunucu
[HttpPost("body/{id}")]
[Consumes("application/json")]
public WeatherForecast Post_FromBody(int id, [FromBody] WeatherForecast wcast) => wcast;
```

```javascript
// İstemci
var data = JSON.stringify({
  Date: '2019-12-06T11:19:17.531Z',
  TemperatureC: 10,
  TemperatureF: 20,
  Summary: 'string'
})

fetch('http://localhost:63270/api/ParameterBindingTutorial/body/1', {
  headers: { 'Content-Type': 'application/json; charset=UTF-8' },
  method: 'POST',
  body: data
})
.then((response) => {
      response.json().then(console.log);
})
.catch(console.error);
```
    
###### *x-www-form-urlencoded* Formatındaki Gövdeyi Parametreye Bağlamak
```csharp
// Sunucu
[HttpPost("form/{id}")]
[Consumes("application/json", "application/x-www-form-urlencoded")]
public WeatherForecast Post_FromForm(int id, [FromForm] WeatherForecast wcast) => wcast;
```

```javascript
// İstemci
var data = 'date=1111-11-06T10%3A47%3A41.555Z&temperatureC=0&summary=string'

fetch('http://localhost:63270/api/ParameterBindingTutorial/form/1', {
  headers: { 'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8' },
  method: 'POST',
  body: data
})
.then((response) => {
      response.json().then(console.log);
})
.catch(console.error);
```


#####  i. [FromBody] Niteliği
Çalışan haliyle sunucu ve istemci tarafındaki kodlar şöyle olmalı:

![image](https://user-images.githubusercontent.com/261946/70321445-6a5df880-1838-11ea-914d-68aab8c11a22.png)

###### Akış & Sonuçlar
> - `FromBody` niteliği, gelen http isteğinin `Content-Type` başlığına bakar. 
> - Eğer http isteğinin gövdesi, `Consumes` niteliğiyle belirtilen tiplerin dışında bir formatla gelmişse `415 (Unsupported Media Type)` hatasını fırlatır.
>   - ![image](https://user-images.githubusercontent.com/261946/70321904-ae053200-1839-11ea-8055-0bec909c9313.png)

> - Eğer içerik tipi uygun ama gövdedeki bilgi parse edilemezse `400 Bad Request` ile validasyon hatası verir
>   - ![image](https://user-images.githubusercontent.com/261946/70322548-6e3f4a00-183b-11ea-9fb1-d75560508b64.png)




##### ii. [FromForm] Niteliği
![image](https://user-images.githubusercontent.com/261946/70319309-0a188800-1833-11ea-97ae-8605674a3546.png)

---

###### `"Content-Type" : "application/json"` Talebini `[FromForm]` Niteliğiyle Parse Edememek!
`Consumes` niteliği her ne kadar `"application/json"` kabul etse bile `FromForm` niteliği gelen bilgiyi 
`application/x-www-form-urlencoded` formatında parse etmeye çalışır. 
Dönen sonuç `new WeatherForecast()` nesnesidir. Çünkü `[FromForm]` niteliği geleni parse etmeden önce yeni bir `WeatherForecast` nesnesi 
yaratır ve gelen bilginin özelliklerini bu nesnenin özelliklerine basmaya çalışır. Parse edemediği için eldeki `WeatherForecast` nesnesini döner.

![image](https://user-images.githubusercontent.com/261946/70320546-0d614300-1836-11ea-9df6-b23dece51fd3.png)

---

- Gövdede taşınan bilgiyi (ilkel veya karmaşık) otomatik olarak parametreye bağlamak için sadece **1 adet** `[FromBody]` kullanabiliriz
```csharp
// Çalışır :)
public HttpResponseMessage Post([FromBody] string name) { ... }

// ÇalışMAZ :(
public HttpResponseMessage Post([FromBody] int id, [FromBody] string name) { ... }
```

- İstemciden gelen karmaşık verileri, sunucuda kendi özel tiplerimize bağlayabilmek için:
  -  [TypeConverter](https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.typeconverter?view=netcore-3.0) veya
  -  [ModelBinder](https://docs.microsoft.com/en-us/aspnet/core/mvc/models/model-binding?view=aspnetcore-3.1) kullanıyoruz  
 
*** 

### [Produces] Niteliğiyle Response Formatı
XML Sonuçlar dönebilmek için XML Formatlayıcısını da eklememiz gerekiyor.

```csharp
public void ConfigureServices(IServiceCollection services) {
    services.AddControllers()
            .AddXmlSerializerFormatters()
```

```csharp
using Microsoft.AspNetCore.Mvc;

namespace Asp.Net.Core.Web.Api.Template.With.Swagger.Controllers {

    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class ProducesResponseFormatController : ControllerBase {
        [HttpGet]
        [Produces("application/xml")]
        public IActionResult Get() {
            /**
             * Anonymous tiplerin XML dönüşümleri gerçekleştirilemez. HTTP 406 hatası döner.
             *    return Ok(new { a = "aaaa" });
             * Bu yüzden tanımlı tiplerin nesnelerini cevap olarak dönmeliyiz
             *    return Ok(new ConcreteClass());
             */
            return Ok(new Model.appsettings.TokenSettings());
        }
    }
}
```

![image](https://user-images.githubusercontent.com/261946/70566359-f8bddb80-1ba4-11ea-9ab1-899dbaf697a3.png)
