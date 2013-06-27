using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ATTrafficAnalayzer.VolumeModel
{


    public class JSONArray
    {
        List<Object> _list;

        public JSONArray()
        {
            _list = new List<object>();
        }

        public void Put(int index, object obj)
        {
            _list.Insert(index, obj);
        }

        public int? GetInt(int index)
        {
            return _list[index] as int?;
        }

        public bool? GetBoolean(int index)
        {
            return _list[index] as bool?;
        }

        public double? GetDouble(int index)
        {
            return _list[index] as double?;
        }

        public JSONObject GetJSONObject(int index)
        {
            return _list[index] as JSONObject;
        }

        public JSONArray GetJSONArray(int index)
        {
            return _list[index] as JSONArray;
        }

        public int Count()
        {
            return _list.Count;
        }

        public void Remove(int index)
        {
            _list.RemoveAt(index);
        }

        internal void Put(Object obj)
        {
            _list.Add(obj);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            for (int i = 0; i < _list.Count; i++)
            {
                if (_list[i].GetType() == typeof(String))
                {
                    sb.Append('"');
                }
                sb.Append(_list[i].ToString());
                if (_list[i].GetType() == typeof(String))
                {
                    sb.Append('"');
                }
                if (i != _list.Count - 1)
                {
                    sb.Append(",");
                }
            }
            sb.Append("]");
            return sb.ToString();
        }
    }
}
