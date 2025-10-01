using prjAPI4.Models.catEmpleados;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace prjAPI4.Data
{
    public class clsCatEmpleadosData
    {
        private readonly string conexion;

        public clsCatEmpleadosData(IConfiguration configuration)
        {
            conexion = configuration.GetConnectionString("CadenaSQL")!;
        }

        //  CORRECCIÓN: ID mapeado correctamente (reader["IdEmpleado"])
        public async Task<List<clsCatEmpleados>> ListaEmpleados()
        {
            List<clsCatEmpleados> lista = new List<clsCatEmpleados>();

            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("listaEmpleados", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            lista.Add(new clsCatEmpleados
                            {
                                intIdEmpleados = Convert.ToInt32(reader["IdEmpleado"]),
                                strNombreCompleto = reader["NOMBRECOMPLETO"].ToString(),
                                strCorreo = reader["CORREO"].ToString(),
                                dblSueldo = Convert.ToDouble(reader["SUELDO"]),
                                strFechaContratacion = reader["FECHACONTRATO"].ToString(),
                                strEstatus = reader["ESTATUS"].ToString()
                            });
                        }
                    }
                }
            }
            return lista;
        }

        // 🟢 CORRECCIÓN: ID mapeado correctamente para obtener un solo empleado
        public async Task<clsCatEmpleados?> ObtenerEmpleado(int IdEmpleado)
        {
            clsCatEmpleados empleado = null;

            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("obtenerEmpleado", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@IdEmpleado", IdEmpleado);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            empleado = new clsCatEmpleados
                            {
                                intIdEmpleados = Convert.ToInt32(reader["IdEmpleado"]),
                                strNombreCompleto = reader["NOMBRECOMPLETO"].ToString(),
                                strCorreo = reader["CORREO"].ToString(),
                                dblSueldo = Convert.ToDouble(reader["SUELDO"]),
                                strFechaContratacion = reader["FECHACONTRATO"].ToString(),
                                strEstatus = reader["ESTATUS"].ToString()
                            };
                        }
                    }
                }
            }
            return empleado;
        }

        public async Task<bool> CrearEmpleado(clsCatEmpleados pobjCatEmpleado)
        {
            bool bolRespuesta = false;
            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("crearEmpleado", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@nombreCompleto", pobjCatEmpleado.strNombreCompleto ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Correo", pobjCatEmpleado.strCorreo ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Sueldo", pobjCatEmpleado.dblSueldo.HasValue ? pobjCatEmpleado.dblSueldo.Value : (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@FechaContrato", pobjCatEmpleado.strFechaContratacion ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Estatus", pobjCatEmpleado.strEstatus ?? (object)DBNull.Value);

                    try
                    {
                        int filasAfectadas = await cmd.ExecuteNonQueryAsync();
                        if (filasAfectadas > 0)
                        {
                            bolRespuesta = true;
                        }
                    }
                    catch (SqlException)
                    {
                        // No hay logging para no depender de la consola
                        bolRespuesta = false;
                    }
                }
            }
            return bolRespuesta;
        }

        // Dentro de clsCatEmpleadosData.cs

        public async Task<bool> EditarEmpleado(clsCatEmpleados pobjCatEmpleado)
        {
            //  Cambiamos la inicialización a TRUE. Solo será FALSE si hay una excepción SQL.
            bool bolRespuesta = true;

            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("editarEmpleado", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Parámetros
                    cmd.Parameters.AddWithValue("@IdEmpleado", pobjCatEmpleado.intIdEmpleados);
                    cmd.Parameters.AddWithValue("@nombreCompleto", pobjCatEmpleado.strNombreCompleto ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Correo", pobjCatEmpleado.strCorreo ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Sueldo", pobjCatEmpleado.dblSueldo.HasValue ? pobjCatEmpleado.dblSueldo.Value : (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@FechaContrato", pobjCatEmpleado.strFechaContratacion ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Estatus", pobjCatEmpleado.strEstatus ?? (object)DBNull.Value);

                    try
                    {
                        // Solo ejecutamos el comando. No necesitamos el resultado de filasAfectadas.
                        await cmd.ExecuteNonQueryAsync();

                        //  Si llegamos aquí sin excepción, es un éxito. bolRespuesta ya es true.
                    }
                    catch (SqlException)
                    {
                        //  Si hay un error SQL (ej. violación de clave), entonces es un fallo.
                        bolRespuesta = false;
                    }
                }
            }

            return bolRespuesta;
        }

        public async Task<bool> EliminarEmpleado(int IdEmpleado)
        {
            //Inicializamos en TRUE. Solo será FALSE si ocurre una excepción SQL.
            bool bolRespuesta = true;

            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("eliminarEmpleado", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@IdEmpleado", IdEmpleado);

                    try
                    {
                        // Solo ejecutamos el comando. No necesitamos el resultado de filasAfectadas.
                        await cmd.ExecuteNonQueryAsync();

                        //  Si llegamos aquí sin excepción, es un éxito. bolRespuesta ya es true.
                    }
                    catch (SqlException)
                    {
                        //  Si hay un error SQL (ej. problema de conexión), entonces es un fallo.
                        bolRespuesta = false;
                    }
                }
            }
            return bolRespuesta;
        }
    }
}