namespace Asp.Net.Core.Web.Api.Template.With.Swagger.Model.Response
{
    using System;
    using System.Runtime.CompilerServices;

    public class Response : IResponse
    {
        public string Message { get; set; }

        public bool DidError { get; set; }

        public string ErrorMessage { get; set; }
    }
}
