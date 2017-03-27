var NombreAplicacion = "Grupo Nazan - Mensajes";

function msgEliminar(vlNombre) {

    var box = bootbox.dialog({
        title: "<i class=" + String.fromCharCode(34) + "fa fa-times-circle fa-2x" + String.fromCharCode(34) + "></i>&nbsp;" + NombreAplicacion,
        message: "<br> Esta seguro que desea Eliminar el Registro [ <b>" + vlNombre + "</b> ] ? <br><br>",
        buttons: {
            success: {
                label: "Eliminar",
                className: "msgbtn-danger",
                callback: function () {

                    return 1;

                }
            },
            main: {
                label: "Cancelar",
                className: "msgbtn-danger active",
                callback: function () {

                    return 0;


                }
            }
        }
    });


    box.find('.modal-header').addClass("msgDanger-title"); // Aqui cambio el color del Header
    box.find('.modal-content').addClass("msgDanger"); // Aqui Cambio el Color del formulario
    box.show();

}

function msgEditar(vlNombre) {
    var box = bootbox.dialog({
        title: "<i class=" + String.fromCharCode(34) + "fa fa-pencil-square fa-2x" + String.fromCharCode(34) + "></i>&nbsp;" + NombreAplicacion,
        message: "<br> Esta seguro que desea Editar el Registro [ <b>" + vlNombre + "</b> ] ? <br><br>",
        buttons: {
            success: {
                label: "Editar",
                className: "msgbtn-info",
                callback: function () {

                    return 1;

                }
            },
            main: {
                label: "Cancelar",
                className: "msgbtn-info active",
                callback: function () {

                    return 0;


                }
            }
        }
    });


    box.find('.modal-header').addClass("msgInfo-title"); // Aqui cambio el color del Header
    box.find('.modal-content').addClass("msgInfo"); // Aqui Cambio el Color del formulario
    box.show();


}


function msgExito(vlMensaje) {

    vlMensaje = vlMensaje || "La Operación se realizo con exito.";

    var box = bootbox.dialog({
        title: "<i class=" + String.fromCharCode(34) + "fa fa-check-circle fa-2x" + String.fromCharCode(34) + "></i>&nbsp;" + NombreAplicacion,
        message: "<br> <b>" + vlMensaje + "</b> <br><br>",
        buttons: {
            success: {
                label: "Aceptar",
                className: "msgbtn-success",
                callback: function () {

                    return 0;

                }
            }
        }
    });


    box.find('.modal-header').addClass("msgSuccess-title"); // Aqui cambio el color del Header
    box.find('.modal-content').addClass("msgSuccess"); // Aqui Cambio el Color del formulario
    box.show();


}

function msgError(vlMensaje) {

    var box = bootbox.dialog({
        title: "<i class=" + String.fromCharCode(34) + "fa fa-times-circle fa-2x" + String.fromCharCode(34) + "></i>&nbsp;" + NombreAplicacion,
        message: "<br> <b>" + vlMensaje + "</b> <br><br>",
        buttons: {
            success: {
                label: "Aceptar",
                className: "msgbtn-danger",
                callback: function () {

                    return 1;

                }
            }
        }
    });


    box.find('.modal-header').addClass("msgDanger-title"); // Aqui cambio el color del Header
    box.find('.modal-content').addClass("msgDanger"); // Aqui Cambio el Color del formulario
    box.show();

}

function msgAdvertencia(vlMensaje) {

    var box = bootbox.dialog({
        title: "<i class=" + String.fromCharCode(34) + "fa fa-exclamation-triangle fa-2x" + String.fromCharCode(34) + "></i>&nbsp;" + NombreAplicacion,
        message: "<br> <b>" + vlMensaje + "</b> <br><br>",
        buttons: {
            success: {
                label: "Aceptar",
                className: "msgbtn-warning",
                callback: function () {

                    return 1;

                }
            },
            main: {
                label: "Cancelar",
                className: "msgbtn-warning active",
                callback: function () {

                    return 0;


                }
            }
        }
    });


    box.find('.modal-header').addClass("msgWarning-title"); // Aqui cambio el color del Header
    box.find('.modal-content').addClass("msgWarning"); // Aqui Cambio el Color del formulario
    box.show();


}


