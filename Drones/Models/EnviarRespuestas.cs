using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Drones.Models
{
    public class EnviarRespuestas
    {
        public string Correo { get; set; }
        public List<string> Preguntas { get; set; }
        public string RespuestaSeleccionada { get; set; }
        public string RespuestaPersonal { get; set; }
    }
}