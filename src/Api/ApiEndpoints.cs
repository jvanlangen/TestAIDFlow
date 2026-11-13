using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace Api;

public static class ApiEndpoints
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static void Map(WebApplication app)
    {
        app.MapGet("/api/health", async (AppDbContext db, CancellationToken token) =>
            await db.Database.CanConnectAsync(token) ? Results.Ok(new { status = "ok" }) : Results.StatusCode(503));

        app.MapGet("/api/app", GetAppState);
        app.MapPost("/api/ingredients", CreateIngredient);
        app.MapPut("/api/ingredients/{id:guid}", RenameIngredient);
        app.MapDelete("/api/ingredients/{id:guid}", DeleteIngredient);
        app.MapPost("/api/ingredients/{id:guid}/packages", AddPackageSize);
        app.MapDelete("/api/ingredients/{ingredientId:guid}/packages/{packageId:guid}", DeletePackageSize);
        app.MapPost("/api/recipe-measures", CreateRecipeMeasure);
        app.MapPut("/api/recipe-measures/{id:guid}", RenameRecipeMeasure);
        app.MapPut("/api/ingredients/{ingredientId:guid}/recipe-measures/{measureId:guid}", SetMeasureEquivalent);
        app.MapPost("/api/meals", (MealRequest request, AppDbContext db, CancellationToken token) =>
            SaveMeal(null, request, db, token));
        app.MapPut("/api/meals/{id:guid}", (Guid id, MealRequest request, AppDbContext db, CancellationToken token) =>
            SaveMeal(id, request, db, token));
        app.MapDelete("/api/meals/{id:guid}", DeleteMeal);
        app.MapPost("/api/grocery-lists", CreateGroceryList);
        app.MapPut("/api/grocery-lists/{id:guid}/items", UpdateGroceryListItems);
        app.MapPost("/api/grocery-lists/{id:guid}/items", AddGroceryListItem);
        app.MapDelete("/api/grocery-lists/{id:guid}", DeleteGroceryList);
    }

    private static async Task<IResult> GetAppState(AppDbContext db, CancellationToken token)
    {
        var ingredients = await db.Ingredients
            .Include(x => x.PackageSizes)
            .Include(x => x.MeasureEquivalents)
            .OrderBy(x => x.Name)
            .ToListAsync(token);
        var measures = await db.RecipeMeasures.OrderBy(x => x.Name).ToListAsync(token);
        var meals = await db.Meals.Include(x => x.Ingredients).OrderBy(x => x.Name).ToListAsync(token);
        var lists = await db.GroceryLists.OrderByDescending(x => x.CreatedAt).ToListAsync(token);
        var measureNames = measures.ToDictionary(x => x.Id, x => x.Name);
        var ingredientNames = ingredients.ToDictionary(x => x.Id, x => x.Name);

        return Results.Ok(new AppStateDto(
            ingredients.Select(x => ToDto(x, measureNames)).ToArray(),
            measures.Select(x => new RecipeMeasureDto(x.Id, x.Name)).ToArray(),
            meals.Select(x => ToDto(x, ingredientNames, measureNames)).ToArray(),
            lists.Select(ToDto).ToArray()));
    }

    private static async Task<IResult> CreateIngredient(
        CreateIngredientRequest request,
        AppDbContext db,
        CancellationToken token)
    {
        var name = CleanName(request.Name, 120);
        if (name is null)
            return Invalid("name", "Enter an ingredient name of 1–120 characters.");
        if (QuantityUnits.Category(request.BaseUnit) is null)
            return Invalid("baseUnit", "Choose g, kg, ml, l or pieces.");
        if (await db.Ingredients.AnyAsync(x => x.Name.ToLower() == name.ToLower(), token))
            return Conflict("An ingredient with that name already exists.");

        var ingredient = new Ingredient { Name = name, BaseUnit = request.BaseUnit };
        db.Ingredients.Add(ingredient);
        await db.SaveChangesAsync(token);
        return Results.Created($"/api/ingredients/{ingredient.Id}", ToDto(ingredient, new Dictionary<Guid, string>()));
    }

    private static async Task<IResult> RenameIngredient(
        Guid id,
        RenameRequest request,
        AppDbContext db,
        CancellationToken token)
    {
        var ingredient = await db.Ingredients
            .Include(x => x.PackageSizes)
            .Include(x => x.MeasureEquivalents)
            .FirstOrDefaultAsync(x => x.Id == id, token);
        if (ingredient is null)
            return Results.NotFound();

        var name = CleanName(request.Name, 120);
        if (name is null)
            return Invalid("name", "Enter an ingredient name of 1–120 characters.");
        if (await db.Ingredients.AnyAsync(x => x.Id != id && x.Name.ToLower() == name.ToLower(), token))
            return Conflict("An ingredient with that name already exists.");

        ingredient.Name = name;
        await db.SaveChangesAsync(token);
        var measureNames = await db.RecipeMeasures.ToDictionaryAsync(x => x.Id, x => x.Name, token);
        return Results.Ok(ToDto(ingredient, measureNames));
    }

    private static async Task<IResult> DeleteIngredient(Guid id, AppDbContext db, CancellationToken token)
    {
        var ingredient = await db.Ingredients.FindAsync([id], token);
        if (ingredient is null)
            return Results.NotFound();
        if (await db.MealIngredients.AnyAsync(x => x.IngredientId == id, token))
            return Conflict("This ingredient is used in a meal. Remove it from the meal before deleting it.");

        db.Ingredients.Remove(ingredient);
        await db.SaveChangesAsync(token);
        return Results.NoContent();
    }

    private static async Task<IResult> AddPackageSize(
        Guid id,
        CreatePackageSizeRequest request,
        AppDbContext db,
        CancellationToken token)
    {
        var ingredient = await db.Ingredients.FindAsync([id], token);
        if (ingredient is null)
            return Results.NotFound();
        if (request.Quantity <= 0)
            return Invalid("quantity", "Package quantity must be greater than zero.");
        if (request.RecipeMeasureId is Guid measureId)
        {
            if (request.Unit is not null)
                return Invalid("unit", "Choose either an underlying unit or a recipe measure.");
            var measure = await db.RecipeMeasures.FindAsync([measureId], token);
            if (measure is null)
                return Invalid("recipeMeasureId", "Choose an existing recipe measure.");
            var equivalent = await db.IngredientMeasureEquivalents
                .FindAsync([id, measureId], token);
            if (equivalent is null)
                return Invalid("recipeMeasureId", $"Define the {measure.Name} equivalent for {ingredient.Name} first.");

            var measuredPackage = new PackageSize
            {
                IngredientId = id,
                Quantity = request.Quantity,
                RecipeMeasureId = measureId
            };
            db.PackageSizes.Add(measuredPackage);
            await db.SaveChangesAsync(token);
            return Results.Created(
                $"/api/ingredients/{id}/packages/{measuredPackage.Id}",
                new PackageSizeDto(
                    measuredPackage.Id,
                    measuredPackage.Quantity,
                    equivalent.Unit,
                    measure.Name,
                    equivalent.Quantity));
        }

        if (request.Unit is null || !IsCompatible(ingredient.BaseUnit, request.Unit))
            return Invalid("unit", $"Choose a unit compatible with {ingredient.Name} ({ingredient.BaseUnit}).");

        var packageSize = new PackageSize
        {
            IngredientId = id,
            Quantity = request.Quantity,
            Unit = request.Unit
        };
        db.PackageSizes.Add(packageSize);
        await db.SaveChangesAsync(token);
        return Results.Created(
            $"/api/ingredients/{id}/packages/{packageSize.Id}",
            new PackageSizeDto(packageSize.Id, packageSize.Quantity, packageSize.Unit));
    }

    private static async Task<IResult> DeletePackageSize(
        Guid ingredientId,
        Guid packageId,
        AppDbContext db,
        CancellationToken token)
    {
        var packageSize = await db.PackageSizes
            .FirstOrDefaultAsync(x => x.Id == packageId && x.IngredientId == ingredientId, token);
        if (packageSize is null)
            return Results.NotFound();
        db.PackageSizes.Remove(packageSize);
        await db.SaveChangesAsync(token);
        return Results.NoContent();
    }

    private static async Task<IResult> CreateRecipeMeasure(
        CreateMeasureRequest request,
        AppDbContext db,
        CancellationToken token)
    {
        var name = CleanName(request.Name, 80);
        if (name is null)
            return Invalid("name", "Enter a measure name of 1–80 characters.");
        if (await db.RecipeMeasures.AnyAsync(x => x.Name.ToLower() == name.ToLower(), token))
            return Conflict("A recipe measure with that name already exists.");

        var measure = new RecipeMeasure { Name = name };
        db.RecipeMeasures.Add(measure);
        await db.SaveChangesAsync(token);
        return Results.Created($"/api/recipe-measures/{measure.Id}", new RecipeMeasureDto(measure.Id, measure.Name));
    }

    private static async Task<IResult> RenameRecipeMeasure(
        Guid id,
        RenameRequest request,
        AppDbContext db,
        CancellationToken token)
    {
        var measure = await db.RecipeMeasures.FindAsync([id], token);
        if (measure is null)
            return Results.NotFound();
        var name = CleanName(request.Name, 80);
        if (name is null)
            return Invalid("name", "Enter a measure name of 1–80 characters.");
        if (await db.RecipeMeasures.AnyAsync(x => x.Id != id && x.Name.ToLower() == name.ToLower(), token))
            return Conflict("A recipe measure with that name already exists.");

        measure.Name = name;
        await db.SaveChangesAsync(token);
        return Results.Ok(new RecipeMeasureDto(measure.Id, measure.Name));
    }

    private static async Task<IResult> SetMeasureEquivalent(
        Guid ingredientId,
        Guid measureId,
        MeasureEquivalentRequest request,
        AppDbContext db,
        CancellationToken token)
    {
        var ingredient = await db.Ingredients.FindAsync([ingredientId], token);
        var measure = await db.RecipeMeasures.FindAsync([measureId], token);
        if (ingredient is null || measure is null)
            return Results.NotFound();
        if (request.Quantity <= 0)
            return Invalid("quantity", "Equivalent quantity must be greater than zero.");
        if (!IsCompatible(ingredient.BaseUnit, request.Unit))
            return Invalid("unit", $"Choose a unit compatible with {ingredient.Name} ({ingredient.BaseUnit}).");

        var equivalent = await db.IngredientMeasureEquivalents.FindAsync([ingredientId, measureId], token);
        if (equivalent is null)
        {
            equivalent = new IngredientMeasureEquivalent
            {
                IngredientId = ingredientId,
                RecipeMeasureId = measureId,
                Quantity = request.Quantity,
                Unit = request.Unit
            };
            db.IngredientMeasureEquivalents.Add(equivalent);
        }
        else
        {
            equivalent.Quantity = request.Quantity;
            equivalent.Unit = request.Unit;
        }

        await db.SaveChangesAsync(token);
        return Results.Ok(new MeasureEquivalentDto(measureId, measure.Name, equivalent.Quantity, equivalent.Unit));
    }

    private static async Task<IResult> SaveMeal(
        Guid? id,
        MealRequest request,
        AppDbContext db,
        CancellationToken token)
    {
        var name = CleanName(request.Name, 120);
        if (name is null)
            return Invalid("name", "Enter a meal name of 1–120 characters.");
        if (request.Ingredients is null || request.Ingredients.Count == 0)
            return Invalid("ingredients", "Add at least one ingredient.");
        if (await db.Meals.AnyAsync(x => x.Id != id && x.Name.ToLower() == name.ToLower(), token))
            return Conflict("A meal with that name already exists.");

        var ingredientIds = request.Ingredients.Select(x => x.IngredientId).Distinct().ToArray();
        var ingredients = await db.Ingredients
            .Where(x => ingredientIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, token);
        var measureIds = request.Ingredients
            .Where(x => x.RecipeMeasureId.HasValue)
            .Select(x => x.RecipeMeasureId!.Value)
            .Distinct()
            .ToArray();
        var measures = await db.RecipeMeasures.Where(x => measureIds.Contains(x.Id)).ToListAsync(token);
        var equivalents = await db.IngredientMeasureEquivalents
            .Where(x => ingredientIds.Contains(x.IngredientId) && measureIds.Contains(x.RecipeMeasureId))
            .ToListAsync(token);

        for (var index = 0; index < request.Ingredients.Count; index++)
        {
            var line = request.Ingredients[index];
            var key = $"ingredients[{index}]";
            if (line.Quantity <= 0)
                return Invalid($"{key}.quantity", "Quantity must be greater than zero.");
            if (!ingredients.TryGetValue(line.IngredientId, out var ingredient))
                return Invalid($"{key}.ingredientId", "Choose an existing ingredient.");

            var directUnit = line.Unit is not null && line.RecipeMeasureId is null;
            var recipeMeasure = line.Unit is null && line.RecipeMeasureId is not null;
            if (directUnit && !IsCompatible(ingredient.BaseUnit, line.Unit))
                return Invalid($"{key}.unit", $"Choose a unit compatible with {ingredient.Name}.");
            if (!directUnit && !recipeMeasure)
                return Invalid(key, "Choose either one supported unit or one recipe measure.");
            if (recipeMeasure && (!measures.Any(x => x.Id == line.RecipeMeasureId) ||
                                  !equivalents.Any(x => x.IngredientId == line.IngredientId &&
                                                        x.RecipeMeasureId == line.RecipeMeasureId)))
                return Invalid(key, $"Define this recipe measure's exact equivalent for {ingredient.Name} before using it.");
        }

        var meal = id is null
            ? new Meal { Name = name }
            : await db.Meals.Include(x => x.Ingredients).FirstOrDefaultAsync(x => x.Id == id, token);
        if (meal is null)
            return Results.NotFound();
        if (id is not null)
            db.MealIngredients.RemoveRange(meal.Ingredients);

        meal.Name = name;
        meal.Ingredients = request.Ingredients.Select(x => new MealIngredient
        {
            IngredientId = x.IngredientId,
            Quantity = x.Quantity,
            Unit = x.Unit,
            RecipeMeasureId = x.RecipeMeasureId
        }).ToList();
        if (id is null)
            db.Meals.Add(meal);

        await db.SaveChangesAsync(token);
        var ingredientNames = ingredients.ToDictionary(x => x.Key, x => x.Value.Name);
        var measureNames = measures.ToDictionary(x => x.Id, x => x.Name);
        return Results.Ok(ToDto(meal, ingredientNames, measureNames));
    }

    private static async Task<IResult> DeleteMeal(Guid id, AppDbContext db, CancellationToken token)
    {
        var meal = await db.Meals.FindAsync([id], token);
        if (meal is null)
            return Results.NotFound();
        db.Meals.Remove(meal);
        await db.SaveChangesAsync(token);
        return Results.NoContent();
    }

    private static async Task<IResult> CreateGroceryList(
        CalculateGroceryListRequest request,
        AppDbContext db,
        CancellationToken token)
    {
        var name = CleanName(request.Name, 160);
        if (name is null)
            return Invalid("name", "Enter a list name of 1–160 characters.");
        if (request.Meals is null || request.Meals.Count == 0)
            return Invalid("meals", "Select at least one meal.");
        if (request.Meals.Any(x => x.PeopleCount <= 0))
            return Invalid("meals", "People counts must be whole numbers greater than zero.");
        if (request.Meals.Select(x => x.MealId).Distinct().Count() != request.Meals.Count)
            return Invalid("meals", "Select each meal only once and adjust its people count.");

        var mealIds = request.Meals.Select(x => x.MealId).ToArray();
        var meals = await db.Meals.Include(x => x.Ingredients)
            .Where(x => mealIds.Contains(x.Id))
            .ToListAsync(token);
        if (meals.Count != mealIds.Length)
            return Invalid("meals", "One or more selected meals no longer exist.");

        var ingredientIds = meals.SelectMany(x => x.Ingredients).Select(x => x.IngredientId).Distinct().ToArray();
        var ingredients = await db.Ingredients.Where(x => ingredientIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, token);
        var measureIds = meals.SelectMany(x => x.Ingredients)
            .Where(x => x.RecipeMeasureId.HasValue)
            .Select(x => x.RecipeMeasureId!.Value)
            .Distinct()
            .ToArray();
        var equivalents = await db.IngredientMeasureEquivalents
            .Where(x => ingredientIds.Contains(x.IngredientId) && measureIds.Contains(x.RecipeMeasureId))
            .ToDictionaryAsync(x => (x.IngredientId, x.RecipeMeasureId), token);
        var totals = new Dictionary<Guid, decimal>();

        foreach (var selection in request.Meals)
        {
            var meal = meals.Single(x => x.Id == selection.MealId);
            foreach (var line in meal.Ingredients)
            {
                var ingredient = ingredients[line.IngredientId];
                var quantity = line.Quantity;
                var unit = line.Unit;
                if (line.RecipeMeasureId is Guid measureId)
                {
                    if (!equivalents.TryGetValue((line.IngredientId, measureId), out var equivalent))
                        return Conflict($"The recipe measure equivalent for {ingredient.Name} must be restored before this list can be calculated.");
                    quantity *= equivalent.Quantity;
                    unit = equivalent.Unit;
                }

                if (unit is null || !IsCompatible(ingredient.BaseUnit, unit))
                    return Conflict($"A recipe quantity for {ingredient.Name} uses an incompatible unit.");
                var baseQuantity = UnitMath.Convert(quantity, unit, ingredient.BaseUnit) * selection.PeopleCount;
                totals[ingredient.Id] = totals.GetValueOrDefault(ingredient.Id) + baseQuantity;
            }
        }

        var selectedIds = request.SelectedPackageSizeIds?.Distinct().ToArray() ?? [];
        var packageSizes = await db.PackageSizes.Where(x => selectedIds.Contains(x.Id)).ToListAsync(token);
        if (packageSizes.Count != selectedIds.Length)
            return Invalid("selectedPackageSizeIds", "One or more selected package sizes no longer exist.");
        if (packageSizes.Any(x => !totals.ContainsKey(x.IngredientId)))
            return Invalid("selectedPackageSizeIds", "Choose package sizes only for ingredients in the selected meals.");

        var packageDtos = await ToPackageSizeDtosAsync(packageSizes, db, token);
        var packageDtosById = packageDtos.ToDictionary(x => x.Id);
        var selectedByIngredient = packageSizes.GroupBy(x => x.IngredientId)
            .ToDictionary(x => x.Key, x => x.Select(p => packageDtosById[p.Id]).ToArray());
        var items = totals
            .OrderBy(x => ingredients[x.Key].Name)
            .Select(x =>
            {
                var ingredient = ingredients[x.Key];
                var options = selectedByIngredient.GetValueOrDefault(x.Key, []);
                return new GroceryItemDto(
                    Guid.NewGuid(),
                    ingredient.Id,
                    ingredient.Name,
                    x.Value,
                    ingredient.BaseUnit,
                    options,
                    UnitMath.MinimumPackageCombinations(x.Value, ingredient.BaseUnit, options));
            })
            .ToArray();
        var selections = request.Meals.Select(selection =>
        {
            var meal = meals.Single(x => x.Id == selection.MealId);
            return new MealSelectionDto(meal.Id, meal.Name, selection.PeopleCount);
        }).ToArray();
        var groceryList = new GroceryList
        {
            Name = name,
            SelectionsJson = JsonSerializer.Serialize(selections, JsonOptions),
            ItemsJson = JsonSerializer.Serialize(items, JsonOptions)
        };
        db.GroceryLists.Add(groceryList);
        await db.SaveChangesAsync(token);
        return Results.Created($"/api/grocery-lists/{groceryList.Id}", ToDto(groceryList));
    }

    private static async Task<IResult> UpdateGroceryListItems(
        Guid id,
        UpdateGroceryListItemsRequest request,
        AppDbContext db,
        CancellationToken token)
    {
        var groceryList = await db.GroceryLists.FindAsync([id], token);
        if (groceryList is null)
            return Results.NotFound();
        if (request.Items is null || request.Items.Any(x => x.Quantity <= 0))
            return Invalid("items", "Item quantities must be greater than zero.");
        if (request.Items.Select(x => x.Id).Distinct().Count() != request.Items.Count)
            return Invalid("items", "Each grocery item can appear only once.");

        var items = Deserialize<GroceryItemDto[]>(groceryList.ItemsJson);
        var byId = items.ToDictionary(x => x.Id);
        if (request.Items.Any(x => !byId.ContainsKey(x.Id)))
            return Invalid("items", "One or more grocery items do not belong to this list.");

        var updated = request.Items.Select(change =>
        {
            var item = byId[change.Id];
            return item with
            {
                Quantity = change.Quantity,
                PackageCombinations = UnitMath.MinimumPackageCombinations(
                    change.Quantity, item.Unit, item.PackageOptions)
            };
        }).ToArray();
        groceryList.ItemsJson = JsonSerializer.Serialize(updated, JsonOptions);
        await db.SaveChangesAsync(token);
        return Results.Ok(ToDto(groceryList));
    }

    private static async Task<IResult> AddGroceryListItem(
        Guid id,
        AddGroceryListItemRequest request,
        AppDbContext db,
        CancellationToken token)
    {
        var groceryList = await db.GroceryLists.FindAsync([id], token);
        if (groceryList is null)
            return Results.NotFound();
        if (request.Quantity <= 0)
            return Invalid("quantity", "Item quantity must be greater than zero.");

        var ingredient = await db.Ingredients.FindAsync([request.IngredientId], token);
        if (ingredient is null)
            return Invalid("ingredientId", "Choose an existing ingredient.");
        var selectedIds = request.SelectedPackageSizeIds?.Distinct().ToArray() ?? [];
        var packages = await db.PackageSizes
            .Where(x => selectedIds.Contains(x.Id) && x.IngredientId == ingredient.Id)
            .ToListAsync(token);
        if (packages.Count != selectedIds.Length)
            return Invalid("selectedPackageSizeIds", "Choose package sizes belonging to this ingredient.");

        var options = await ToPackageSizeDtosAsync(packages, db, token);
        var item = new GroceryItemDto(
            Guid.NewGuid(),
            ingredient.Id,
            ingredient.Name,
            request.Quantity,
            ingredient.BaseUnit,
            options,
            UnitMath.MinimumPackageCombinations(request.Quantity, ingredient.BaseUnit, options));
        var items = Deserialize<GroceryItemDto[]>(groceryList.ItemsJson).Append(item).ToArray();
        groceryList.ItemsJson = JsonSerializer.Serialize(items, JsonOptions);
        await db.SaveChangesAsync(token);
        return Results.Created($"/api/grocery-lists/{id}/items/{item.Id}", item);
    }

    private static async Task<IResult> DeleteGroceryList(Guid id, AppDbContext db, CancellationToken token)
    {
        var groceryList = await db.GroceryLists.FindAsync([id], token);
        if (groceryList is null)
            return Results.NotFound();
        db.GroceryLists.Remove(groceryList);
        await db.SaveChangesAsync(token);
        return Results.NoContent();
    }

    private static IngredientDto ToDto(Ingredient ingredient, IReadOnlyDictionary<Guid, string> measureNames)
    {
        var packageSizes = ingredient.PackageSizes
            .Select(package => ToPackageSizeDto(package, ingredient, measureNames))
            .OrderBy(package => UnitMath.Convert(
                package.Quantity * (package.EquivalentQuantity ?? 1m),
                package.Unit ?? throw new InvalidOperationException("A package size has no underlying unit."),
                ingredient.BaseUnit))
            .ToArray();
        return new(
            ingredient.Id,
            ingredient.Name,
            ingredient.BaseUnit,
            packageSizes,
            ingredient.MeasureEquivalents
                .Where(x => measureNames.ContainsKey(x.RecipeMeasureId))
                .Select(x => new MeasureEquivalentDto(
                    x.RecipeMeasureId, measureNames[x.RecipeMeasureId], x.Quantity, x.Unit)).ToArray());
    }

    private static PackageSizeDto ToPackageSizeDto(
        PackageSize package,
        Ingredient ingredient,
        IReadOnlyDictionary<Guid, string> measureNames)
    {
        if (package.RecipeMeasureId is not Guid measureId)
            return new(
                package.Id,
                package.Quantity,
                package.Unit ?? throw new InvalidOperationException("A package size has no unit."));

        if (!measureNames.TryGetValue(measureId, out var measureName))
            throw new InvalidOperationException("A package size references an unknown recipe measure.");
        var equivalent = ingredient.MeasureEquivalents.SingleOrDefault(x => x.RecipeMeasureId == measureId)
            ?? throw new InvalidOperationException("A package size has no ingredient-specific measure equivalent.");
        return new(package.Id, package.Quantity, equivalent.Unit, measureName, equivalent.Quantity);
    }

    private static async Task<PackageSizeDto[]> ToPackageSizeDtosAsync(
        IReadOnlyCollection<PackageSize> packages,
        AppDbContext db,
        CancellationToken token)
    {
        var ingredientIds = packages.Select(x => x.IngredientId).Distinct().ToArray();
        if (ingredientIds.Length == 0)
            return [];
        var measureIds = packages
            .Where(x => x.RecipeMeasureId.HasValue)
            .Select(x => x.RecipeMeasureId!.Value)
            .Distinct()
            .ToArray();
        var packageIngredients = await db.Ingredients
            .Where(x => ingredientIds.Contains(x.Id))
            .Include(x => x.MeasureEquivalents)
            .ToDictionaryAsync(x => x.Id, token);
        var measureNames = await db.RecipeMeasures
            .Where(x => measureIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Name, token);
        return packages.Select(package => ToPackageSizeDto(
            package,
            packageIngredients[package.IngredientId],
            measureNames)).ToArray();
    }

    private static MealDto ToDto(
        Meal meal,
        IReadOnlyDictionary<Guid, string> ingredientNames,
        IReadOnlyDictionary<Guid, string> measureNames) =>
        new(
            meal.Id,
            meal.Name,
            meal.Ingredients.Select(x => new MealIngredientDto(
                x.Id,
                x.IngredientId,
                ingredientNames.GetValueOrDefault(x.IngredientId, "Unknown ingredient"),
                x.Quantity,
                x.Unit,
                x.RecipeMeasureId,
                x.RecipeMeasureId is Guid measureId ? measureNames.GetValueOrDefault(measureId) : null)).ToArray());

    private static GroceryListDto ToDto(GroceryList list) =>
        new(
            list.Id,
            list.Name,
            list.CreatedAt,
            Deserialize<MealSelectionDto[]>(list.SelectionsJson),
            Deserialize<GroceryItemDto[]>(list.ItemsJson));

    private static T Deserialize<T>(string json) =>
        JsonSerializer.Deserialize<T>(json, JsonOptions)
        ?? throw new InvalidOperationException("Stored grocery-list data is invalid.");

    private static bool IsCompatible(string baseUnit, string? unit) =>
        unit is not null && QuantityUnits.Category(baseUnit) == QuantityUnits.Category(unit);

    private static string? CleanName(string? value, int maxLength)
    {
        var name = value?.Trim();
        return string.IsNullOrWhiteSpace(name) || name.Length > maxLength ? null : name;
    }

    private static IResult Invalid(string key, string message) =>
        Results.ValidationProblem(new Dictionary<string, string[]> { [key] = [message] });

    private static IResult Conflict(string message) => Results.Conflict(new { error = message });
}
