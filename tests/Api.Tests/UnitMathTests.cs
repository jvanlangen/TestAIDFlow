using System.Text.Json;
using Api;

namespace Api.Tests;

public sealed class UnitMathTests
{
    [Theory]
    [InlineData(1500, "g", "kg", 1.5)]
    [InlineData(2.5, "l", "ml", 2500)]
    [InlineData(3, "pieces", "pieces", 3)]
    public void Converts_only_compatible_units(decimal quantity, string from, string to, decimal expected)
    {
        Assert.Equal(expected, UnitMath.Convert(quantity, from, to));
    }

    [Fact]
    public void Rejects_conversions_between_quantity_categories()
    {
        Assert.Throws<ArgumentException>(() => UnitMath.Convert(1, "g", "ml"));
    }

    [Fact]
    public void Minimum_package_count_keeps_required_amount_separate_and_finds_the_smallest_count()
    {
        var options = new[]
        {
            new PackageSizeDto(Guid.NewGuid(), 3, "kg"),
            new PackageSizeDto(Guid.NewGuid(), 1, "kg")
        };

        var combinations = UnitMath.MinimumPackageCombinations(2500, "g", options);

        var combination = Assert.Single(combinations);
        Assert.Equal("3 kg", Assert.Single(combination.Packages).Label);
        Assert.Equal(1, Assert.Single(combination.Packages).Count);
    }

    [Fact]
    public void Package_measure_equivalents_calculate_total_content_and_preserve_the_package_label()
    {
        var package = new PackageSizeDto(Guid.NewGuid(), 3, "g", "sachets", 18);

        var combinations = UnitMath.MinimumPackageCombinations(108, "g", [package]);

        Assert.Equal("3 sachets (54 g)", package.Label);
        Assert.Equal(2, Assert.Single(combinations).Packages.Single().Count);
        Assert.Equal(package.Label, Assert.Single(combinations).Packages.Single().Label);
    }

    [Fact]
    public void Returns_every_minimum_count_combination_that_covers_the_amount()
    {
        var options = new[]
        {
            new PackageSizeDto(Guid.NewGuid(), 1.5m, "kg"),
            new PackageSizeDto(Guid.NewGuid(), 1.4m, "kg"),
            new PackageSizeDto(Guid.NewGuid(), 1.3m, "kg"),
            new PackageSizeDto(Guid.NewGuid(), 1.2m, "kg")
        };

        var combinations = UnitMath.MinimumPackageCombinations(2.5m, "kg", options);

        Assert.Equal(9, combinations.Count);
        Assert.All(combinations, combination =>
        {
            Assert.Equal(2, combination.Packages.Sum(x => x.Count));
            Assert.True(combination.Packages.Sum(package =>
                options.Single(option => option.Label == package.Label).Quantity * package.Count) >= 2.5m);
        });
    }

    [Fact]
    public void No_package_options_produce_no_package_recommendation()
    {
        Assert.Empty(UnitMath.MinimumPackageCombinations(2.5m, "kg", []));
    }

    [Fact]
    public void Decimal_json_strings_preserve_exact_quantities_for_browser_editing()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new DecimalStringJsonConverter());

        var json = JsonSerializer.Serialize(0.125000m, options);
        var value = JsonSerializer.Deserialize<decimal>(json, options);

        Assert.Equal("\"0.125\"", json);
        Assert.Equal(0.125m, value);
    }

    [Fact]
    public void Decimal_json_strings_remove_only_redundant_trailing_zeroes()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new DecimalStringJsonConverter());

        Assert.Equal("\"2.5\"", JsonSerializer.Serialize(2.500m, options));
    }
}