function fncmsgbox(Mensaje, Titulo, Tipo, Iconos, Botones) {

    Tipo = Tipo || "default";
    Iconos = Iconos || false;
    Botones = Botones || 1;
   

/*

Mensaje = Contenido del Mensaje para la Pantalla
Titulo = Titulo del Mensaje
Tipo:
    danger = Mensaje Rojo (Con dos Botones)
    warning = Mensaje Amarillo (Con dos Botones) 
    info = Mensaje Azul (Con dos Botones)
    success = Mensaje Verde (Un Boton)
    default = Mensaje por defecto

*/

    var vlIconos = "";
    var vlColorMensaje = "";
    var vlColorTitulo = "";
    var vlColorBotonNormal = "";
    var vlColorBotonActivo = "";
    var vlTituloMensaje = "";


    if (Tipo == "danger") 
    {
        vlIconos = "fa fa-times-circle fa-2x";
        vlColorTitulo = "msgDanger-title";
        vlColorMensaje = "msgDanger";
        vlColorBotonNormal = "msgbtn-danger";
        vlColorBotonActivo = "msgbtn-danger active";
    }

    if (Tipo == "warning") {
        vlIconos = "fa fa-exclamation-triangle fa-2x";
        vlColorTitulo = "msgWarning-title";
        vlColorMensaje = "msgWarning";
        vlColorBotonNormal = "msgbtn-warning";
        vlColorBotonActivo = "msgbtn-warning active";
    }

    if (Tipo == "info") {
        vlIconos = "fa fa-info-circle fa-2x";
        vlColorTitulo = "msgInfo-title";
        vlColorMensaje = "msgInfo";
        vlColorBotonNormal = "msgbtn-info";
        vlColorBotonActivo = "msgbtn-info active";
    }

    if (Tipo == "question") {
        vlIconos = "fa fa-question-circle fa-2x";
        vlColorTitulo = "msgDefault-title";
        vlColorMensaje = "msgDefault";
        vlColorBotonNormal = "msgbtn-default";
        vlColorBotonActivo = "msgbtn-default active";
    }
        
    if (Tipo == "success") {
        vlIconos = "fa fa-times-circle fa-2x";
        vlColorTitulo = "msgSuccess-title";
        vlColorMensaje = "msgSuccess";
        vlColorBotonNormal = "msgbtn-success";
        vlColorBotonActivo = "msgbtn-success active";
    }

    if (Tipo == "default") {
        vlIconos = "fa fa-cogs fa-2x";
        vlColorTitulo = "msgDefault-title";
        vlColorMensaje = "msgDefault";
        vlColorBotonNormal = "msgbtn-default";
        vlColorBotonActivo = "msgbtn-default active";
    }

    if (Tipo == "default2") {
        vlIconos = "fa fa-info-circle fa-2x";
        vlColorTitulo = "msgInfo-title";
        vlColorMensaje = "msgDefault";
        vlColorBotonNormal = "msgbtn-info";
        vlColorBotonActivo = "msgbtn-info active";
    }

    if (Iconos == true) {
        vlTituloMensaje = "<i class=" + String.fromCharCode(34) + vlIconos + String.fromCharCode(34) + "></i>&nbsp;" + Titulo
    }

    if (Iconos == false) {
        vlTituloMensaje = Titulo
    }

    if (Botones == 1) {
        var box = bootbox.dialog({
            title: vlTituloMensaje,
            message: "<br> <b>" + Mensaje + "</b> <br><br>",
            buttons: {
                aceptar: {
                    label: "Aceptar",
                    className: vlColorBotonNormal,
                    callback: function () {
                        //alert(0);
                        return 0;

                    }
                }
            }
        });
    }


    if (Botones == 2) {
        var box = bootbox.dialog({
            title: vlTituloMensaje,
            message: "<br> <b>" + Mensaje + "</b> <br><br>",
            buttons: {
                aceptar: {
                    label: "Aceptar",
                    className: vlColorBotonNormal,
                    callback: function () {
                        //alert(1);
                        return 1;

                    }
                },
                cancelar: {
                    label: "Cancelar",
                    className: vlColorBotonActivo,
                    callback: function () {
                        //alert(0);
                        return 0;

                    }
                }
            }
        });
    }


    box.find('.modal-header').addClass(vlColorTitulo); // Aqui cambio el color del Header
    box.find('.modal-content').addClass(vlColorMensaje); // Aqui Cambio el Color del formulario
    box.show();

    //return box;


}

