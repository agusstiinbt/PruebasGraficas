using System.ComponentModel;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace CigoWeb.Core.Helpers;

public static class EnumHelper
{
    private static readonly BindingFlags EnumFieldBindingFlags = BindingFlags.Public | BindingFlags.Static;

    /// <summary>
    /// Converts an enum to a dictionary where each key is the numeric enum value and each value is the enum description (or name when no description exists).
    /// </summary>
    /// <typeparam name="T">Enum type to convert.</typeparam>
    /// <param name="replaceUnderscores">When <see langword="true"/>, replaces underscores with spaces in returned descriptions.</param>
    /// <returns>A dictionary keyed by integer enum value.</returns>
    public static Dictionary<int, string> EnumToDictionary<T>(bool replaceUnderscores = false) where T : Enum
    {
        var dict = new Dictionary<int, string>();
        foreach (var item in GetEnumValues<T>())
        {
            dict.Add(Convert.ToInt32(item), GetDisplayText(item, replaceUnderscores));
        }

        return dict;
    }

    /// <summary>
    /// Converts an enum to a dictionary where each key is the enum description (or name) and each value is the numeric enum value.
    /// </summary>
    /// <typeparam name="T">Enum type to convert.</typeparam>
    /// <param name="replaceUnderscores">When <see langword="true"/>, replaces underscores with spaces in keys.</param>
    /// <returns>A dictionary keyed by enum description or name.</returns>
    public static Dictionary<string, int> EnumToDictionaryInvert<T>(bool replaceUnderscores = false) where T : Enum
    {
        var dict = new Dictionary<string, int>();
        foreach (var item in GetEnumValues<T>())
        {
            dict.Add(GetDisplayText(item, replaceUnderscores), Convert.ToInt32(item));
        }

        return dict;
    }

    /// <summary>
    /// Converts an enum to a list of key/value objects where each key is the numeric enum value and each value is the enum description (or name).
    /// </summary>
    /// <typeparam name="T">Enum type to convert.</typeparam>
    /// <param name="replaceUnderscores">When <see langword="true"/>, replaces underscores with spaces in values.</param>
    /// <returns>List of key/value rows suitable for binding and serialization.</returns>
    public static List<GenericDictionary> EnumToListGeneric<T>(bool replaceUnderscores = false) where T : Enum
    {
        var list = new List<GenericDictionary>();
        foreach (var item in GetEnumValues<T>())
        {
            list.Add(new GenericDictionary
            {
                key = Convert.ToInt32(item),
                value = GetDisplayText(item, replaceUnderscores)
            });
        }

        return list;
    }

    /// <summary>
    /// Converts an enum to a list of key/value objects where each key is the enum description (or name) and each value is the numeric enum value.
    /// </summary>
    /// <typeparam name="T">Enum type to convert.</typeparam>
    /// <param name="replaceUnderscores">When <see langword="true"/>, replaces underscores with spaces in keys.</param>
    /// <returns>List of key/value rows suitable for binding and serialization.</returns>
    public static List<GenericDictionaryInverse> EnumToListGenericInvert<T>(bool replaceUnderscores = false) where T : Enum
    {
        var list = new List<GenericDictionaryInverse>();
        foreach (var item in GetEnumValues<T>())
        {
            list.Add(new GenericDictionaryInverse
            {
                key = GetDisplayText(item, replaceUnderscores),
                value = Convert.ToInt32(item)
            });
        }

        return list;
    }

    /// <summary>
    /// Creates a JSON object where keys are enum descriptions (or names) and values are numeric enum values.
    /// </summary>
    /// <typeparam name="T">Enum type to convert.</typeparam>
    /// <param name="useOriginalName">When <see langword="true"/>, uses enum names as keys instead of descriptions.</param>
    /// <param name="replaceUnderscores">When <see langword="true"/>, replaces underscores with spaces in keys.</param>
    /// <returns>Formatted JSON string.</returns>
    public static string EnumToJson<T>(bool useOriginalName = false, bool replaceUnderscores = false) where T : Enum
    {
        var obj = new JObject();
        foreach (var item in GetEnumValues<T>())
        {
            obj[GetDisplayText(item, replaceUnderscores, useOriginalName)] = Convert.ToInt32(item);
        }

        return obj.ToString();
    }

    /// <summary>
    /// Returns the description for an enum value. If a <see cref="DescriptionAttribute"/> is not present, the enum name is returned.
    /// </summary>
    /// <typeparam name="T">Enum type.</typeparam>
    /// <param name="enumValue">Enum value to resolve.</param>
    /// <returns>Description text, or enum member name when no description is present.</returns>
    public static string GetEnumDescription<T>(T enumValue) where T : Enum
    {
        return GetDisplayText(enumValue, replaceUnderscores: false);
    }

