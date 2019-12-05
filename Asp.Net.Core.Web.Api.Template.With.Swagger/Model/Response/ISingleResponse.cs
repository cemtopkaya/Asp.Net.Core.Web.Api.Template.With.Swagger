namespace Asp.Net.Core.Web.Api.Template.With.Swagger.Model.Response
{
    using System;

    public interface ISingleResponse<TModel> : IResponse
    {
        TModel Model { get; set; }
    }
}
