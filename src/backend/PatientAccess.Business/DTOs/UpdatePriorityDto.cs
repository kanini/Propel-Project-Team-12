/**
 * UpdatePriorityDto - Data Transfer Object for Priority Flag Update
 * Used for updating patient priority status in the queue
 */

using System.ComponentModel.DataAnnotations;

namespace PatientAccess.Business.DTOs;

/// <summary>
/// DTO for updating patient priority flag in the queue
/// </summary>
public class UpdatePriorityDto
{
    /// <summary>
    /// Priority flag (true for emergency, false for normal)
    /// </summary>
    [Required]
    public bool IsPriority { get; set; }
}
