using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace Drones.Models
{
    public class Auditoria
    {
        // Método estático para registrar auditoría en la base de datos
        public static void RegistrarAuditoria(int identificacion, string actividad, string connectionString)
        {
            // Utilizamos using para garantizar que los recursos se liberen correctamente
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open(); // Abrimos la conexión a la base de datos

                // Creamos un comando SQL para ejecutar el procedimiento almacenado
                using (SqlCommand command = new SqlCommand("RegistrarAuditoria", connection))
                {
                    // Especificamos que estamos ejecutando un procedimiento almacenado
                    command.CommandType = System.Data.CommandType.StoredProcedure;

                    // Definimos los parámetros del procedimiento almacenado
                    command.Parameters.AddWithValue("@Identificacion", identificacion);
                    command.Parameters.AddWithValue("@Actividad", actividad);

                    // Ejecutamos el comando
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}