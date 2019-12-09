using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Asp.Net.Core.Web.Api.Template.With.Swagger.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class JsonWebTokenAuthController : ControllerBase {
        private readonly IConfiguration config;

        public JsonWebTokenAuthController(IConfiguration config) {
            this.config = config;
        }

        [HttpPost("token")]
        public ActionResult GetToken([FromBody] Model.Login login) {
            if (!login.UserName.Equals("petro") && !login.Password.Equals("net")) {
                return this.BadRequest("Hatali kullanici bilgileri!");
            }
            TokenSettings instance = new TokenSettings();
            this.config.Bind("JwtSettings", instance);
            string secret = instance.Secret;
            var credentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(instance.IssuerSigningKey, "http://www.w3.org/2001/04/xmldsig-more#hmac-sha256");
            var list1 = new List<System.Security.Claims.Claim>();
            list1.Add(new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Yönetici"));
            list1.Add(new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Servis Görevlisi"));
            list1.Add(new Claim("Yetkiler", "Servis:C,R,U,D;Dosya:C,R"));
            DateTime? nullable = new DateTime?(DateTime.Now.AddMinutes(1.0));
            DateTime? nullable2 = null;
            var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(instance.Issuer, instance.Audience, (IEnumerable<Claim>)list1, nullable2, nullable, credentials);
            return this.Ok(new JwtSecurityTokenHandler().WriteToken(token));
        }
    }
}
