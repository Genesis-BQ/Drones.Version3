using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Drones.Models
{
    public class HistorialCompra
    {
        public int IdFactura { get; set; }
        public int IdCompra { get; set; }

        public int Monto { get; set; }
    }
}