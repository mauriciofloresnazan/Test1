using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Ppgz.Repository;
using Ppgz.Web.Models;

namespace Ppgz.Web.Controllers
{
    [Authorize]
	public class ProveedorMensajesInstitucionalesController : Controller
	{
		private readonly PpgzEntities _db = new PpgzEntities();

		//
		// GET: /ProveedorMensajesInstitucionales/
		public ActionResult Index()
		{
			var mensajes = _db.mensajes.ToList();

		    var id = Int32.Parse(User.Identity.GetUserId());

            var mensajesUsuario = _db.usuario_mensajes.Where(u => u.usuario_id == id).ToList();

            //var test = _db.usuario_mensajes.FirstOrDefault(u => u.usuario_id == id);


            ViewBag.mensajes = mensajes;




            ViewBag.mensajesUsuario = Json(mensajesUsuario);

			return View();
		}

        [HttpPost]
        public ActionResult Visualizar(int? id)
        {
            var mensaje = _db.mensajes.Single(i => i.id == id);


            var usuarioId = Int32.Parse(User.Identity.GetUserId());

            if (_db.usuario_mensajes.Any(i => i.mensaje_id == id && i.usuario_id == usuarioId))
            {
                return Content("Actualizado"); ;
            }

            var usuarioMensaje = new usuario_mensajes
            {
                mensaje_id = mensaje.id,
                usuario_id = int.Parse(User.Identity.GetUserId())
            };

            _db.usuario_mensajes.Add(usuarioMensaje);

            _db.SaveChanges();

            return Content("Actualizado");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Leido(int id)
        {

            var mensaje = _db.mensajes.Single(i => i.id == id);
                
        
            if(_db.usuario_mensajes.Any(i => i.mensaje_id == id && i.usuario_id ==int.Parse(User.Identity.GetUserId())))
            {
               return Content("Leido");;
            }

            var usuarioMensaje = new usuario_mensajes
            {
                mensaje_id = mensaje.id,
                usuario_id = int.Parse(User.Identity.GetUserId())
            };

            _db.usuario_mensajes.Add(usuarioMensaje);

            _db.SaveChanges();

            return Content("Leido");
        }
	}
}