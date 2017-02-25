using System.Web.Mvc;

namespace Ppgz.Web.Areas.Mercaderia
{
    public class MercaderiaAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Mercaderia";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Mercaderia_default",
                "Mercaderia/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}