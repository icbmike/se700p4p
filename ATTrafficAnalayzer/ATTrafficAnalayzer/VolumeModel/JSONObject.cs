using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ATTrafficAnalayzer.VolumeModel
{
    public class JSONObject
    {
        private Dictionary<String, Object> _dict;
        
        public JSONObject(){
            _dict = new Dictionary<string,object>();
        }

        public void Add(String key, Object obj)
        {
            if(obj == null)
            {
                _dict.Remove(key);
            }else
            {
                _dict.Add(key, obj);
            }

        }
        public JSONObject GetJSONObject(String key)
        {
            if(_dict.ContainsKey(key)){
                return _dict[key] as JSONObject;
            }else{
                return null;
            }
        }

        public String GetString(String key)
        {
             if(_dict.ContainsKey(key)){
                return _dict[key] as String;
            }else{
                return null;
            }
        }

        public int? GetInt(String key)
        {
             if(_dict.ContainsKey(key)){
                return _dict[key] as int?;
            }else{
                return null;
            }
        }

        public double? GetDouble(String key)
        {
             if(_dict.ContainsKey(key)){
                return _dict[key] as double?;
            }else{
                return null;
            }
        }

        public bool? GetBoolean(String key)
        {
            if (_dict.ContainsKey(key))
            {
                return _dict[key] as bool?;
            }
            else
            {
                return null;
            }
        }

        public JSONArray GetJSONArray(String key)
        {
            if (_dict.ContainsKey(key))
            {
                return _dict[key] as JSONArray;
            }
            else
            {
                return null;
            }
        }

        public List<String> GetKeys()
        {
            return _dict.Keys.ToList();
        }

        public bool ContainsKey(String key)
        {
            return _dict.ContainsKey(key);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            var keys = _dict.Keys.ToList();
            for (int i = 0; i < keys.Count; i++ )
            {
                sb.Append(keys[i]);
                sb.Append(":");
                if (_dict[keys[i]].GetType() == typeof(String))
                {
                    sb.Append('"');
                }
                sb.Append(_dict[keys[i]].ToString());
                if (_dict[keys[i]].GetType() == typeof(String))
                {
                    sb.Append('"');
                }
                if(i != keys.Count -1) sb.Append(",");
            }
            sb.Append("}");
            return sb.ToString();
        }
    }
}
