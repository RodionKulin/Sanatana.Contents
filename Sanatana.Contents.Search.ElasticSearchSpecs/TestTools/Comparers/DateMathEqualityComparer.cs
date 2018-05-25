using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Contents.Search.ElasticSearchSpecs.TestTools.Comparers
{
    public class DateMathEqualityComparer : IEqualityComparer<DateMath>
    {
        public bool Equals(DateMath x, DateMath y)
        {
            if (x == null && y == null)
            {
                return true;
            }

            if (x == null || y == null)
            {
                return false;
            }

            string xValue = x.ToString();
            string yValue = y.ToString();
            return xValue == yValue;
        }

        public int GetHashCode(DateMath obj)
        {
            string value = obj == null
                ? string.Empty
                : obj.ToString();
            return value.GetHashCode();
        }
    }
}
