using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace Drones.Models
{
    public class PreguntaSeguridad
    {

        public static string connectionString = "Server=GENESIS;Database=Drones;Integrated Security=True;";

        public int PreguntaID { get; set; }
        public string Pregunta { get; set; }
        // Nuevas propiedades para provincia, canton y distrito
        [Required(ErrorMessage = "La provincia es obligatoria")]
        public string Provincia { get; set; }

        [Required(ErrorMessage = "El cantón es obligatorio")]
        public string Canton { get; set; }

        [Required(ErrorMessage = "El distrito es obligatorio")]
        public string Distrito { get; set; }

    }
}