    /// <summary>
    /// Returns all enum descriptions for the provided enum type.
    /// </summary>
    /// <typeparam name="T">Enum type.</typeparam>
    /// <returns>List of descriptions ordered by enum declaration order.</returns>
    public static List<string> GetEnumDescriptions<T>() where T : Enum
    {
        return GetEnumValues<T>()
            .Select(GetEnumDescription)
            .ToList();
    }

    /// <summary>
    /// Finds the enum member whose description (or name) matches the provided text.
    /// </summary>
    /// <typeparam name="T">Enum type.</typeparam>
    /// <param name="text">Text to match against enum descriptions.</param>
    /// <param name="replaceUnderscores">When <see langword="true"/>, underscores in descriptions are replaced with spaces before comparison.</param>
    /// <returns>The matched enum value, or <see langword="default"/> when no match is found.</returns>
    public static T GetEnumFromText<T>(string text, bool replaceUnderscores = false) where T : struct, Enum
    {
        foreach (var item in GetEnumValues<T>())
        {
            if (string.Equals(GetDisplayText(item, replaceUnderscores), text, StringComparison.OrdinalIgnoreCase))
            {
                return item;
            }
        }

        return default;
    }

    /// <summary>
    /// Finds the enum member whose numeric value matches <paramref name="value"/>.
    /// </summary>
    /// <typeparam name="T">Enum type.</typeparam>
    /// <param name="value">Numeric enum value.</param>
    /// <returns>The matched enum value, or <see langword="default"/> when no match is found.</returns>
    public static T GetEnumFromValue<T>(int value) where T : struct, Enum
    {
        foreach (var item in GetEnumValues<T>())
        {
            if (Convert.ToInt32(item) == value)
            {
                return item;
            }
        }

        return default;
    }

    /// <summary>
    /// Finds the enum member whose name exactly matches <paramref name="value"/>.
    /// </summary>
    /// <typeparam name="T">Enum type.</typeparam>
    /// <param name="value">Enum member name to match.</param>
    /// <returns>The matched enum value, or <see langword="default"/> when no match is found.</returns>
    public static T GetEnumFromValue<T>(string value) where T : struct, Enum
    {
        foreach (var item in GetEnumValues<T>())
        {
            if (item.ToString() == value)
            {
                return item;
            }
        }

        return default;
    }

    /// <summary>
    /// Returns enum values filtered to single-bit flags when <typeparamref name="TEnum"/> is decorated with <see cref="FlagsAttribute"/>.
    /// For non-flag enums, all values are returned.
    /// </summary>
    /// <typeparam name="TEnum">Enum type.</typeparam>
    /// <returns>Single-flag values sorted by numeric value, or all values for non-flag enums.</returns>
    public static IReadOnlyList<TEnum> GetSingleFlagValues<TEnum>() where TEnum : struct, Enum
    {
        var type = typeof(TEnum);
        var values = GetEnumValues<TEnum>().ToArray();

        var isFlags = type.GetCustomAttribute<FlagsAttribute>() is not null;
        if (!isFlags)
        {
            return values;
        }

        return values
            .Where(v =>
            {
                var n = Convert.ToUInt64(v);
                return n != 0 && IsPowerOfTwo(n);
            })
            .OrderBy(v => Convert.ToUInt64(v))
            .ToArray();
    }

    #region Private Helpers

    private static IEnumerable<object> GetSingleFlagValues(Type enumType)
    {
        var values = Enum.GetValues(enumType).Cast<object>().ToArray();

        var isFlags = enumType.GetCustomAttribute<FlagsAttribute>() is not null;
        if (!isFlags)
            return values;

        return values
            .Where(v =>
            {
                var n = Convert.ToUInt64(v);
                return n != 0 && IsPowerOfTwo(n);
            })
            .OrderBy(v => Convert.ToUInt64(v))
            .ToArray();
    }

    private static bool IsPowerOfTwo(ulong n) => (n & (n - 1)) == 0;

    private static string GetFieldDescriptionOrName(Type enumType, string fieldName)
    {
        var field = enumType.GetField(fieldName, EnumFieldBindingFlags);
        return field?.GetCustomAttribute<DescriptionAttribute>()?.Description ?? fieldName;
    }

