namespace Asp.Net.Core.Web.Api.Template.With.Swagger.Model.Response
{
    using System;

    public interface IPagedResponse<TModel> : IListResponse<TModel>, IResponse
    {
        int ItemsCount { get; set; }

        double PageCount { get; }
    }
}
