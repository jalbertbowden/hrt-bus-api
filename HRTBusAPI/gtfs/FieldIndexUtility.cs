using System;
using System.Globalization;

namespace HRTBusAPI.gtfs
{
    public static class FieldIndexUtility
    {
        public static T CreateIndex<T>(string fields)
        {
            var obj = (T)Activator.CreateInstance(typeof(T));

            var propertyNames = fields.Split(',');
            var textInfo = new CultureInfo("en-US", false).TextInfo;
            for (int i = 0; i < propertyNames.Length; i++)
            {
                var propName = textInfo.ToTitleCase(propertyNames[i].Replace("_", " ")).Replace(" ", "");
                var prop = obj.GetType().GetProperty(propName);
                prop.SetValue(obj, i, null);
            }

            return obj;
        }
    }
}