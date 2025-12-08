namespace Backend.DTOs;

public record PlannedDto(decimal PlannedAmount);
public record EveningDto(decimal ActualAmount, bool StuckToPlan, string? Reflection = null);
