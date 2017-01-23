using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Ppgz.Mvc.Startup))]
namespace Ppgz.Mvc
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
