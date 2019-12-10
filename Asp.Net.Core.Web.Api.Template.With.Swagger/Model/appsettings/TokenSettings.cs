using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Asp.Net.Core.Web.Api.Template.With.Swagger.Model.appsettings {
    public class TokenSettings {
        public string Secret { get; set; }

        public string Issuer { get; set; }

        public string Audience { get; set; }

        public int ExpirationTime { get; set; }

        public string UygulamaAdi { get; set; }

        public SymmetricSecurityKey IssuerSigningKey =>
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.Secret));
    }
}
