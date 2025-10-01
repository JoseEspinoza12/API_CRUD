using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using prjAPI4.Data;
using prjAPI4.Models.catEmpleados;
using System.Collections.Generic;
using System.Threading.Tasks;

// 1. Añadir atributos de API y Ruta Base del Controlador
[Route("api/[controller]")] // Define la ruta base: /api/Empleado
[ApiController]
public class EmpleadoController : ControllerBase
{
    private readonly clsCatEmpleadosData _objEmpleadoData;

    public EmpleadoController(clsCatEmpleadosData empleadosData)
    {
        _objEmpleadoData = empleadosData;
    }

    // ----------------------------------------------------------------------
    // 1. READ (Listar Todos) - GET /api/Empleado
    // ----------------------------------------------------------------------
    [HttpGet] // Ahora usa la ruta base: /api/Empleado
    public async Task<IActionResult> listar()
    {
        List<clsCatEmpleados> Lista = await _objEmpleadoData.ListaEmpleados();
        return StatusCode(StatusCodes.Status200OK, Lista);
    }

    // ----------------------------------------------------------------------
    // 2. READ (Obtener por ID) - GET /api/Empleado/{IdEmpleado}
    // ----------------------------------------------------------------------
    // La ruta es: /api/Empleado/123
    [HttpGet("{IdEmpleado}")]
    public async Task<IActionResult> Obtener(int IdEmpleado)
    {
        clsCatEmpleados? empleado = await _objEmpleadoData.ObtenerEmpleado(IdEmpleado);
        // ... (Lógica de 404/200)
        if (empleado == null)
        {
            return NotFound(new { mensaje = $"Empleado con ID {IdEmpleado} no encontrado." });
        }
        return Ok(empleado);
    }

    // ----------------------------------------------------------------------
    // 3. CREATE (Crear) - POST /api/Empleado
    // ----------------------------------------------------------------------
    // La ruta es: /api/Empleado
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] clsCatEmpleados empleado)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        bool resultado = await _objEmpleadoData.CrearEmpleado(empleado);

        if (resultado)
        {
            // 201 Created es más apropiado para POST
            return StatusCode(StatusCodes.Status201Created, new { mensaje = "Empleado creado correctamente" });
        }
        else
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error al crear el empleado." });
        }
    }

    // ----------------------------------------------------------------------
    // 4. UPDATE (Editar) - PUT /api/Empleado
    // ----------------------------------------------------------------------
    // La ruta es: /api/Empleado
    [HttpPut]
    public async Task<IActionResult> Editar([FromBody] clsCatEmpleados empleado)
    {
        try
        {
            // Llama a la capa de datos. Si la DB se actualiza, esta función devuelve TRUE.
            bool resultado = await _objEmpleadoData.EditarEmpleado(empleado);

            //  CORRECCIÓN: Si el resultado es TRUE (la fila se afectó), devuelve 200 OK.
            if (resultado)
            {
                return Ok(new { mensaje = "Empleado editado correctamente." });
            }
            else
            {
                //  Si el resultado es FALSE (0 filas afectadas), el ID no existía.
                return NotFound(new { mensaje = $"No se encontró el empleado con IdEmpleado {empleado.intIdEmpleados} para editar (ID inexistente o inactivo)." });
            }
        }
        catch (Exception)
        {
            //  Manejo de errores internos.
            return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error interno del servidor al procesar la solicitud de edición." });
        }
    }

    // ----------------------------------------------------------------------
    // 5. DELETE (Eliminar Lógico) - DELETE /api/Empleado/{IdEmpleado}
    // ----------------------------------------------------------------------
    // Necesitas la ruta para el parámetro, o el cuerpo de la petición no se mapeará correctamente.
    // La ruta es: /api/Empleado/123
    [HttpDelete("{IdEmpleado}")]
    public async Task<IActionResult> Eliminar(int IdEmpleado)
    {
        try
        {
            // Llama a la capa de datos
            bool resultado = await _objEmpleadoData.EliminarEmpleado(IdEmpleado);

            // 🛑 CORRECCIÓN CLAVE: Evalúa el TRUE para devolver 200 OK.
            if (resultado)
            {
                // Éxito: El resultado es TRUE. Retorna 200 OK.
                return Ok(new { mensaje = "Empleado eliminado correctamente." });
            }
            else
            {
                // Fracaso Lógico: El resultado es FALSE porque filasAfectadas fue 0. 
                // Esto significa que el ID no existía o ya estaba inactivo.
                return NotFound(new { mensaje = $"No se encontró un empleado activo con el IdEmpleado {IdEmpleado} para eliminar." });
            }
        }
        catch (Exception)
        {
            // Fracaso por Excepción: Si ocurrió un error no manejado (ej. conexión)
            return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error interno del servidor al procesar la solicitud de eliminación." });
        }
    }
}