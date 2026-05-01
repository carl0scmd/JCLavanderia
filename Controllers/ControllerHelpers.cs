using Microsoft.AspNetCore.Mvc;

namespace JCLavanderia.Pedidos.Controllers;

internal static class ControllerHelpers
{
    public static string? CleanOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    public static string CleanRequired(string value) => value.Trim();

    public static bool IsBlank(string? value) => string.IsNullOrWhiteSpace(value);

    public static (int Page, int PageSize) NormalizePaging(int page, int pageSize) =>
        (Math.Max(page, 1), Math.Clamp(pageSize, 1, 100));

    public static BadRequestObjectResult BadRequestMessage(string message) =>
        new(new { message });
}
