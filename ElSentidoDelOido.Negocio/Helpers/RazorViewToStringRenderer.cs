using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;

namespace ElSentidoDelOido.Negocio.Helpers
{
    public class RazorViewToStringRenderer : IRazorViewToStringRenderer
    {
        private readonly IRazorViewEngine _viewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IServiceProvider _serviceProvider;

        public RazorViewToStringRenderer(IRazorViewEngine viewEngine, ITempDataProvider tempDataProvider, IServiceProvider serviceProvider)
        {
            _viewEngine = viewEngine;
            _tempDataProvider = tempDataProvider;
            _serviceProvider = serviceProvider;
        }

        public async Task<string> RenderViewToStringAsync<TModel>(string viewPath, TModel model)
        {
            var httpContext = new DefaultHttpContext { RequestServices = _serviceProvider };
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

            var viewResult = _viewEngine.FindView(actionContext, viewPath, isMainPage: false);
            if (!viewResult.Success)
            {
                viewResult = _viewEngine.GetView(executingFilePath: null, viewPath: viewPath, isMainPage: false);
                if (!viewResult.Success)
                {
                    var searched = string.Join(Environment.NewLine, viewResult?.SearchedLocations ?? Array.Empty<string>());
                    throw new InvalidOperationException($"No se encontró la vista '{viewPath}'. Buscados:{Environment.NewLine}{searched}");
                }
            }

            var view = viewResult.View;

            await using var sw = new StringWriter();
            var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary()) { Model = model };
            var tempData = new TempDataDictionary(actionContext.HttpContext, _tempDataProvider);

            var viewContext = new ViewContext(actionContext, view, viewData, tempData, sw, new HtmlHelperOptions());

            await view.RenderAsync(viewContext);
            return sw.ToString();
        }
    }
}
