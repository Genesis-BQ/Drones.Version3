using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web;
using Drones.Controllers;

namespace Drones.Models
{
    public class LibroV
    {
        public static string connectionString = "Server=GENESIS;Database=Drones;Integrated Security=True;";

        //public void Grabar(int identificacion, string nombre, int telefono, string correo, string contraseña)
        //{
        //    using (SqlConnection connection = new SqlConnection(connectionString))
        //    {
        //        try
        //        {
        //            connection.Open();
        //            SqlCommand command = new SqlCommand("InsertRegistro", connection);
        //            command.CommandType = CommandType.StoredProcedure;
        //            command.Parameters.AddWithValue("@Identificacion", identificacion);
        //            command.Parameters.AddWithValue("@Nombre", nombre);
        //            command.Parameters.AddWithValue("@Telefono", telefono);
        //            command.Parameters.AddWithValue("@Correo", correo);
        //            command.Parameters.AddWithValue("@Contraseña", contraseña);
        //            command.ExecuteNonQuery();
        //        }
        //        catch (Exception ex)
        //        {
        //            // Registra la excepción en un archivo o sistema de logging
        //            Console.WriteLine($"Ha ocurrido un error: {ex.Message}");

        //        }
        //    }
        //}
        public bool ValidarCredenciales(int identificacion, string contraseña, out int resultado)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand("ValidarCredenciales", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Identificacion", identificacion);
                    command.Parameters.AddWithValue("@Contraseña", contraseña);

                    // Agregar parámetro de salida
                    SqlParameter resultadoParam = new SqlParameter("@Resultado", SqlDbType.Int);
                    resultadoParam.Direction = ParameterDirection.Output;
                    command.Parameters.Add(resultadoParam);

                    // Ejecutar el procedimiento almacenado
                    command.ExecuteNonQuery();

                    // Obtener el resultado
                    resultado = Convert.ToInt32(resultadoParam.Value);

                    return resultado == 1; // Devuelve true si las credenciales son válidas
                }
                catch (Exception ex)
                {
                    // Manejar la excepción, registrarla o relanzarla según sea necesario
                    // Aquí simplemente lo registramos, pero podrías hacer algo más adecuado para tu aplicación
                    Console.WriteLine($"Error en ValidarCredenciales: {ex.Message}");
                    resultado = 0; // Establecer resultado en un valor predeterminado
                    return false; // Devolver false en caso de error
                }
            }
        }




    }

}