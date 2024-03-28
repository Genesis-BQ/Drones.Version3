using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Drones.Models
{
    public class Traktor
    {
        public string Tipo { get; set; }
        public string Modelo { get; set; }
        public string Descripcion { get; set; }
        public string FichaTecnica { get; set; }
        public decimal Precio { get; set; }
    }
}