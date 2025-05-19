using System.Text.RegularExpressions;

namespace Application.Validation
{
    public static partial class GlobalValidator
    {
        private static readonly Dictionary<string, (double? Min, double? Max)> SensorRanges = new()
        {
            { "Temperature", (-40, 130) },
            { "Humidity", (0, 100) },
            { "AirPressure", (0.00001, null) },
            { "AirQuality", (0, 2000) }
        };

        public static bool IsValidSensorType(string sensorType) =>
            !string.IsNullOrEmpty(sensorType) && SensorRanges.ContainsKey(sensorType);

        private static bool IsValueInRange(string sensorType, double value)
        {
            if (!SensorRanges.TryGetValue(sensorType, out var range))
                return false;

            var aboveMin = !range.Min.HasValue || value >= range.Min.Value;
            var belowMax = !range.Max.HasValue || value <= range.Max.Value;
            return aboveMin && belowMax;
        }

        private static bool AreBothValuesInRange(string sensorType, double val1, double val2)
            => IsValueInRange(sensorType, val1) && IsValueInRange(sensorType, val2);

        public static bool IsConditionFormatValid(string condition, string sensorType)
        {
            if (string.IsNullOrEmpty(condition)) return false;

            if (sensorType == "Temperature")
            {
                return SingleConditionTempRegex().IsMatch(condition)
                       || RangeConditionTempRegex().IsMatch(condition);
            }

            return SingleConditionOtherRegex().IsMatch(condition);
        }

        public static bool IsConditionValueInRange(string sensorType, string condition)
        {
            if (!IsConditionFormatValid(condition, sensorType)) return false;

            if (sensorType == "Temperature")
            {
                if (SingleConditionTempRegex().Match(condition) is { Success: true } match1)
                {
                    return double.TryParse(match1.Groups[2].Value, out var value) && IsValueInRange(sensorType, value);
                }

                if (RangeConditionTempRegex().Match(condition) is not { Success: true } match2) return false;
                if (!double.TryParse(match2.Groups[1].Value, out var val1)) return false;
                if (!double.TryParse(match2.Groups[3].Value, out var val2)) return false;

                var min = Math.Min(val1, val2);
                var max = Math.Max(val1, val2);
                return AreBothValuesInRange(sensorType, min, max);

            }

            {
                if (SingleConditionOtherRegex().Match(condition) is not { Success: true } match) return false;
                return double.TryParse(match.Groups[2].Value, out var value) && IsValueInRange(sensorType, value);
            }
        }

        // Temperature single condition allows optional minus
        [GeneratedRegex(@"^(<=|>=)(-?\d+(\.\d+)?)$")]
        private static partial Regex SingleConditionTempRegex();

        // Temperature range condition allows negative numbers on both sides
        [GeneratedRegex(@"^(-?\d+(\.\d+)?)-(-?\d+(\.\d+)?)$")]
        private static partial Regex RangeConditionTempRegex();

        // Other sensors single condition only positive numbers (no minus)
        [GeneratedRegex(@"^(<=|>=)(\d+(\.\d+)?)$")]
        private static partial Regex SingleConditionOtherRegex();
    }
}