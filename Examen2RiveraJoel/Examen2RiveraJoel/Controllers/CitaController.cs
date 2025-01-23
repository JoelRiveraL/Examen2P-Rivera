using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Prueba2Hotel.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CitaController : ControllerBase
    {
        private readonly AppDBContext _appDBContext;

        public CitaController(AppDBContext appDBContext)
        {
            _appDBContext = appDBContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetCitas()
        {
            return Ok(await _appDBContext.Cita.ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> PostCita(Cita cita)
        {
            if (!ModelState.IsValid)
            {
                var errores = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return Ok(new { message = "Errores de validación", errores });
            }

            string mensaje = UtilsCita.ValidacionDatosCita(cita);

            if (!string.IsNullOrEmpty(mensaje))
            {
                return Ok(new { message = mensaje });
            }

            //Validar que no exista una cita en esa fecha y hora en el mismo consultorio
            var citaExistente = await _appDBContext.Cita
                .FirstOrDefaultAsync(c => c.Fecha == cita.Fecha && c.Hora == cita.Hora && c.Consultorio == cita.Consultorio);

            if (citaExistente != null)
            {
                return Ok(new { message = "Ya existe una cita en esa fecha y hora en el mismo consultorio." });
            }

            _appDBContext.Cita.Add(cita);
            await _appDBContext.SaveChangesAsync();
            return Ok(new { message = "Cita creada correctamente.", idCita = cita.Id });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutCita(int id, [FromBody] Cita cita)
        {
            if (!ModelState.IsValid)
            {
                var errores = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return Ok(new { message = "Errores de validación", errores });
            }

            string mensaje = UtilsCita.ValidacionDatosCita(cita);
            if (!string.IsNullOrEmpty(mensaje))
            {
                return Ok(new { message = mensaje });
            }

            var citaExistente = await _appDBContext.Cita.FirstOrDefaultAsync(c => c.Id == id);
            if (citaExistente == null)
            {
                return Ok(new { message = "Cita no encontrada." });
            }

            citaExistente.Fecha = cita.Fecha;
            citaExistente.Hora = cita.Hora;
            citaExistente.PacienteId = cita.PacienteId;
            citaExistente.DoctorId = cita.DoctorId;

            try
            {
                _appDBContext.Cita.Update(citaExistente);
                await _appDBContext.SaveChangesAsync();
                return Ok(new { message = "Cita actualizada exitosamente.", cita = citaExistente });
            }
            catch (DbUpdateConcurrencyException)
            {
                return Ok(new { message = "Error al actualizar la cita." });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCita(int id)
        {
            var cita = await _appDBContext.Cita.FirstOrDefaultAsync(c => c.Id == id);
            if (cita == null)
            {
                return Ok(new { message = "Cita no encontrada." });
            }

            _appDBContext.Cita.Remove(cita);
            await _appDBContext.SaveChangesAsync();
            return Ok(new { message = "Cita eliminada correctamente." });
        }
    }

    public static class UtilsCita
    {
        public static string ValidacionDatosCita(Cita cita)
        {
            if (cita.Fecha == null)
            {
                return "La fecha es requerida.";
            }
            if (cita.Hora == null)
            {
                return "La hora es requerida.";
            }

            return string.Empty;
        }
    }
}