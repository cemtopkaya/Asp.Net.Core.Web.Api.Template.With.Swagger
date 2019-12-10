using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Asp.Net.Core.Web.Api.Template.With.Swagger.Model.appsettings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Asp.Net.Core.Web.Api.Template.With.Swagger.Controllers {

    [Authorize]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class JsonWebTokenAuthController : ControllerBase {
        private readonly IConfiguration config;

        public JsonWebTokenAuthController(IConfiguration config) {
            this.config = config;
        }


        /// <summary>
        /// Token alabilmek için kullanılır
        /// </summary>
        /// <remarks>
        /// <code>
        /// {
        ///   "UserName":"petro",
        ///   "Password":"net"
        /// }
        /// </code>
        /// </remarks>
        /// <param name="login">Kullanıcı adı ve şifresi</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("token")]
        public ActionResult GetToken([FromBody] Model.Login login) {
            System.IdentityModel.Tokens.Jwt.JwtSecurityToken token;
            Model.appsettings.TokenSettings instance;
            List<System.Security.Claims.Claim> claims;
            Microsoft.IdentityModel.Tokens.SigningCredentials credentials;
            // Token şimdiden ne kadar zaman sonra geçerli olsun. Hemen geçerli olsun diye null.
            DateTime? notWorkBeforeDate = null;
            // Token'ın kullanım süresini belirler. Daima kullanılsın diye null set edilebilir.
            DateTime? expirationDate = DateTime.Now.AddDays(7);

            if (!login.UserName.Equals("petro") && !login.Password.Equals("net")) {
                return this.BadRequest("Hatali kullanici bilgileri!");
            }

            // appsettings.json 'dan ilgili ayarları çekelim
            instance = new TokenSettings();
            this.config.Bind("JwtSettings", instance);

            // Oluşan token bilgisini HMAC-SHA256 ile şifreleyeceğiz
            credentials = new SigningCredentials(instance.IssuerSigningKey, "http://www.w3.org/2001/04/xmldsig-more#hmac-sha256");
            
            // Token ile göndereceğimiz bilgileri ayarlayalım
            claims = new List<Claim> {
                new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Yönetici"),
                new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Servis Görevlisi"),
                // Uygulamaya özgü bir özellik eklerken:
                new Claim("Yetkiler", "Servis:C,R,U,D;Dosya:C,R"),
                // ister "xml schema" bilgisiyle ister bu bilgiyle eşleşmi ClaimTypes.xxx tipinin statik özellikleriyle
                new Claim(ClaimTypes.Country, "Türkiye"),
                new Claim(ClaimTypes.GivenName, "Cem Topkaya")
            };

            var tokenDescriptor = new SecurityTokenDescriptor {
                Audience = instance.Audience,
                Expires = expirationDate,
                IssuedAt = DateTime.Now,
                Issuer = instance.Issuer,
                NotBefore = null, // null verdiğimiz için DateTime.Now değerini yazacak
                SigningCredentials = credentials,
                Subject = new ClaimsIdentity(claims)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            token = tokenHandler.CreateJwtSecurityToken(tokenDescriptor);
            return Ok(new { login, Token = tokenHandler.WriteToken(token) });


            token = new JwtSecurityToken(
                instance.Issuer,
                instance.Audience,
                (IEnumerable<Claim>)claims,
                notWorkBeforeDate,
                expirationDate,
                credentials);

            return this.Ok(new JwtSecurityTokenHandler().WriteToken(token));
        }

        /// <summary>
        /// Token doğrulama
        /// </summary>
        /// <response code="401">Yetkisiz erişim!</response>
        /// <returns></returns>
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [HttpGet("authorizedAction")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public IActionResult GetData() {
            return Ok(new { a = "A", b = "B" });
        }


    }
}