    private static IEnumerable<TEnum> GetEnumValues<TEnum>() where TEnum : Enum
    {
        return Enum.GetValues(typeof(TEnum)).Cast<TEnum>();
    }

    private static string GetDisplayText<TEnum>(TEnum value, bool replaceUnderscores, bool useOriginalName = false) where TEnum : Enum
    {
        var enumValue = (Enum)(object)value;
        var text = useOriginalName ? enumValue.ToString() : enumValue.ToDescription();

        if (replaceUnderscores)
        {
            text = text.Replace("_", " ");
        }

        return text;
    }

    #endregion

    #region Extension Methods

    /// <summary>
    /// Returns the display description for an enum value.
    /// For flag combinations, known single-bit flags are expanded and joined with ", ".
    /// </summary>
    /// <param name="value">Enum value to resolve.</param>
    /// <returns>Description text suitable for UI display.</returns>
    public static string GetDescription(this Enum value)
    {
        var type = value.GetType();
        var numeric = Convert.ToUInt64(value);

        // Exact named value (single value or defined combined name)
        var exactName = Enum.GetName(type, Enum.ToObject(type, numeric));
        if (exactName is not null)
            return GetFieldDescriptionOrName(type, exactName);

        var isFlags = type.GetCustomAttribute<FlagsAttribute>() is not null;
        if (!isFlags)
            return Enum.ToObject(type, numeric).ToString() ?? string.Empty;

        // Flags combination: decompose into single-bit values
        if (numeric == 0)
            return string.Empty;

        var parts = new List<string>();
        ulong knownMask = 0;

        foreach (var flagObj in GetSingleFlagValues(type))
        {
            var flagValue = Convert.ToUInt64(flagObj);
            if (flagValue == 0) continue;

            knownMask |= flagValue;

            if ((numeric & flagValue) == flagValue)
            {
                parts.Add(((Enum)flagObj).GetDescription());
            }
        }

        // Unknown bits set => fall back to default ToString() to avoid lying
        var unknownBits = numeric & ~knownMask;
        if (unknownBits != 0)
        {
            return Enum.ToObject(type, numeric).ToString() ?? string.Empty;
        }

        return parts.Count > 0 ? string.Join(", ", parts) : (Enum.ToObject(type, numeric).ToString() ?? string.Empty);
    }

    /// <summary>
    /// Gets the icon class from <see cref="LocalizedDescriptionAttribute"/> for a named enum value.
    /// </summary>
    /// <param name="value">Enum value to resolve.</param>
    /// <param name="defaultIconClass">Fallback icon class when no icon metadata is present.</param>
    /// <returns>Localized icon class, or <paramref name="defaultIconClass"/>.</returns>
    public static string GetIconClass(this Enum value, string defaultIconClass)
    {
        var type = value.GetType();
        var numeric = Convert.ToUInt64(value);
        var name = Enum.GetName(type, Enum.ToObject(type, numeric));
        if (name is null)
        {
            return defaultIconClass;
        }

        var field = type.GetField(name, EnumFieldBindingFlags);
        return field?.GetCustomAttribute<LocalizedDescriptionAttribute>()?.IconClass ?? defaultIconClass;
    }

    private static string ToDescription(this Enum enumValue)
    {
        Type type = enumValue.GetType();

        MemberInfo[] memInfo = type.GetMember(enumValue.ToString());

        if (memInfo != null && memInfo.Length > 0)
        {
            object[] attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attrs != null && attrs.Length > 0) { return ((DescriptionAttribute)attrs[0]).Description; }
        }

        return enumValue.ToString();
    }

    /// <summary>
    /// Converts an Enum on its equivalent Integuer value.
    /// </summary>
    /// <param name="objEnum">Enumerador a convertir.</param>
    /// <returns></returns>
    public static int ToInt(this Enum objEnum)
    {
        return Convert.ToInt32(objEnum);
    }



    #endregion

    #region DTO Types



    /// <summary>
    /// Simple DTO representing numeric enum value and display text.
    /// </summary>
    public class GenericDictionary
    {
        /// <summary>
        /// Integer value
        /// </summary>
        public int key { get; set; }

        /// <summary>
        /// String description
        /// </summary>
        public string value { get; set; } = string.Empty;
    }

    /// <summary>
    /// Simple DTO representing display text and numeric enum value.
    /// </summary>
    public class GenericDictionaryInverse
    {
        /// <summary>
        /// String description.
        /// </summary>
        public string key { get; set; } = string.Empty;

        /// <summary>
        /// Integer value.
        /// </summary>
        public int value { get; set; }
    }

    #endregion
}
