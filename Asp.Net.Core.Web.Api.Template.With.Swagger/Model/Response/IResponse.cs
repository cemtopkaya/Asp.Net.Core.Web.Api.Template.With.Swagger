namespace Asp.Net.Core.Web.Api.Template.With.Swagger.Model.Response
{
    using System;

    public interface IResponse
    {
        string Message { get; set; }

        bool DidError { get; set; }

        string ErrorMessage { get; set; }
    }
}
