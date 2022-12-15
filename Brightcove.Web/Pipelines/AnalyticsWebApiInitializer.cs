using System.Web.Http;
using Sitecore.Pipelines;

namespace Brightcove.Web.Pipelines
{
    public class AnalyticsWebApiInitializer
    {
        public void Process(PipelineArgs args)
        {
            GlobalConfiguration.Configure(Configure);
        }

        protected void Configure(HttpConfiguration configuration)
        {
            var routes = configuration.Routes;
            routes.MapHttpRoute(
              "MFReports",
              "sitecore/api/mediaframework/mfreports/{datasource}/{sitename}",
              new
              {
                  controller = "MFReports",
                  action = "Get"
              });
        }
    }
}