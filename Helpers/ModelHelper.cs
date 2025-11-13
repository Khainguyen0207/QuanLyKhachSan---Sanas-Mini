using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json.Linq;
using QuanLyKhachSan.Models;

namespace QuanLyKhachSan.Helpers
{
    public class ModelHelper
    {
        public static Dictionary<string, string> CollectionModel(object model, FormCollection collection)
        {
            var props = model.GetType().GetProperties();
            var dict = new Dictionary<string, string>();

            foreach (var item in props)
            {
                string key = item.Name;
                string value = collection[key];

                if (value == null)
                {
                    continue;
                }

                dict[key] = value;
            }

            return dict;
        }

        public static T CreateModelFromCollection<T>(FormCollection collection) where T : new()
        {
            var model = new T();
            var props = typeof(T).GetProperties();

            Dictionary<string, string> dict = CollectionModel(model, collection);

            foreach (var prop in props)
            {
                if (dict.TryGetValue(prop.Name, out string value))
                {
                    value = value?.Trim();

                    if (string.IsNullOrEmpty(value))
                    {
                        continue;
                    }
                    
                    try
                    {
                        var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                        if (value == "on" || value == "off")
                        {
                            prop.SetValue(model, value == "on");
                            continue;
                        }

                        if (targetType == typeof(bool))
                        {
                            if (value == "0" || value == "1")
                            {
                                prop.SetValue(model, value == "1");
                                continue;
                            }
                        }

                        object convertedValue = Convert.ChangeType(value, targetType);
                        prop.SetValue(model, convertedValue);
                    }
                    catch (Exception ex) {
                        throw new Exception($"Key {prop.Name} không thể convert\nValue: {value}", ex);
                    }
                }
            }

            return model;
        }

        public static Dictionary<string, string> DirtyModelFromCollection<T>(T model, FormCollection collection) where T : new()
        {
            var props = model.GetType().GetProperties();

            Dictionary<string, string> changes = new Dictionary<string, string>();

            foreach (var prop in props)
            {
                if (CollectionModel(model, collection).TryGetValue(prop.Name, out string value))
                {
                    value = value?.Trim();

                    try
                    {
                        var valueOld = prop.GetValue(model);

                        if (prop.Name == "avatar")
                        {
                            var password = value;
                        }

                        if (value == "on" || value == "off")
                        {
                            if (isDirty(valueOld, value == "on"))
                            {
                                changes.Add(prop.Name, (value == "on").ToString());
                            }

                            continue;
                        }

                        if (value == "0" || value == "1")
                        {
                            if (isDirty(valueOld, value == "1"))
                            {
                                changes.Add(prop.Name, (value == "1").ToString());
                            }

                            continue;
                        }

                        if (isDirty(valueOld, value))
                        {
                            changes.Add(prop.Name, value);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                }
            }

            return changes;
        }


        private static bool isDirty(object a, object b)
        {
            if (a == null || b == null)
            {
                return true;
            }

            return a.ToString() != b.ToString();
        }
    }
}