﻿using System.ComponentModel.DataAnnotations;
using contester.Data.Common;
using contester.Features.Authentication;
using contester.Features.ContestApplications;
using contester.Features.Contests;

namespace contester.Features.Users;

public class User : AuditableEntity
{
    public Guid Id { get; set; }
    
    [MaxLength(50)]
    public string FirstName { get; set; } = null!;
    
    [MaxLength(50)]
    public string LastName { get; set; } = null!;
    
    [MaxLength(50)]
    public string? Patronymic { get; set; }
    
    [MaxLength(150)]
    public string AdditionalInfo { get; set; } = null!;

    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; } = null!;
    
    [MaxLength(100)]
    public string PasswordHash { get; set; } = null!;
    public Guid PasswordRecoveryToken { get; set; }
    public DateTime PasswordRecoveryTokenExpiresAt { get; set; }
    public Guid EmailConfirmationToken { get; set; }
    public DateTime EmailConfirmationTokenExpiresAt { get; set; }
    [MaxLength(100)]
    public string EmailConfirmationCode { get; set; } = null!;
    public DateTime EmailConfirmationCodeExpiresAt { get; set; }
    public bool IsEmailConfirmed { get; set; }

    public int UserRoleId { get; set; }
    public UserRole UserRole { get; set; } = null!;

    /// <summary>
    /// Last login date UTC
    /// </summary>
    public DateTime LastLogin { get; set; }
    
    public List<Contest> ContestsUserParticipatesIn { get; set; } = null!;
    public List<Contest> AuthoredContests { get; set; } = null!;
    public List<ContestApplication> ContestApplications { get; set; } = null!;
}
