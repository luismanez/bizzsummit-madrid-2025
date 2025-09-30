using System;
using System.ComponentModel;
using System.Text.Json;
using Microsoft.SemanticKernel;

namespace Abyx.Orchestrator;

public sealed class CalculatorTool
{
    [KernelFunction("compute_yearly_cost")]
    [Description("Compute yearly cost given a monthly EUR price and seats.")]
    public Task<string> ComputeYearlyAsync(
        [Description("Monthly EUR per seat")] double eurPerMonth,
        [Description("Number of seats")] int seats)
    {
        var monthly = eurPerMonth * seats;
        var yearly = monthly * 12;
        var result = new { source = "Calculator", monthlyEur = monthly, yearlyEur = yearly };
        return Task.FromResult(JsonSerializer.Serialize(result));
    }
}
