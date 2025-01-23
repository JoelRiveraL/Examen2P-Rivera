using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Prueba2Hotel.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MedicoController : ControllerBase
    {
        private readonly AppDBContext _appDBContext;

        public MedicoController(AppDBContext appDBContext)
        {
            _appDBContext = appDBContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetMedicos()
        {
            return Ok(await _appDBContext.Medico.ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> PostMedico(Medico medico)
        {
            if (!ModelState.IsValid)
            {
                var errores = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return Ok(new { message = "Errores de validación", errores });
            }

            string mensaje = UtilsMedico.ValidacionDatosMedico(medico);

            if (!string.IsNullOrEmpty(mensaje))
            {
                return Ok(new { message = mensaje });
            }

            if (await _appDBContext.Medico.AnyAsync(m => m.Nombre == medico.Nombre && m.Apellido == medico.Apellido))
            {
                return Ok(new { message = "Ya existe un médico con ese nombre y apellido." });
            }

            _appDBContext.Medico.Add(medico);
            await _appDBContext.SaveChangesAsync();
            return Ok(new { message = "Médico creado correctamente.", idMedico = medico.Id });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutMedico(int id, [FromBody] Medico medico)
        {
            if (!ModelState.IsValid)
            {
                var errores = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return Ok(new { message = "Errores de validación", errores });
            }

            if (id != medico.Id)
            {
                return Ok(new { message = "El ID del médico no coincide." });
            }

            string mensaje = UtilsMedico.ValidacionDatosMedico(medico);
            if (!string.IsNullOrEmpty(mensaje))
            {
                return Ok(new { message = mensaje });
            }

            var medicoExistente = await _appDBContext.Medico.FirstOrDefaultAsync(m => m.Id == id);
            if (medicoExistente == null)
            {
                return Ok(new { message = "Médico no encontrado." });
            }

            medicoExistente.Nombre = medico.Nombre;
            medicoExistente.Apellido = medico.Apellido;
            medicoExistente.Especialidad = medico.Especialidad;

            try
            {
                _appDBContext.Medico.Update(medicoExistente);
                await _appDBContext.SaveChangesAsync();
                return Ok(new { message = "Médico actualizado exitosamente.", medico = medicoExistente });
            }
            catch (DbUpdateConcurrencyException)
            {
                return Ok( new { message = "Error al actualizar el médico." });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMedico(int id)
        {
            var medico = await _appDBContext.Medico.FirstOrDefaultAsync(m => m.Id == id);
            if (medico == null)
            {
                return Ok(new { message = "Médico no encontrado." });
            }

            _appDBContext.Medico.Remove(medico);
            await _appDBContext.SaveChangesAsync();
            return Ok(new { message = "Médico eliminado correctamente." });
        }
    }

    public static class UtilsMedico
    {
        public static string ValidacionDatosMedico(Medico medico)
        {
            if (string.IsNullOrEmpty(medico.Nombre))
            {
                return "El nombre es requerido.";
            }
            if (string.IsNullOrEmpty(medico.Apellido))
            {
                return "El apellido es requerido.";
            }
            if (string.IsNullOrEmpty(medico.Especialidad))
            {
                return "La especialidad es requerida.";
            }

            if (medico.Nombre.Length > 100)
            {
                return "El nombre no puede tener más de 100 caracteres.";
            }
            if (medico.Apellido.Length > 100)
            {
                return "El apellido no puede tener más de 100 caracteres.";
            }
            if (medico.Especialidad.Length > 100)
            {
                return "La especialidad no puede tener más de 100 caracteres.";
            }

            return string.Empty;
        }
    }
}