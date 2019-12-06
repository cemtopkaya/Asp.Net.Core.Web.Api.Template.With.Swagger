# Asp.Net Core Web Api Versiyonlama
Kaynaklar:
- https://www.infoworld.com/article/3433156/advanced-versioning-in-aspnet-core-web-api.html?upd=1575639094602
- https://www.hanselman.com/blog/ASPNETCoreRESTfulWebAPIVersioningMadeEasy.aspx

## Ön Gereklilikler
1. Önce versiyonlama için `Microsoft.AspNetCore.Mvc.Versioning` paketininin `4.0.0-preview8.19405.7` versiyonunu yükleyelim. Çünkü diğer versiyonları Asp.Net Core 3.0.1 sonrası çalışmıyor.
2. Versiyonlamaya yapabilmek için servislere ekleyelim
```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddControllers();

    #region Api Versiyonlama
    services.AddApiVersioning(v =>
    {
        v.ReportApiVersions = true;
        v.AssumeDefaultVersionWhenUnspecified = true;
        v.DefaultApiVersion = new ApiVersion(1, 0);
        /* Eğer versiyon bilgisi tarih olsun istersek: */
        //v.DefaultApiVersion = new ApiVersion(new DateTime(2016, 7, 1));

        /* Eğer header ile versiyon bilgisi göndermezsek 
         *   - ya URL Segment 
         *   - ya da Querystring 
         * olarak versiyon bilgisini geçirebileceğiz.
         * 
         * Eğer versiyon bilgisini HTTP Header içinde göndermek istersek
         * URL Segment içinde versiyon bilgisi alabilecek şekilde işaretlenmemiş
         * denetleyiciler yani Querystring ile çalışabilen denetleyiciler 
         * header'dan gelen versiyon bilgisine göre çalışabilir.
         * Çalışır  > [Route("api/versioning")]
         * ÇalışMAZ > [Route("api/v{version:apiVersion}/versioning")]
         * 
         * Header içinde versiyonu taşıyan key'in ne olacağını burada belirtebiliriz
         */
        v.ApiVersionReader = new HeaderApiVersionReader("verMEZsion");
    });
    #endregion
} 
```

## QueryString Parametresiyle Versiyonlama

