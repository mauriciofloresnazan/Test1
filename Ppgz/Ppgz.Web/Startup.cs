using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Ppgz.Web.Startup))]
namespace Ppgz.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
