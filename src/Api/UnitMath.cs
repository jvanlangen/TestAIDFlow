using System.Globalization;

namespace Api;

public static class UnitMath
{
    public static decimal Convert(decimal quantity, string fromUnit, string toUnit)
    {
        var fromCategory = QuantityUnits.Category(fromUnit);
        var toCategory = QuantityUnits.Category(toUnit);
        if (fromCategory is null || fromCategory != toCategory)
            throw new ArgumentException($"Cannot convert {fromUnit} to {toUnit}.");

        return quantity * Factor(fromUnit) / Factor(toUnit);
    }

    public static IReadOnlyList<PackageCombinationDto> MinimumPackageCombinations(
        decimal required,
        string targetUnit,
        IReadOnlyList<PackageSizeDto> packageOptions)
    {
        if (required <= 0 || packageOptions.Count == 0)
            return [];

        var options = packageOptions
            .Select(option => new
            {
                Option = option,
                Quantity = Convert(option.Quantity, option.Unit, targetUnit)
            })
            .Where(x => x.Quantity > 0)
            .OrderByDescending(x => x.Quantity)
            .ToArray();

        if (options.Length == 0)
            return [];

        var largest = options[0].Quantity;
        var minimumCountDecimal = decimal.Ceiling(required / largest);
        if (minimumCountDecimal > long.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(required), "The required package count is too large.");

        var minimumCount = (long)minimumCountDecimal;
        var combinations = new List<PackageCombinationDto>();
        var counts = new long[options.Length];

        void Enumerate(int optionIndex, long remainingSlots, decimal total)
        {
            if (optionIndex == options.Length - 1)
            {
                counts[optionIndex] = remainingSlots;
                if (total + remainingSlots * options[optionIndex].Quantity >= required)
                {
                    var packages = counts
                        .Select((count, index) => new PackageCountDto(options[index].Option.Label, count))
                        .Where(x => x.Count > 0)
                        .ToArray();
                    combinations.Add(new PackageCombinationDto(packages));
                }
                return;
            }

            var quantity = options[optionIndex].Quantity;
            if (total + remainingSlots * quantity < required)
                return;

            for (var count = remainingSlots; count >= 0; count--)
            {
                var nextSlots = remainingSlots - count;
                var nextTotal = total + count * quantity;
                if (nextTotal + nextSlots * options[optionIndex + 1].Quantity < required)
                    break;

                counts[optionIndex] = count;
                Enumerate(optionIndex + 1, nextSlots, nextTotal);
            }
        }

        Enumerate(0, minimumCount, 0);
        return combinations;
    }

    public static string Format(decimal value) => value.ToString(CultureInfo.InvariantCulture);

    private static decimal Factor(string unit) => unit switch
    {
        "g" or "ml" or "pieces" => 1,
        "kg" or "l" => 1000,
        _ => throw new ArgumentException($"Unsupported unit: {unit}.")
    };
}