1. `versioning` Adlı denetleyicimizin 1 ve 2 inci versiyonlarını elbette farklı sınıf adları ama aynı denetleyici adıyla bir dosyada tanımlayalım
```csharp
using Microsoft.AspNetCore.Mvc;

namespace Asp.Net.Core.Web.Api.Template.With.Swagger.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/versioning")]
    [ApiController]
    public class Versioning_QueryString_Controller_v1 : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() => Ok("QueryString - version 1.0");
    }

    [ApiVersion("2.0")]
    [ApiVersion("3.0")]
    [Route("api/versioning")]
    [ApiController]
    public class Versioning_QueryString_Controller_v2 : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() => Ok("QueryString - version 2.0 ~ 3.0");
    }

    [ApiVersion("4.0-Beta")]
    [ApiVersion("4.0")]
    [ApiVersion("4.1")]
    [ApiVersion("4.5")]
    [Route("api/versioning")]
    [ApiController]
    public class Versioning_QueryString_Controller_v4 : ControllerBase
    {
        [HttpGet]
        public IActionResult Get(ApiVersion v)
        {
            return Ok(new
            {
                Method = "QueryString",
                Requested_Version = v,
                Description = "4.0-Alpha< version <=4.5"
            });
        }
    }
}
```
2. `v.DefaultApiVersion = new ApiVersion(1, 0);` İle varsayılan versiyon numarasını "1.0" olarak belirtiyoruz. 
Bu durumda http://localhost:60286/api/versioning adresine gelen isteği 
`DefaultApiVersion` özelliğiyle aynı olan "1.0" versiyonunu içeren `Versioning_QueryString_Controller_v1` sınıfına yönlendirecek.
> ![image](https://user-images.githubusercontent.com/261946/70335311-1e22b080-1858-11ea-8761-c2b5f390f68a.png)

3. Aynı versiyonu bilinçli olarak istersek:
> ![image](https://user-images.githubusercontent.com/261946/70335276-106d2b00-1858-11ea-99cc-a48332eaa770.png)

4. Eğer Query ile varsayılan olmayan bir versiyonu ("2.0", "3.0") istersek bu kez `Versioning_QueryString_Controller_v2` denetleyicisine yönlenecek:
> ![image](https://user-images.githubusercontent.com/261946/70335200-e4ea4080-1857-11ea-9c34-df2147e31dbc.png)
> ![image](https://user-images.githubusercontent.com/261946/70335238-fb909780-1857-11ea-9426-2eacb32710b2.png)

## URL Üstünden Versiyonlama
URL adresi üstünden versiyonlama yapmak istediğimizde versiyon bilgisini URL adresinin bir parçası olarak yazarız: http://localhost:123/api/v1/kisi 

1. Startup.cs önceki gibi varsayılan versiyon seçilecek şekilde ayarlanır.
2. URL versiyonlamada bu kez şu denetleyicileri yazalım:
```csharp
using Microsoft.AspNetCore.Mvc;

namespace Asp.Net.Core.Web.Api.Template.With.Swagger.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/versioning")]
    [ApiController]
    public class Versioning_UrlPathSegment_Controller_v1 : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() => Ok("URL Path Segment - version 1.0");
    }

    [ApiVersion("2.0")]
    [ApiVersion("3.0")]
    [ApiVersion("3.1")]
    [Route("api/v{version:apiVersion}/versioning")]
    [ApiController]
    public class Versioning_UrlPathSegment_Controller_v2 : ControllerBase
    {
        [HttpGet]
        public IActionResult Get(ApiVersion version) => Ok(version + " URL Path Segment - version 2.0 ~ 3.1");
    }


    [ApiVersion("4.0-Alpha")]
    [ApiVersion("4.1")]
    //[ApiVersion("4.1.1")] // Çalıştırılamaz çünkü 3. grup string tipinde Status olmalı!
    [ApiVersion("4.2")]
    [ApiVersion("4.3-Alpha")]
    [ApiVersion("4.3-Beta")]
    [ApiVersion("4.3-RC1")]
    [ApiVersion("4.3-RC2")]
    [ApiVersion("4.3")]
    [ApiVersion("4.4")]
    [ApiVersion("4.5")]
    [Route("api/v{version:apiVersion}/versioning")]
    [ApiController]
    public class Versioning_UrlPathSegment_Controller_v4 : ControllerBase
    {
        [HttpGet]
        public IActionResult Get(ApiVersion v)
        {
            return Ok(new
            {
                Method = "URL Path Segment",
                Requested_Version = v,
                Description = "4.0-Alpha< version <=4.5"
            });
        }
    }
}
```
- En basit haliyle URL içine yazdığımız versiyon bilgisi aynı versiyonda 2 denetleyici olmasına rağmen 
(`Versioning_UrlPathSegment_Controller_v1` ve `Versioning_QueryString_Controller_v1`) URL üstünden versiyonladığımız için
`Versioning_UrlPathSegment_Controller_v1` sınıfın "1.0" versiyonunun Get metodu çağırıldı.
> ![image](https://user-images.githubusercontent.com/261946/70336184-c127fa00-1859-11ea-82e8-1a9df78a561a.png)
- URL Adresinde 2 farklı versiyonu hem URL Segment olarak hem QueryString olarak istememize rağmen ilk versiyonlama olan 
URL Segment sonucu gösterildi. 
> ![image](https://user-images.githubusercontent.com/261946/70336464-3abfe800-185a-11ea-8824-7e9fd9c82f0f.png)
    
- `ApiVersion` tipinde parametreyi metodumuza girdiğimiz vakit URL üstündeki versiyon bilgisi otomatik olarak parametremize bağlanır.
Versiyon 3 parçadan oluşuyor `major.minor.status`. Status bilgisi metin değeri alıyor (Alpha, RC, Beta, Preview vs).
> ![image](https://user-images.githubusercontent.com/261946/70336086-96d63c80-1859-11ea-9403-e4f112df25b3.png)

- Aradığımız versiyon bilgisine ulaşılamayınca! (UnsupportedApiVersion)
> ![image](https://user-images.githubusercontent.com/261946/70336974-3f38d080-185b-11ea-866c-a4bad60e562f.png)

## Header İçinde Versiyon Bilgisi

> - Header içinde anahtarın ne olacağını Startup.cs dosyasında belirliyoruz. 
> - Route bilgisinde versiyonu çekmeyen tüm denetleyiciler (yani `[Route("api/[controller]")]` tadındakiler) 
> header içindeki versiyon bilgisine göre çalıştırılırlar.

Javascript ile çalıştırmak:

![image](https://user-images.githubusercontent.com/261946/70341629-6fd13800-1864-11ea-96db-e058f4ab3bea.png)

Postman ile çalıştırmak:

![image](https://user-images.githubusercontent.com/261946/70341877-f38b2480-1864-11ea-9922-543d292396cc.png)

## DEPRECATING (Eski versiyonu raftan indirmek)
6.0 versiyonunu artık kapattığımızı düşünelim. Yinede bu versiyona gelen talepleri `UnsupportedApiVersion` 
mesajıyla göstermek yerine 7.0 versiyonuyla karşılayalım. Ama diyelimki şu, şu versiyonları kullanabilirsin.
> ![image](https://user-images.githubusercontent.com/261946/70339010-1286b800-185f-11ea-8e47-a0a41367e398.png)

Aynı anda 6.0 versiyonunu bir denetleyicide `[ApiVersion("6.0")]` ile gösterir, 
örneğin 7.0 versiyonlu denetleyicide `[ApiVersion("6.0", Deprecated=true)]` diye yazarsak hata alırız.
> ![image](https://user-images.githubusercontent.com/261946/70339538-0f3ffc00-1860-11ea-9699-b84f4d44baa0.png)
 
> Bu yüzden aynı api versiyonunu sadece bir denetleyici üstünde tanımlı bırakmalıyız.

###### Artık koduna bakabiliriz:

```csharp
using Microsoft.AspNetCore.Mvc;

namespace Asp.Net.Core.Web.Api.Template.With.Swagger.Controllers
{
    //[ApiVersion("6.0")]
    //[Route("api/v{version:apiVersion}/versioning")]
    //[ApiController]
    //public class Versioning_Deprecating_Controller_v6 : ControllerBase
    //{
    //    [HttpGet]
    //    public IActionResult Get() => Ok("URL Path Segment - version 6.0");
    //}

    [ApiVersion("6.1")]
    [Route("api/v{version:apiVersion}/versioning")]
    [ApiController]
    public class Versioning_Deprecating_Controller_v61 : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() => Ok("URL Path Segment - version 6.1");
    }

    [ApiVersion("6.2")]
    [Route("api/v{version:apiVersion}/versioning")]
    [ApiController]
    public class Versioning_Deprecating_Controller_v62 : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() => Ok("URL Path Segment - version 6.2");
    }

    /*
     * Deprecated özelliğiyle 6.0 versiyonunun kaldırıldığını yerine 
     * 6.1, 6.2, 7.0'ın kullanılabileceğini bildiriyoruz.
     */
    [ApiVersion("6.0", Deprecated = true)]
    [ApiVersion("7.0")]
    [Route("api/v{version:apiVersion}/versioning")]
    [ApiController]
    public class Versioning_Deprecating_Controller_v2 : ControllerBase
    {
        [HttpGet]
        public IActionResult Get(ApiVersion version)
        {
            return Ok(new
            {
                Method = "URL Path Segment",
                Requested_Version = version,
                Description = "7.0 için evet ama 6.0'a el salla!"
            });
        }
    }
}
```

Özetle: Bir API versiyonunu eğer kullanılmayacak diye işaretlemek isterseniz (Deprecating):

    [ApiVersion( "2.0" )]
    [ApiVersion( "1.0", Deprecated = true )]