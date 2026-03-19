using Backend.Api.Entities;
using Backend.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotesController : ControllerBase
{
    private readonly INoteService _noteService;
    private readonly ILogger<NotesController> _logger;

    public NotesController(INoteService noteService, ILogger<NotesController> logger)
    {
        _noteService = noteService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Note>>> GetAll()
    {
        try
        {
            var notes = await _noteService.GetAllNotesAsync();
            return Ok(notes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all notes");
            return StatusCode(500, new { message = "Error fetching notes" });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Note>> GetById(int id)
    {
        try
        {
            var note = await _noteService.GetNoteByIdAsync(id);
            if (note == null)
            {
                return NotFound(new { message = $"Note with ID {id} not found" });
            }
            return Ok(note);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching note with ID {Id}", id);
            return StatusCode(500, new { message = "Error fetching note" });
        }
    }

    [HttpPost]
    public async Task<ActionResult<Note>> Create([FromBody] Note note)
    {
        try
        {
            var createdNote = await _noteService.CreateNoteAsync(note);
            return CreatedAtAction(nameof(GetById), new { id = createdNote.Id }, createdNote);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating note");
            return StatusCode(500, new { message = "Error creating note" });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Note note)
    {
        try
        {
            await _noteService.UpdateNoteAsync(id, note);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating note with ID {Id}", id);
            return StatusCode(500, new { message = "Error updating note" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _noteService.DeleteNoteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting note with ID {Id}", id);
            return StatusCode(500, new { message = "Error deleting note" });
        }
    }
}