//**********************************************************************************
// Esta funcion NO Necesita la Libreria BootBox
//**********************************************************************************
function msgboxProcesando() {

    //********************************************************************
    //Funcion Para Mostrar Mensajes de (Cargando, Procesando, Esperando)
    //********************************************************************
    // Forma de Utilizar
    //********************************************************************
    // En la pagina que desea utilizar el mensaje debe hacer lo siguiente
    // 
    // Declarar una variable ejemplo: [ msgProcesando ]
    //
    // Quedaria en la pagina asi: var msgProcesando = msgboxProcesando();
    //
    // Y utilizaria algunas de las siguientes Opciones:
    //--------------------------------------------------------------------
    // Opcion 1 - Mensaje Estandar: msgProcesando.show(); 
    // Opcion 2 - Mensaje con Titulo Personalizado : msgProcesando.show('Texto del Mensaje');
    // Opcion 3 - Mensaje con Titulo Personalizado y Color de la barra de progreso: msgProcesando.show('Custom message', {dialogSize: 'sm', progressType: 'warning'});
    //--------------------------------------------------------------------
    // Cerrar Mensaje: msgProcesando.hide();
    //********************************************************************


    var msgProcesando = (function () {

        // Creating modal dialog's DOM
        var $dialog = $(
            		'<div class="modal fade" data-backdrop="static" data-keyboard="false" tabindex="-1" role="dialog" aria-hidden="true" style="padding-top:15%; overflow-y:visible;">' +
            		'<div class="modal-dialog modal-m">' +
            		'<div class="modal-content">' +
            			'<div class="modal-header"><h3 style="margin:0;"></h3></div>' +
            			'<div class="modal-body">' +
            				'<div class="progress progress-striped active" style="margin-bottom:0;"><div class="progress-bar" style="width: 100%"></div></div>' +
            			'</div>' +
            		'</div></div></div>');

        return {
            /**
            * Opens our dialog
            * @param message Custom message
            * @param options Custom options:
            * 				  options.dialogSize - bootstrap postfix for dialog size, e.g. "sm", "m";
            * 				  options.progressType - bootstrap postfix for progress bar type, e.g. "success", "warning".
            */
            show: function (message, options) {
                // Assigning defaults
                var settings = $.extend({
                    dialogSize: 'm',
                    progressType: ''
                }, options);
                if (typeof message === 'undefined') {
                    message = 'Procesando...';
                }
                if (typeof options === 'undefined') {
                    options = {};
                }
                // Configuring dialog
                $dialog.find('.modal-dialog').attr('class', 'modal-dialog').addClass('modal-' + settings.dialogSize);
                $dialog.find('.progress-bar').attr('class', 'progress-bar');
                if (settings.progressType) {
                    $dialog.find('.progress-bar').addClass('progress-bar-' + settings.progressType);
                }
                $dialog.find('h3').text(message);
                // Opening dialog
                $dialog.modal();
            },
            /**
            * Closes dialog
            */
            hide: function () {
                $dialog.modal('hide');
            }
        }

    })();
    //********************************************************************

    return msgProcesando;

}
//*********************************************************************************
// FIN DE LA FUNCION msgboxProcesando
//*********************************************************************************


//**********************************************************************************
// Esta funcion NO Necesita la Libreria BootBox
//**********************************************************************************
function msgboxProcesandoCorreo() {

    //********************************************************************
    //Funcion Para Mostrar Mensajes de (Cargando, Procesando, Esperando)
    //********************************************************************
    // Forma de Utilizar
    //********************************************************************
    // En la pagina que desea utilizar el mensaje debe hacer lo siguiente
    // 
    // Declarar una variable ejemplo: [ msgProcesando ]
    //
    // Quedaria en la pagina asi: var msgProcesando = msgboxProcesando();
    //
    // Y utilizaria algunas de las siguientes Opciones:
    //--------------------------------------------------------------------
    // Opcion 1 - Mensaje Estandar: msgProcesando.show(); 
    // Opcion 2 - Mensaje con Titulo Personalizado : msgProcesando.show('Texto del Mensaje');
    // Opcion 3 - Mensaje con Titulo Personalizado y Color de la barra de progreso: msgProcesando.show('Custom message', {dialogSize: 'sm', progressType: 'warning'});
    //--------------------------------------------------------------------
    // Cerrar Mensaje: msgProcesando.hide();
    //********************************************************************


    var msgProcesandoCorreo = (function () {

        // Creating modal dialog's DOM
        var $dialog = $(
            		'<div class="modal fade" data-backdrop="static" data-keyboard="false" tabindex="-1" role="dialog" aria-hidden="true" style="padding-top:15%; overflow-y:visible;">' +
            		'<div class="modal-dialog modal-m">' +
            		'<div class="modal-content">' +
            			'<div class="modal-header"><h3 style="margin:0;"></h3></div>' +
            			'<div class="modal-body">' +
            				'<div class="progress progress-striped active" style="margin-bottom:0;"><div class="progress-bar" style="width: 100%"></div></div>' +
            			'</div>' +
            		'</div></div></div>');

        return {
            /**
            * Opens our dialog
            * @param message Custom message
            * @param options Custom options:
            * 				  options.dialogSize - bootstrap postfix for dialog size, e.g. "sm", "m";
            * 				  options.progressType - bootstrap postfix for progress bar type, e.g. "success", "warning".
            */
            show: function (message, options) {
                // Assigning defaults
                var settings = $.extend({
                    dialogSize: 'm',
                    progressType: ''
                }, options);
                if (typeof message === 'undefined') {
                    message = 'Enviando correo...';
                }
                if (typeof options === 'undefined') {
                    options = {};
                }
                // Configuring dialog
                $dialog.find('.modal-dialog').attr('class', 'modal-dialog').addClass('modal-' + settings.dialogSize);
                $dialog.find('.progress-bar').attr('class', 'progress-bar');
                if (settings.progressType) {
                    $dialog.find('.progress-bar').addClass('progress-bar-' + settings.progressType);
                }
                $dialog.find('h3').text(message);
                // Opening dialog
                $dialog.modal();
            },
            /**
            * Closes dialog
            */
            hide: function () {
                $dialog.modal('hide');
            }
        }

    })();
    //********************************************************************

    return msgProcesandoCorreo;

}
//*********************************************************************************
// FIN DE LA FUNCION msgboxProcesando
//*********************************************************************************