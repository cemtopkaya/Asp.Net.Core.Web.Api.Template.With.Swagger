namespace Asp.Net.Core.Web.Api.Template.With.Swagger.Model.Response
{
    using System;
    using System.Collections.Generic;

    public interface IListResponse<TModel> : IResponse
    {
        IEnumerable<TModel> Model { get; set; }
    }
}
