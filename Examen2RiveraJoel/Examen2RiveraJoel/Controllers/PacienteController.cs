using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Prueba2Hotel.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PacienteController : ControllerBase
    {
        private readonly AppDBContext _appDBContext;

        public PacienteController(AppDBContext appDBContext)
        {
            _appDBContext = appDBContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetPacientes()
        {
            return Ok(await _appDBContext.Paciente.ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> PostPaciente(Paciente paciente)
        {
            if (!ModelState.IsValid)
            {
                var errores = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return Ok(new { message = "Errores de validación", errores });
            }

            string mensaje = UtilsPaciente.ValidacionDatosPaciente(paciente);

            if (!string.IsNullOrEmpty(mensaje))
            {
                return Ok(new { message = mensaje });
            }

            if (await _appDBContext.Paciente.AnyAsync(p => p.Correo == paciente.Correo))
            {
                return Ok(new { message = "Ya existe un paciente con ese correo." });
            }

            _appDBContext.Paciente.Add(paciente);
            await _appDBContext.SaveChangesAsync();
            return Ok(new { message = "Paciente creado correctamente.", idPaciente = paciente.Id });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutPaciente(int id, [FromBody] Paciente paciente)
        {
            if (!ModelState.IsValid)
            {
                var errores = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return Ok(new { message = "Errores de validación", errores });
            }

            if (id != paciente.Id)
            {
                return Ok(new { message = "El ID del paciente no coincide." });
            }

            string mensaje = UtilsPaciente.ValidacionDatosPaciente(paciente);
            if (!string.IsNullOrEmpty(mensaje))
            {
                return Ok(new { message = mensaje });
            }

            var pacienteExistente = await _appDBContext.Paciente.FirstOrDefaultAsync(p => p.Id == id);
            if (pacienteExistente == null)
            {
                return Ok(new { message = "Paciente no encontrado." });
            }

            pacienteExistente.Nombre = paciente.Nombre;
            pacienteExistente.Apellido = paciente.Apellido;
            pacienteExistente.Correo = paciente.Correo;
            pacienteExistente.Direccion = paciente.Direccion;

            try
            {
                _appDBContext.Paciente.Update(pacienteExistente);
                await _appDBContext.SaveChangesAsync();
                return Ok(new { message = "Paciente actualizado exitosamente.", paciente = pacienteExistente });
            }
            catch (DbUpdateConcurrencyException)
            {
                return Ok(new { message = "Error al actualizar el paciente." });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePaciente(int id)
        {
            var paciente = await _appDBContext.Paciente.FirstOrDefaultAsync(p => p.Id == id);
            if (paciente == null)
            {
                return Ok(new { message = "Paciente no encontrado." });
            }

            _appDBContext.Paciente.Remove(paciente);
            await _appDBContext.SaveChangesAsync();
            return Ok(new { message = "Paciente eliminado correctamente." });
        }
    }

    public static class UtilsPaciente
    {
        public static string ValidacionDatosPaciente(Paciente paciente)
        {
            if (string.IsNullOrEmpty(paciente.Nombre))
            {
                return "El nombre es requerido.";
            }
            if (string.IsNullOrEmpty(paciente.Apellido))
            {
                return "El apellido es requerido.";
            }
            if (string.IsNullOrEmpty(paciente.Correo))
            {
                return "El correo es requerido.";
            }
            if (paciente.Direccion == null)
            {
                return "La dirección es requerida.";
            }

            if (paciente.Nombre.Length > 100)
            {
                return "El nombre no puede tener más de 100 caracteres.";
            }
            if (paciente.Apellido.Length > 100)
            {
                return "El apellido no puede tener más de 100 caracteres.";
            }
            if (paciente.Correo.Length > 100)
            {
                return "El correo no puede tener más de 100 caracteres.";
            }
            if (paciente.Direccion.Length > 500)
            {
                return "La dirección no puede tener más de 500 caracteres.";
            }

            return string.Empty;
        }
    }
}