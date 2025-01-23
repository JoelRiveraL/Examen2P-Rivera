using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Prueba2Hotel.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConsultorioController : ControllerBase
    {
        private readonly AppDBContext _appDBContext;

        public ConsultorioController(AppDBContext appDBContext)
        {
            _appDBContext = appDBContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetConsultorios()
        {
            return Ok(await _appDBContext.Consultorio.ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> PostConsultorio(Consultorio consultorio)
        {
            if (!ModelState.IsValid)
            {
                var errores = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return Ok(new { message = "Errores de validación", errores });
            }

            string mensaje = UtilsConsultorio.ValidacionDatosConsultorio(consultorio);

            if (!string.IsNullOrEmpty(mensaje))
            {
                return Ok(new { message = mensaje });
            }

            _appDBContext.Consultorio.Add(consultorio);
            await _appDBContext.SaveChangesAsync();
            return Ok(new { message = "Consultorio creado correctamente.", id = consultorio.Id });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutConsultorio(int id, [FromBody] Consultorio consultorio)
        {
            if (!ModelState.IsValid)
            {
                var errores = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return Ok(new { message = "Errores de validación", errores });
            }

            if (id != consultorio.Id)
            {
                return Ok(new { message = "El ID del consultorio no coincide." });
            }

            string mensaje = UtilsConsultorio.ValidacionDatosConsultorio(consultorio);
            if (!string.IsNullOrEmpty(mensaje))
            {
                return Ok(new { message = mensaje });
            }

            var consultorioExistente = await _appDBContext.Consultorio.FirstOrDefaultAsync(c => c.Id == id);
            if (consultorioExistente == null)
            {
                return Ok(new { message = "Consultorio no encontrado." });
            }

            consultorioExistente.Numero = consultorio.Numero;
            consultorioExistente.Piso = consultorio.Piso;

            try
            {
                _appDBContext.Consultorio.Update(consultorioExistente);
                await _appDBContext.SaveChangesAsync();
                return Ok(new { message = "Consultorio actualizado exitosamente.", consultorio = consultorioExistente });
            }
            catch (DbUpdateConcurrencyException)
            {
                return Ok(new { message = "Error al actualizar el consultorio." });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteConsultorio(int id)
        {
            var consultorio = await _appDBContext.Consultorio.FirstOrDefaultAsync(c => c.Id == id);
            if (consultorio == null)
            {
                return Ok(new { message = "Consultorio no encontrado." });
            }

            _appDBContext.Consultorio.Remove(consultorio);
            await _appDBContext.SaveChangesAsync();
            return Ok(new { message = "Consultorio eliminado correctamente." });
        }
    }

    public static class UtilsConsultorio
    {
        public static string ValidacionDatosConsultorio(Consultorio consultorio)
        {
            if (consultorio.Numero == null)
            {
                return "El número es requerido.";
            }
            if (consultorio.Piso == null)
            {
                return "El piso es requerido.";
            }

            return string.Empty;
        }
    }
}