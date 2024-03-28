using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using Drones.Models;
using System.Net.Mail;
using System.Net;
namespace Drones.Controllers
{
    public class HomeController : Controller
    {
        private string connectionString = LibroV.connectionString;
       

        // GET: Home
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Home()
        {
            return View();
        }

        //Preguntas de Seguridad del registro

        public ActionResult Registrar()
        {
            // Obtener las preguntas de seguridad
            var preguntasSeguridad = ObtenerPreguntasSeguridad();

            // Crear un nuevo usuario con las preguntas de seguridad
            var usuario = new Usuario
            {
                PreguntasSeguridad = preguntasSeguridad
            };

            return View(usuario);
        }


        // Método para obtener las preguntas de seguridad desde la base de datos
        public List<PreguntaSeguridad> ObtenerPreguntasSeguridad()
        {
            List<PreguntaSeguridad> preguntas = new List<PreguntaSeguridad>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand cmd = new SqlCommand("MostrarPreguntasSeguridad", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            preguntas.Add(new PreguntaSeguridad
                            {
                                PreguntaID = Convert.ToInt32(reader["PreguntaID"]),
                                Pregunta = reader["Pregunta"].ToString()
                            });
                        }
                    }
                }
            }

            ViewBag.Preguntas = new SelectList(preguntas, "PreguntaID", "Pregunta");
            return preguntas;
        }


        //Registrar el usuario
        public ActionResult CargarDatos()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CargarDatos(Usuario usuario, string provincia, string canton, string distrito)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Verificar la existencia del registro antes de realizar el registro
                    LibroV libro = new LibroV();

                    // Llamada al método para registrar auditoría
                    Auditoria.RegistrarAuditoria(usuario.Identificacion, "registro", connectionString);

                    // Llamada al procedimiento almacenado para realizar el registro
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();

                        using (SqlCommand cmd = new SqlCommand("InsertRegistro", connection))
                        {
                            cmd.CommandType = System.Data.CommandType.StoredProcedure;

                            // Parámetros del procedimiento almacenado
                            cmd.Parameters.AddWithValue("@Identificacion", usuario.Identificacion);
                            cmd.Parameters.AddWithValue("@Nombre", usuario.Nombre);
                            cmd.Parameters.AddWithValue("@Apellido", usuario.Apellido);
                            cmd.Parameters.AddWithValue("@Residencia", usuario.Residencia);
                            cmd.Parameters.AddWithValue("@Telefono", usuario.Telefono);
                            cmd.Parameters.AddWithValue("@Correo", usuario.Correo);
                            cmd.Parameters.AddWithValue("@Contraseña", usuario.Contraseña);
                            cmd.Parameters.AddWithValue("@Pregunta1", usuario.Pregunta1);
                            cmd.Parameters.AddWithValue("@Respuesta1", usuario.Respuesta1);
                            cmd.Parameters.AddWithValue("@Pregunta2", usuario.Pregunta2);
                            cmd.Parameters.AddWithValue("@Respuesta2", usuario.Respuesta2);
                            cmd.Parameters.AddWithValue("@Pregunta3", usuario.Pregunta3);
                            cmd.Parameters.AddWithValue("@Respuesta3", usuario.Respuesta3);
                            cmd.Parameters.AddWithValue("@Provincia", provincia);
                            cmd.Parameters.AddWithValue("@Canton", canton);
                            cmd.Parameters.AddWithValue("@Distrito", distrito);

                            cmd.ExecuteNonQuery();
                        }
                    }

                    EnviarCorreoDeConfirmacion(usuario.Correo);

                    // Redireccionar a una vista de éxito o cualquier otra acción que desees después de registrar al usuario
                    return RedirectToAction("CargarDatos");
                }
                else
                {
                    // Los datos del modelo no son válidos, muestra el formulario con errores
                    return View("ErrorRegistro");
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627)
                {
                    return View("ErrorRegistro");
                }
                else
                {
                    return View("ErrorRegistro");
                }
            }
            catch (Exception ex)
            {
                return View("ErrorRegistro");
            }
        }

        //Ajax provincias
        public ActionResult ObtenerProvincias()
        {
            List<string> provincias = new List<string>();
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string procedimientoAlmacenado = "ObtenerProvinciasDesdeBD";
                    using (SqlCommand command = new SqlCommand(procedimientoAlmacenado, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            provincias.Add(reader["nombre"].ToString());
                        }
                        reader.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                // Manejar la excepción aquí según sea necesario
            }

            return Json(provincias, JsonRequestBehavior.AllowGet);
        }


        public ActionResult ObtenerCantones(string provincia)
        {
            List<string> cantones = new List<string>();
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string procedimientoAlmacenado = "ObtenerCantonesDesdeBD";
                    using (SqlCommand command = new SqlCommand(procedimientoAlmacenado, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@Provincia", provincia);
                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            cantones.Add(reader["nombre"].ToString());
                        }
                        reader.Close();
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return Json(cantones, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ObtenerDistritos(string provincia, string canton)
        {
            List<string> distritos = new List<string>();
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string procedimientoAlmacenado = "ObtenerDistritosDesdeBD";
                    using (SqlCommand command = new SqlCommand(procedimientoAlmacenado, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@Provincia", provincia);
                        command.Parameters.AddWithValue("@Canton", canton);
                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            distritos.Add(reader["nombre"].ToString());
                        }
                        reader.Close();
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return Json(distritos, JsonRequestBehavior.AllowGet);
        }

        //Obtener la identificacion del usuario
        private int ObtenerIdentificacionUsuario()
        {

            if (Session["UsuarioIdentificacion"] != null)
            {

                return Convert.ToInt32(Session["UsuarioIdentificacion"]);
            }
            else
            {

                return -1;
            }
        }

        //Validar Login
        [HttpPost]
        public ActionResult Validarlogin()
        {

            string identificacionS = Request.Form["identificacion"]?.ToString();

            if (string.IsNullOrEmpty(identificacionS) || !int.TryParse(identificacionS, out int identificacion))
            {

                ViewBag.MensajeError = "Identificación inválida";
                return View("ErrorInicioSesion");
            }

            string contrasenaEncriptada = Request.Form["contrasena"]?.ToString();

            if (string.IsNullOrEmpty(contrasenaEncriptada))
            {

                ViewBag.MensajeError = "Contraseña inválida";
                return View("ErrorInicioSesion");
            }


            bool credencialesValidas = ValidarCredenciales(identificacion, contrasenaEncriptada);

            if (credencialesValidas)
            {


                EnviarCodigoDeConfirmacion(identificacion);


                Session["UsuarioIdentificacion"] = identificacion;


                Auditoria.RegistrarAuditoria(identificacion, "Inicio de sesión exitoso", connectionString);

                return View("Token");
            }
            else
            {

                IncrementarIntentosFallidos();
                string mensajeBloqueo = ObtenerMensajeUsuarioBloqueado();
                if (!string.IsNullOrEmpty(mensajeBloqueo))
                {
                    ViewBag.MensajeError = mensajeBloqueo;
                    return View("UsuarioBloqueado");
                }


                Auditoria.RegistrarAuditoria(identificacion, "Intento de inicio de sesión fallido", connectionString);
            }


            ViewBag.MensajeError = "Credenciales inválidas";
            return View("ErrorInicioSesion");
        }


        private bool ValidarCredenciales(int identificacion, string contrasenaEncriptada)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand("ValidarCredenciales", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Identificacion", identificacion);
                    command.Parameters.AddWithValue("@Contraseña", contrasenaEncriptada);


                    SqlParameter resultadoParam = new SqlParameter("@Resultado", SqlDbType.Int);
                    resultadoParam.Direction = ParameterDirection.Output;
                    command.Parameters.Add(resultadoParam);


                    Auditoria.RegistrarAuditoria(identificacion, "Validación de credenciales", connectionString);


                    command.ExecuteNonQuery();


                    int resultado = Convert.ToInt32(resultadoParam.Value);

                    return resultado == 1;
                }
                catch (Exception ex)
                {

                    throw ex;
                }
            }
        }

        public ActionResult ErrorInicioSesion()
        {

            return View();
        }

        //Bloque del usuario por medio de los 3 intentos
        public ActionResult UsuarioBloqueado()
        {
            ActualizarTiempoRestante();

            // Obtener la identificación del usuario desde la sesión
            int identificacionUsuario = (int)Session["UsuarioIdentificacion"];

            // Llamada al método para registrar auditoría de usuario bloqueado
            Auditoria.RegistrarAuditoria(identificacionUsuario, "Usuario bloqueado", connectionString);

            return View();
        }
       
        private void IncrementarIntentosFallidos()
        {
            int intentosFallidos = (int)(Session["IntentosFallidos"] ?? 0);
            intentosFallidos++;
            Session["IntentosFallidos"] = intentosFallidos;

            if (intentosFallidos == 1)
            {

                Session["TiempoBloqueo"] = DateTime.Now.AddMinutes(1);
            }
            else if (intentosFallidos == 2)
            {

                Session["TiempoBloqueo"] = DateTime.Now.AddMinutes(20);
            }
            else if (intentosFallidos >= 3)
            {

                Session["TiempoBloqueo"] = DateTime.Now.AddHours(1);
            }
        }

        private string ObtenerMensajeUsuarioBloqueado()
        {
            DateTime? tiempoBloqueo = Session["TiempoBloqueo"] as DateTime?;

            if (tiempoBloqueo.HasValue && tiempoBloqueo > DateTime.Now)
            {
                TimeSpan tiempoRestante = tiempoBloqueo.Value - DateTime.Now;
                Session["TiempoRestante"] = tiempoRestante.ToString(@"mm\:ss");
                return $"Tu cuenta está bloqueada. Por favor, inténtalo de nuevo después de {tiempoRestante.ToString(@"mm\:ss")}.";
            }

            Session["TiempoRestante"] = null;
            return null;
        }

        private void ActualizarTiempoRestante()
        {
            DateTime? tiempoBloqueo = Session["TiempoBloqueo"] as DateTime?;

            if (tiempoBloqueo.HasValue && tiempoBloqueo > DateTime.Now)
            {
                TimeSpan tiempoRestante = tiempoBloqueo.Value - DateTime.Now;
                Session["TiempoRestante"] = tiempoRestante.ToString(@"mm\:ss");
            }
            else
            {
                Session["TiempoRestante"] = null;
            }
        }
        
        //Token
        public ActionResult Token(string codigoIngresado)
        {


            string codigoAlmacenado = Session["CodigoConfirmacion"] as string;

            if (codigoAlmacenado != null && codigoIngresado == codigoAlmacenado)
            {

                int identificacionUsuario = (int)Session["UsuarioIdentificacion"];


                Auditoria.RegistrarAuditoria(identificacionUsuario, "Ingreso del Token", connectionString);


                return RedirectToAction("Home", "Home");
            }
            else
            {

                int identificacionUsuario = (int)Session["UsuarioIdentificacion"];


                Auditoria.RegistrarAuditoria(identificacionUsuario, "Intento de inicio de sesión fallido en la página Token", connectionString);


                ViewBag.MensajeError = "El código ingresado es incorrecto. Por favor, inténtalo nuevamente.";
                return View("Token");
            }
        }

       
        //Numeros aletorios
        private int GenerarCodigoAleatorio()
        {
            Random random = new Random();
            return random.Next(1000, 10000);
        }


        //Correos
        private string ObtenerCorreoPorIdentificacion(int identificacion)
        {
            string correo = null;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand cmd = new SqlCommand("ObtenerCorreoPorIdentificacion", connection))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;


                    cmd.Parameters.AddWithValue("@Identificacion", identificacion);


                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        correo = reader["Correo"].ToString();
                    }
                }
            }

            return correo;
        }

        private void EnviarCodigoDeConfirmacion(int identificacion)
        {

            string correoDestino = ObtenerCorreoPorIdentificacion(identificacion);

            if (correoDestino != null)
            {

                int codigoAleatorio = GenerarCodigoAleatorio();


                string codigo = codigoAleatorio.ToString("D4");


                SmtpClient smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("gbarahonaq1708@gmail.com", "faoo mwhe weio lsnw"),
                    EnableSsl = true,
                };


                MailMessage mensaje = new MailMessage("gbarahonaq1708@gmail.com", correoDestino)
                {
                    Subject = "Código de confirmación",
                    Body = $"Querido Usuario:\n\nTu código de confirmación es: {codigo}. Úsalo para la confirmación de token.\n\nAtentamente,\nDrones Blue and White Robotics",
                };


                smtpClient.Send(mensaje);


                Session["CodigoConfirmacion"] = codigo;
            }
        }

        private void EnviarCorreoDeConfirmacion(string correoUsuario)
        {
            string asunto = "Confirmación de Registro";
            string cuerpo = $"Querido Cliente,\n\n" + $"\n\n" + $"\n\n" + $"Muchas gracias por haber completado nuestro formulario de registro. Esperamos que disfrute explorando nuestra página y encuentre los productos que necesita. Le recordamos algunas recomendaciones y sugerencias en caso de cualquier inconveniente.\n\n" + $"\n\n" + $"\n\n" + $"\n\n" + $"Después de haber ingresado su identificación y contraseña, se le enviará un token a su correo registrado para su validación. Una vez que ingrese el token, podrá acceder a nuestra página principal. Recuerde manejar con cuidado el token, ya que solo tiene 3 intentos. Después de estos intentos, se bloqueará. Lo mismo ocurrirá si ingresa incorrectamente su contraseña. Como recomendación, le pedimos que revise con atención cómo ingresa su contraseña y su token.\n\n" + $"\n\n" + $"\n\n" + $"Atentamente,\n" + $"\n\n" +
                    $"Drones Blue and White Robotics";

            using (MailMessage mensajeCorreo = new MailMessage("gbarahonaq1708@gmail.com", correoUsuario))
            {
                mensajeCorreo.Subject = asunto;
                mensajeCorreo.Body = cuerpo;
                mensajeCorreo.IsBodyHtml = true;
                using (SmtpClient clienteSmtp = new SmtpClient("smtp.gmail.com"))
                {
                    clienteSmtp.Port = 587;
                    clienteSmtp.UseDefaultCredentials = false;
                    clienteSmtp.Credentials = new NetworkCredential("gbarahonaq1708@gmail.com", "faoo mwhe weio lsnw");
                    clienteSmtp.EnableSsl = true;

                    clienteSmtp.Send(mensajeCorreo);
                }
            }
        }

        //Mostrar Productos
        public ActionResult Drones()
        {
            List<Drone> drones = GetDronesFromDatabase();
            return View(drones);
        }

        private List<Drone> GetDronesFromDatabase()
        {
            List<Drone> drones = new List<Drone>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("ObtenerProductosDrones", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Drone drone = new Drone
                            {
                                Tipo = Convert.ToString(reader["Tipo"]),
                                Modelo = Convert.ToString(reader["Modelo"]),
                                Descripcion = Convert.ToString(reader["Descripcion"]),
                                FichaTecnica = Convert.ToString(reader["Ficha_tecnica"]),
                                Precio = Convert.ToDecimal(reader["Precio"])
                            };

                            drones.Add(drone);
                        }
                    }
                }
            }

            return drones;
        }

        private string ObtenerNumeroSerieDesdeBD(string modelo)
        {
            string numeroSerie = string.Empty;

           
            string query = "SELECT Numero_Serie FROM Producto WHERE Modelo = @Modelo";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Modelo", modelo);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            numeroSerie = reader["Numero_Serie"].ToString();
                        }
                    }
                }
            }

            return numeroSerie;
        }

        public ActionResult Traktor()
        {
            List<Traktor> traktors = GetTraktorFromDatabase();
            return View(traktors);
        }
        private List<Traktor> GetTraktorFromDatabase()
        {
            List<Traktor> traktorsList = new List<Traktor>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("ObtenerProductosTraktor", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Traktor traktor = new Traktor
                            {
                                Tipo = Convert.ToString(reader["Tipo"]),
                                Modelo = Convert.ToString(reader["Modelo"]),
                                Descripcion = Convert.ToString(reader["Descripcion"]),
                                FichaTecnica = Convert.ToString(reader["Ficha_tecnica"]),
                                Precio = Convert.ToDecimal(reader["Precio"])
                            };

                            traktorsList.Add(traktor); 
                        }
                    }
                }
            }

            return traktorsList; 
        }
       
        //Carrito de compras
        public ActionResult Carrito()
        {
            List<Carrito> carritoItems = ObtenerCarritoDesdeBaseDatos();
            return View(carritoItems);
        }

        private List<Carrito> ObtenerCarritoDesdeBaseDatos()
        {
            int identificacionUsuario = ObtenerIdentificacionUsuario();
            if (identificacionUsuario == -1)
            {

                return new List<Carrito>();
            }

            List<Carrito> carritoItems = new List<Carrito>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();


                string query = "SELECT * FROM Carrito WHERE Identificacion = @Identificacion";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Identificacion", identificacionUsuario);
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        Carrito item = new Carrito
                        {
                            Numero_Serie = reader["Numero_Serie"].ToString(),
                            Modelo = reader["Modelo"].ToString(),
                            Cantidad = Convert.ToInt32(reader["Cantidad"]),
                            Precio = Convert.ToDecimal(reader["Precio"]),
                            Precio_Total = Convert.ToDecimal(reader["Precio_Total"])
                        };


                        carritoItems.Add(item);
                    }
                }
            }

            return carritoItems;
        }


        [HttpPost]
        public ActionResult ActualizarCantidad(string numeroSerie, int nuevaCantidad, decimal? nuevoPrecio)
        {
            int identificacionUsuario = ObtenerIdentificacionUsuario();
            if (identificacionUsuario == -1)
            {
               
                return RedirectToAction("Login"); 
            }

            decimal nuevoPrecioTotal = 0; 

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("ActualizarCantidadCarrito", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Numero_Serie", numeroSerie);
                    command.Parameters.AddWithValue("@NuevaCantidad", nuevaCantidad);
                    command.Parameters.AddWithValue("@Identificacion", identificacionUsuario);

                    SqlParameter nuevoPrecioTotalParam = new SqlParameter("@NuevoPrecioTotal", SqlDbType.Decimal);
                    nuevoPrecioTotalParam.Precision = 18;
                    nuevoPrecioTotalParam.Scale = 2;
                    nuevoPrecioTotalParam.Direction = ParameterDirection.Output;
                    command.Parameters.Add(nuevoPrecioTotalParam);

                    command.ExecuteNonQuery();

                    
                    nuevoPrecioTotal = (decimal)nuevoPrecioTotalParam.Value;
                }
            }
            
            return RedirectToAction("Carrito");
        }

        public ActionResult AgregarAlCarrito()
        {
            return View();
        }
        [HttpPost]
        public ActionResult AgregarAlCarrito(FormCollection form)
        {

            string modelo = form["modelo"];
            int precio = Convert.ToInt32(form["precio"]);


            int identificacionUsuario = Convert.ToInt32(Session["UsuarioIdentificacion"]);


            string numeroSerie = ObtenerNumeroSerieDesdeBD(modelo);

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand cmd = new SqlCommand("dbo.AgregarAlCarrito", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;


                    cmd.Parameters.AddWithValue("@Identificacion", identificacionUsuario);
                    cmd.Parameters.AddWithValue("@Numero_Serie", numeroSerie);
                    cmd.Parameters.AddWithValue("@Modelo", modelo);
                    cmd.Parameters.AddWithValue("@Cantidad", 1);
                    cmd.Parameters.AddWithValue("@Precio", precio);
                    cmd.Parameters.AddWithValue("@Precio_Total", precio);


                    Auditoria.RegistrarAuditoria(identificacionUsuario, $"Agregado al carrito: {modelo}", connectionString);


                    cmd.ExecuteNonQuery();
                }
            }


            return RedirectToAction("Home");
        }

        //Formas de pago
        public ActionResult Pagar()
        {
            return View();
        }

        public ActionResult Paypal()
        {
            int identificacionUsuario = ObtenerIdentificacionUsuario();
            decimal totalAPagar = ObtenerTotalAPagar(identificacionUsuario);


            Auditoria.RegistrarAuditoria(identificacionUsuario, "Acceso a Paypal", connectionString);

            ViewBag.TotalAPagar = totalAPagar;
            ViewBag.IdentificacionUsuario = identificacionUsuario;

            return View();
        }

        public ActionResult Tarjeta()
        {
            int identificacionUsuario = ObtenerIdentificacionUsuario();
            decimal totalAPagar = ObtenerTotalAPagar(identificacionUsuario);


            Auditoria.RegistrarAuditoria(identificacionUsuario, "Acceso a la página de pago con tarjeta", connectionString);

            ViewBag.TotalAPagar = totalAPagar;
            ViewBag.IdentificacionUsuario = identificacionUsuario;

            return View();
        }

        public ActionResult Tranferencia()
        {
            int identificacionUsuario = ObtenerIdentificacionUsuario();
            decimal totalAPagar = ObtenerTotalAPagar(identificacionUsuario);


            Auditoria.RegistrarAuditoria(identificacionUsuario, "Acceso a la página de pago con transfereancia", connectionString);

            ViewBag.TotalAPagar = totalAPagar;
            ViewBag.IdentificacionUsuario = identificacionUsuario;

            return View();
        }

        public ActionResult MostrarTotalAPagar()
        {
            int identificacionUsuario = ObtenerIdentificacionUsuario();

            if (identificacionUsuario == -1)
            {
                
                return RedirectToAction("Login"); 
            }

            decimal totalAPagar = ObtenerTotalAPagar(identificacionUsuario);

            ViewBag.TotalAPagar = totalAPagar;

            return View();
        }

        private decimal ObtenerTotalAPagar(int identificacionUsuario)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand command = new SqlCommand("ObtenerTotalAPagar", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@Identificacion", identificacionUsuario);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    decimal totalAPagar = 0;

                    while (reader.Read())
                    {
                       
                        totalAPagar += Convert.ToDecimal(reader["TotalAPagar"]);
                    }

                    return totalAPagar;
                }
            }
        }

        public ActionResult PagarYBorrar()
        {
            int identificacionUsuario = ObtenerIdentificacionUsuario();

            if (identificacionUsuario == -1)
            {
               
                return RedirectToAction("Login"); 
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("BorrarCarritoPorIdentificacion", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Identificacion", identificacionUsuario);

                    command.ExecuteNonQuery();
                }
            }

            

            return RedirectToAction("Home");
        }
        public ActionResult loginPaypal()
        {

            return View();
        }

        
        //Recuperar contraseña 
       public ActionResult RecuperarContraseña() {
            return View();
       }

        //public ActionResult EnviarCorreo()
        //{
        //    return View();
        //}


        public ActionResult ObtenerPreguntas(string correo)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Verificar si el correo está registrado
                    int resultadoCorreoExistente = 0;
                    using (SqlCommand verificarCorreoCommand = new SqlCommand("VerificarCorreo", connection))
                    {
                        verificarCorreoCommand.CommandType = CommandType.StoredProcedure;
                        verificarCorreoCommand.Parameters.AddWithValue("@correo_verificar", correo);
                        verificarCorreoCommand.Parameters.Add("@resultado", SqlDbType.Int).Direction = ParameterDirection.Output;
                        verificarCorreoCommand.ExecuteNonQuery();
                        resultadoCorreoExistente = Convert.ToInt32(verificarCorreoCommand.Parameters["@resultado"].Value);
                    }

                    if (resultadoCorreoExistente == 0)
                    {
                        ViewBag.ErrorMessage = "El correo electrónico no está registrado.";
                        return View("RecuperarContraseña");
                    }

                    // Eliminar la contraseña asociada al correo
                    using (SqlCommand eliminarContraseñaCommand = new SqlCommand("EliminarContraseña", connection))
                    {
                        eliminarContraseñaCommand.CommandType = CommandType.StoredProcedure;
                        eliminarContraseñaCommand.Parameters.AddWithValue("@correo", correo);
                        eliminarContraseñaCommand.ExecuteNonQuery();
                    }

                    // Obtener las preguntas de seguridad si el correo está registrado
                    SqlCommand command = new SqlCommand("ObtenerPreguntas", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Correo", correo);

                    PreguntasViewModel model = new PreguntasViewModel();
                    model.Correo = correo;

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            model.Pregunta1 = reader["Pregunta1"].ToString();
                            model.Pregunta2 = reader["Pregunta2"].ToString();
                            model.Pregunta3 = reader["Pregunta3"].ToString();
                        }
                        else
                        {
                            ViewBag.ErrorMessage = "No se encontraron preguntas asociadas a este correo.";
                            return View("RecuperarContraseña");
                        }
                    }

                    TempData["Correo"] = correo;

                    // Enviar el código de recuperación
                    EnviarCodigoDeRecuperacion(correo);

                    ViewBag.Correo = correo;

                    return View("Recuperar", model);
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error al recuperar las preguntas: " + ex.Message;
                return View("RecuperarContraseña");
            }
        }


        private void EnviarCodigoDeRecuperacion(string correo)
        {
            
            int codigoAleatorio = GenerarCodigoAleatorio();

            
            string codigo = codigoAleatorio.ToString("D4");

          
            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("gbarahonaq1708@gmail.com", "faoo mwhe weio lsnw"),
                EnableSsl = true,
            };

            
            MailMessage mensaje = new MailMessage("gbarahonaq1708@gmail.com", correo)
            {
                Subject = "Código de recuperación de contraseña",
                Body = $"Querido Usuario:\n\nTu código de recuperación es: {codigo}. Úsalo para restablecer tu contraseña.\n\nAtentamente,\nDrones Blue and White Robotics",
            };

            
            smtpClient.Send(mensaje);

           
            Session["CodigoRecuperacion"] = codigo;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

       
        private void ResetPassword(string correo, string connectionString)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (var command = new SqlCommand("EliminarContraseña", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    
                    command.Parameters.Add(new SqlParameter("@correo", correo));

                   
                    command.ExecuteNonQuery();
                }
            }
        }


        //Verifcar Preguntas
        public ActionResult Recuperar(string correo)
        {
            // Crear un modelo para la vista
            PreguntasViewModel model = new PreguntasViewModel();
            model.Correo = correo;

            return View(model); 
        }

        [HttpPost]
        public ActionResult ValidarRespuestas(string correo, string respuesta1, string respuesta2, string respuesta3)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand("CompararRespuestas", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Correo", correo);
                    command.Parameters.AddWithValue("@Respuesta1", respuesta1);
                    command.Parameters.AddWithValue("@Respuesta2", respuesta2);
                    command.Parameters.AddWithValue("@Respuesta3", respuesta3);

                    SqlParameter resultadoParam = new SqlParameter("@Resultado", SqlDbType.Int);
                    resultadoParam.Direction = ParameterDirection.Output;
                    command.Parameters.Add(resultadoParam);

                    command.ExecuteNonQuery();

                    int resultado = Convert.ToInt32(resultadoParam.Value);

                    if (resultado == 1)
                    {
                        return RedirectToAction("VerficarCodigo");
                    }
                    else if (resultado == 0)
                    {
                        ViewBag.ErrorMessage = "Las respuestas no son correctas. Inténtalo de nuevo.";
                        return View("Recuperar");
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Error al validar las respuestas.";
                        return View("Recuperar");
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error al validar las respuestas: " + ex.Message;
                return View("Recuperar");
            }
        }



        //Verficar Codigo
        public ActionResult VerficarCodigo()
        {
            return View();
        }

        [HttpPost]
        public ActionResult VerificarCodigo(string codigoIngresado)
        {
            string codigoAlmacenado = Session["CodigoRecuperacion"] as string;

            bool codigoValido = VerificarCodigo(codigoIngresado, codigoAlmacenado, "RecuperarContraseña");

            if (codigoValido)
            {
                return RedirectToAction("RestablecerContraseña", "Home");
            }
            else
            {
                ViewBag.MensajeError = "El código ingresado es incorrecto. Por favor, inténtalo nuevamente.";
                return View("VerficarCodigo");
            }
        }

        private bool VerificarCodigo(string codigoIngresado, string codigoAlmacenado, string vistaError)
        {

            if (codigoAlmacenado != null && codigoIngresado == codigoAlmacenado)
            {

                return true;
            }


            ViewBag.MensajeError = "El código ingresado es incorrecto. Por favor, inténtalo nuevamente.";
            return false;
        }

        //Restaurar Constraseña
        public ActionResult RestablecerContraseña()
        {

            return View();
        }
        [HttpPost]
        public ActionResult RestablecerContraseña(string nuevaContraseña)
        {
            
            string correo = TempData["Correo"] as string;

            try
            {
               
                bool cambioExitoso = InsertarContraseñaEnBaseDeDatos(correo, nuevaContraseña);

                if (cambioExitoso)
                {
                    ViewBag.Mensaje = "La contraseña se cambió exitosamente.";
                    return View("RestablecerContraseña");
                }
                else
                {
                    ViewBag.MensajeError = "No se pudo cambiar la contraseña. Inténtalo nuevamente.";
                    return View("RestablecerContraseña");
                }
            }
            catch (Exception ex)
            {
                ViewBag.MensajeError = $"Error: {ex.Message}";
                return View("RestablecerContraseña");
            }
        }

        private bool InsertarContraseñaEnBaseDeDatos(string correo, string nuevaContraseña)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (var command = new SqlCommand("InsertarContraseña", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                       
                        command.Parameters.AddWithValue("@correo", correo);
                        command.Parameters.AddWithValue("@nuevaContraseña", nuevaContraseña);

                        command.ExecuteNonQuery();
                    }
                }

                return true; 
            }
            catch (Exception)
            {
               
                return false;
            }
        }

    }

}