using Backend.Api.Entities;
using Backend.Api.Repositories;

namespace Backend.Api.Services;

public interface INoteService
{
    Task<IEnumerable<Note>> GetAllNotesAsync();
    Task<Note?> GetNoteByIdAsync(int id);
    Task<Note> CreateNoteAsync(Note note);
    Task UpdateNoteAsync(int id, Note note);
    Task DeleteNoteAsync(int id);
}

public class NoteService : INoteService
{
    private readonly INoteRepository _noteRepository;
    private readonly ILogger<NoteService> _logger;

    public NoteService(INoteRepository noteRepository, ILogger<NoteService> logger)
    {
        _noteRepository = noteRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<Note>> GetAllNotesAsync()
    {
        _logger.LogInformation("Fetching all notes");
        return await _noteRepository.GetAllAsync();
    }

    public async Task<Note?> GetNoteByIdAsync(int id)
    {
        _logger.LogInformation("Fetching note with ID: {Id}", id);
        return await _noteRepository.GetByIdAsync(id);
    }

    public async Task<Note> CreateNoteAsync(Note note)
    {
        _logger.LogInformation("Creating new note with title: {Title}", note.Title);
        
        // Business logic: Validate note
        if (string.IsNullOrWhiteSpace(note.Title))
        {
            throw new ArgumentException("Note title cannot be empty");
        }

        return await _noteRepository.AddAsync(note);
    }

    public async Task UpdateNoteAsync(int id, Note note)
    {
        _logger.LogInformation("Updating note with ID: {Id}", id);
        
        var existingNote = await _noteRepository.GetByIdAsync(id);
        if (existingNote == null)
        {
            throw new KeyNotFoundException($"Note with ID {id} not found");
        }

        note.Id = id;
        await _noteRepository.UpdateAsync(note);
    }

    public async Task DeleteNoteAsync(int id)
    {
        _logger.LogInformation("Deleting note with ID: {Id}", id);
        
        var existingNote = await _noteRepository.GetByIdAsync(id);
        if (existingNote == null)
        {
            throw new KeyNotFoundException($"Note with ID {id} not found");
        }

        await _noteRepository.DeleteAsync(id);
    }
}
