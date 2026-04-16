using System.ComponentModel;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace PruebasGraficas.Classes.Helper
{
    public static class EnumManager
    {
        /// <summary>
        /// Converts a Enum to a dictionary with the value of the Enum as key and the description as value.If Replace_ is true, it replaces the "_" with " " in the description.T is the Enum to convert.
        /// </summary>
        /// <typeparam name="T">The Enum to convert</typeparam>
        /// <param name="Replace_"></param>
        /// <returns></returns>
        public static Dictionary<int, string> EnumToDictionary<T>(bool Replace_ = false) where T : Enum
        {
            var dict = new Dictionary<int, string>();

            foreach (var item in Enum.GetValues(typeof(T)))
            {
                string value = ((Enum)item).ToDescription();

                if (Replace_)
                    value = value.Replace("_", " ");

                dict.Add((int)item, value);
            }

            return dict;
        }

        /// <summary>
        /// Converts a Enum to a dictionary with the description of the Enum as key and the value as value.If Replace_ is true, it replaces the "_" with " " in the description. T is the Enum to convert.
        /// </summary>
        /// <typeparam name="T">The Enum To Convert</typeparam>
        /// <param name="Replace_"></param>
        /// <returns></returns>
        public static Dictionary<string, int> EnumToDictionaryInvert<T>(bool Replace_ = false) where T : Enum
        {
            var dict = new Dictionary<string, int>();

            foreach (var item in Enum.GetValues(typeof(T)))
            {
                string value = ((Enum)item).ToDescription();

                if (Replace_)
                    value = value.Replace("_", " ");

                dict.Add(value.ToString(), (int)item);
            }

            return dict;
        }

        /// <summary>
        /// Returns the description of a Enum value. If the Enum value does not have a description, it returns the name of the Enum value. T is the Enum type and enumValue is the Enum value to get the description from.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumValue"></param>
        /// <returns></returns>
        public static string GetEnumDescription<T>(T enumValue) where T : Enum
        {
            // [Description("Accounting & Finance")]
            // Accounting = 256
            //If description exists it returns Accounting & Finance
            var fieldInfo = typeof(T).GetField(enumValue.ToString());
            var attributes = fieldInfo?.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];
            return attributes?.FirstOrDefault()?.Description ?? enumValue.ToString();
        }

        /// <summary>
        /// Returns a list of Descriptions of a given Enum. If an Enum value does not have a description, it returns the name of the Enum Value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<string> GetEnumDescriptions<T>() where T : Enum
        {
            return Enum.GetValues(typeof(T))
                .OfType<T>()
                .Select(e => GetEnumDescription(e))
                .ToList();
        }

        /// <summary>
        /// Returns a list of GenericDictionary with the value of the Enum as key and the description as value. If the Enum value does not have a description it returns the name of the value. If Replace_ is true, it replaces the "_" with " " in the description. T is the Enum to convert.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Replace_"></param>
        /// <returns></returns>
        public static List<GenericDictionary> EnumToListGeneric<T>(bool Replace_ = false) where T : Enum
        {
            var list = new List<GenericDictionary>();

            foreach (var item in Enum.GetValues(typeof(T)))
            {
                string value = ((Enum)item).ToDescription();

                if (Replace_)
                    value = value.Replace("_", " ");

                list.Add(new GenericDictionary() { key = (int)item, value = value });
            }

            return list;
        }

        /// <summary>
        /// Returns a list of GenericDictionaryInverse with the description of the Enum as key and the value as value. If the Enum value does not have a description it returns the name of the value. If Replace_ is true, it replaces the "_" with " " in the description. T is the Enum to convert.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Replace_"></param>
        /// <returns></returns>
        public static List<GenericDictionaryInverse> EnumToListGenericInvert<T>(bool Replace_ = false)
        {
            var list = new List<GenericDictionaryInverse>();

            foreach (var item in Enum.GetValues(typeof(T)))
            {
                string value = ((Enum)item).ToDescription();

                if (Replace_)
                    value = value.Replace("_", " ");

                list.Add(new GenericDictionaryInverse() { key = value.ToString(), value = (int)item });
            }

            return list;
        }

        /// <summary>
        /// Returns the Enum value of the given Enum T that matches the given text parameter. It only seaarches the description of the Enum values. If Replace_ is true, it replaces the "_" with " " in the description before comparing it with the text parameter. T is the Enum to search and text is the text to compare with the description of the Enum values.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="text"></param>
        /// <param name="Replace_"></param>
        /// <returns></returns>
        public static T GetEnumFromText<T>(string text, bool Replace_ = false)
        {
            var dict = new Dictionary<int, string>();

            foreach (var item in Enum.GetValues(typeof(T)))
            {
                string value = ((Enum)item).ToDescription();

                if (Replace_)
                    value = value.Replace("_", " ");

                if (value.ToLower() == text.ToLower())
                    return (T)item;

                dict.Add((int)item, value);
            }

            return default(T);
        }

        /// <summary>
        /// Returns the Enum value of the given Enum T that matches the given integer parameter. It only searches the value of the Enum values. T is the Enum to search and Value is the integer to compare with the value of the Enum values. Returns a default value (0) of T if no match is found. If multiple matches are found, it returns the first match. If T is not an Enum, it will throw an exception.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Value"></param>
        /// <returns></returns>
        public static T GetEnumFromValue<T>(int Value) where T : Enum
        {
            var dict = new Dictionary<int, string>();

            foreach (var item in Enum.GetValues(typeof(T)))
            {
                if ((int)item == Value)
                    return (T)item;
            }

            return default(T);
        }

        /// <summary>
        /// Returns the Enum Value of the given Enum T that matches the given string parameter.It doesnt search the description of the Enum Values, it only searches the name of the Enum Values. T is the Enum to search and Value is the string to compare with the name of the Enum values. Returns a default value (0) of T if no match is found. If multiple matches are found, it returns the first match.  
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Value"></param>
        /// <returns></returns>
        public static T GetEnumFromValue<T>(string Value)
        {
            foreach (var item in Enum.GetValues(typeof(T)))
            {
                var enumValue = (Enum)item;
                var enumDescription = enumValue.ToString();

                if (enumDescription == Value)
                {
                    return (T)item;
                }
            }

            return default(T);
        }

        /// <summary>
        /// It returns a Json of a given Enum T with the description of the Enum values as key and the value of the Enum values as value. If Original is true, it returns the name of the Enum values instead of the description. If Replace_ is true, it replaces the "_" with " " in the description. T is the Enum to convert. The Json is returned as a string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Original"></param>
        /// <param name="Replace_"></param>
        /// <returns></returns>
        public static string EnumToJson<T>(bool Original = false, bool Replace_ = false) where T : Enum
        {
            var obj = new JObject();

            foreach (var item in Enum.GetValues(typeof(T)))
            {
                string value = Original ? ((Enum)item).ToString() : ((Enum)item).ToDescription();

                if (Replace_)
                    value = value.Replace("_", " ");

                obj[value] = (int)item;
            }

            return obj.ToString();
        }

        /// <summary>
        /// Converts an Enum on its equivalent Integuer value.
        /// </summary>
        /// <param name="objEnum">Enumerador a convertir.</param>
        /// <returns></returns>
        private static int ToInt(this Enum objEnum)
        {
            return Convert.ToInt32(objEnum);
        }

        private static string ToDescription(this Enum objEnum) //ext method
        {
            Type type = objEnum.GetType();

            MemberInfo[] memInfo = type.GetMember(objEnum.ToString());

            if (memInfo != null && memInfo.Length > 0)
            {
                object[] attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (attrs != null && attrs.Length > 0) { return ((DescriptionAttribute)attrs[0]).Description; }
            }

            return objEnum.ToString();
        }

        public class GenericDictionary
        {
            /// <summary>
            /// Integer value
            /// </summary>
            public int key { get; set; }

            /// <summary>
            /// String description
            /// </summary>
            public string value { get; set; }
        }

        public class GenericDictionaryInverse
        {
            public string key { get; set; }

            public int value { get; set; }
        }

        public class Generic
        {
            public string key { get; set; }
            public string value { get; set; }
        }

    }
}
