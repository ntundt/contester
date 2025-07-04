﻿using System.ComponentModel.DataAnnotations;
using diploma.Features.Contests;
using diploma.Features.SchemaDescriptions;

namespace diploma.Features.Problems;

public class Problem
{
    public Guid Id { get; set; }
    [MaxLength(255)]
    public string Name { get; set; } = null!;
    [MaxLength(255)]
    public string StatementPath { get; set; } = null!;
    
    public bool OrderMatters { get; set; }
    public decimal FloatMaxDelta { get; set; }
    public bool CaseSensitive { get; set; }
    public TimeSpan TimeLimit { get; set; }
    public int MaxGrade { get; set; }
    public int Ordinal { get; set; }
    
    public Guid ContestId { get; set; }
    public Contest Contest { get; set; } = null!;
    public Guid SchemaDescriptionId { get; set; }
    public SchemaDescription SchemaDescription { get; set; } = null!;
    [MaxLength(255)]
    public string SolutionPath { get; set; } = null!;
    [MaxLength(255)]
    public string SolutionDbms { get; set; } = null!;
}
