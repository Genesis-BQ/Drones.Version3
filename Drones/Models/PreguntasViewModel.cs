using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Drones.Models
{
    public class PreguntasViewModel
    {
        public string Correo { get; set; }
        public string Pregunta1 { get; set; }
        public string Pregunta2 { get; set; }
        public string Pregunta3 { get; set; }
        public string Respuesta1 { get; set; }
        public string Respuesta2 { get; set; }
        public string Respuesta3 { get; set; }
        public string ErrorMessage { get; set; }
    }

}