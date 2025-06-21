// File: EHRsystem/Models/Entities/Admin.cs
// Last updated: Admin entity model.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EHRsystem.Models; // For the base User model

namespace EHRsystem.Models.Entities
{
    // Admin inherits from User (Table-Per-Hierarchy or TPH)
    public class Admin : User
    {
        // No additional properties specific to Admin for now, but could be added here.
        // Example if needed:
        // public string AdminSpecificProperty { get; set; }
    }
}
