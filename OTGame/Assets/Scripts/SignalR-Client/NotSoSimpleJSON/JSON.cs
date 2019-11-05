/*
 * Created by Arshia001
 * Based heavily on http://wiki.unity3d.com/index.php/SimpleJSON by Bunny83
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NotSoSimpleJSON
{
    public static class JSON
    {
        public static JSONNode Parse(string aJSON)
        {
            return JSONNode.Parse(aJSON);
        }

        public static JSONNode FromData(object data)
        {
            if (data == null)
                return new JSONNull();
            if (data is JSONNode)
                return (JSONNode)data;
            if (data is IJSONSerializable)
                return ((IJSONSerializable)data).ToJson();
            if (data is int || data is int?
                || data is short || data is short?
                || data is byte || data is byte?
                || data is uint || data is uint?
                || data is ushort || data is ushort?
                || data is sbyte || data is sbyte?
                || data is Enum e)
                return new JSONInt(Convert.ToInt32(data));
            if (data is float || data is float? || data is double || data is double?)
                return new JSONDouble(Convert.ToDouble(data));
            if (data is bool || data is bool?)
                return new JSONBool(Convert.ToBoolean(data));
            if (data is string)
                return new JSONString((string)data);
            if (data is IEnumerable)
            {
                var Result = new JSONArray();
                foreach (var Item in (IEnumerable)data)
                    Result.Add(FromData(Item));
                return Result;
            }
            if (data is IDictionary)
            {
                var Result = new JSONObject();
                var Dic = (IDictionary)data;
                foreach (var Key in Dic.Keys)
                    Result.Add(Key.ToString(), FromData(Dic[Key]));
                return Result;
            }
            if (data is Vector2Int ipt)
                return FromData(new { X = ipt.x, Y = ipt.y });
            if (data is Vector2 pt)
                return FromData(new { X = pt.x, Y = pt.y });
            if (data is Vector3 pt3)
                return FromData(new { X = pt3.x, Y = pt3.z });
            if (data is object)
            {
                var result = new JSONObject();
                foreach (var prop in data.GetType().GetProperties())
                    result.Add(prop.Name, FromData(prop.GetValue(data)));
                foreach (var field in data.GetType().GetFields())
                    result.Add(field.Name, FromData(field.GetValue(data)));
                return result;
            }

            throw new NotSupportedException();
        }
    }
}
