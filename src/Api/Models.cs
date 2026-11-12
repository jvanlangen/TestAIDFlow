using System.Globalization;

namespace Api;

public static class QuantityUnits
{
    public static readonly string[] All = ["g", "kg", "ml", "l", "pieces"];

    public static string? Category(string? unit) => unit switch
    {
        "g" or "kg" => "mass",
        "ml" or "l" => "volume",
        "pieces" => "pieces",
        _ => null
    };
}

public sealed class Ingredient
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public required string BaseUnit { get; set; }
    public List<PackageSize> PackageSizes { get; set; } = [];
    public List<IngredientMeasureEquivalent> MeasureEquivalents { get; set; } = [];
}

public sealed class PackageSize
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid IngredientId { get; set; }
    public decimal Quantity { get; set; }
    public string? Unit { get; set; }
    public Guid? RecipeMeasureId { get; set; }
}

public sealed class RecipeMeasure
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public List<IngredientMeasureEquivalent> Equivalents { get; set; } = [];
}

public sealed class IngredientMeasureEquivalent
{
    public Guid IngredientId { get; set; }
    public Guid RecipeMeasureId { get; set; }
    public decimal Quantity { get; set; }
    public required string Unit { get; set; }
}

public sealed class Meal
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public List<MealIngredient> Ingredients { get; set; } = [];
}

public sealed class MealIngredient
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MealId { get; set; }
    public Guid IngredientId { get; set; }
    public decimal Quantity { get; set; }
    public string? Unit { get; set; }
    public Guid? RecipeMeasureId { get; set; }
}

public sealed class GroceryList
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public required string SelectionsJson { get; set; }
    public required string ItemsJson { get; set; }
}

public sealed record PackageSizeDto(
    Guid Id,
    decimal Quantity,
    string? Unit,
    string? MeasureName = null,
    decimal? EquivalentQuantity = null)
{
    public string Label => MeasureName is null
        ? $"{Quantity.ToString("G29", CultureInfo.InvariantCulture)} {Unit}"
        : $"{Quantity.ToString("G29", CultureInfo.InvariantCulture)} {MeasureName} ({(Quantity * EquivalentQuantity!.Value).ToString("G29", CultureInfo.InvariantCulture)} {Unit})";
}

public sealed record MeasureEquivalentDto(Guid RecipeMeasureId, string MeasureName, decimal Quantity, string Unit);

public sealed record IngredientDto(
    Guid Id,
    string Name,
    string BaseUnit,
    IReadOnlyList<PackageSizeDto> PackageSizes,
    IReadOnlyList<MeasureEquivalentDto> MeasureEquivalents);

public sealed record RecipeMeasureDto(Guid Id, string Name);

public sealed record MealIngredientDto(
    Guid Id,
    Guid IngredientId,
    string IngredientName,
    decimal Quantity,
    string? Unit,
    Guid? RecipeMeasureId,
    string? MeasureName);

public sealed record MealDto(Guid Id, string Name, IReadOnlyList<MealIngredientDto> Ingredients);

public sealed record MealSelectionDto(Guid MealId, string MealName, int PeopleCount);

public sealed record PackageCountDto(string Label, long Count);

public sealed record PackageCombinationDto(IReadOnlyList<PackageCountDto> Packages);

public sealed record GroceryItemDto(
    Guid Id,
    Guid IngredientId,
    string IngredientName,
    decimal Quantity,
    string Unit,
    IReadOnlyList<PackageSizeDto> PackageOptions,
    IReadOnlyList<PackageCombinationDto> PackageCombinations);

public sealed record GroceryListDto(
    Guid Id,
    string Name,
    DateTimeOffset CreatedAt,
    IReadOnlyList<MealSelectionDto> Selections,
    IReadOnlyList<GroceryItemDto> Items);

public sealed record AppStateDto(
    IReadOnlyList<IngredientDto> Ingredients,
    IReadOnlyList<RecipeMeasureDto> RecipeMeasures,
    IReadOnlyList<MealDto> Meals,
    IReadOnlyList<GroceryListDto> GroceryLists);

public sealed record CreateIngredientRequest(string Name, string BaseUnit);
public sealed record RenameRequest(string Name);
public sealed record CreatePackageSizeRequest(decimal Quantity, string? Unit, Guid? RecipeMeasureId = null);
public sealed record CreateMeasureRequest(string Name);
public sealed record MeasureEquivalentRequest(decimal Quantity, string Unit);
public sealed record MealIngredientRequest(
    Guid IngredientId,
    decimal Quantity,
    string? Unit,
    Guid? RecipeMeasureId);
public sealed record MealRequest(string Name, IReadOnlyList<MealIngredientRequest> Ingredients);
public sealed record MealSelectionRequest(Guid MealId, int PeopleCount);
public sealed record CalculateGroceryListRequest(
    string Name,
    IReadOnlyList<MealSelectionRequest> Meals,
    IReadOnlyList<Guid> SelectedPackageSizeIds);
public sealed record GroceryItemQuantityRequest(Guid Id, decimal Quantity);
public sealed record UpdateGroceryListItemsRequest(IReadOnlyList<GroceryItemQuantityRequest> Items);
public sealed record AddGroceryListItemRequest(Guid IngredientId, decimal Quantity, IReadOnlyList<Guid> SelectedPackageSizeIds);
