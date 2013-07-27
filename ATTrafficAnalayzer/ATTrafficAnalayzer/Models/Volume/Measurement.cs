using System.Collections.Generic;

namespace ATTrafficAnalayzer.Models.Volume
{
    public class Measurement
    {
        private int _value;
        private readonly List<string> _approaches = new List<string>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="approachName"></param>
        /// <returns></returns>
        public void CheckIfMax(int value, string approachName)
        {
            if (value > _value)
            {
                _value = value;
                _approaches.Clear();
                _approaches.Add(approachName);
            }
            else if (value == _value)
            {
                _approaches.Add(approachName);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetApproachesAsString()
        {
            return string.Join(", ", _approaches);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int GetValue()
        {
            return _value;
        }
    }
}
