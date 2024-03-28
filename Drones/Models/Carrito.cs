using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Drones.Models
{
    public class Carrito
    {
        public string Numero_Serie { get; set; }
        public string Modelo { get; set; }
        public int Cantidad { get; set; }
        public decimal Precio { get; set; }
        public decimal? Precio_Total { get; set; } 
    }
}