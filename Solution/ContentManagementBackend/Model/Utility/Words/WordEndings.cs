using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ContentManagementBackend
{
    public class WordEndings
    {                    
        enum EndingVariant
        { 
            /// <summary>
            /// x1
            /// </summary>
            One,
            /// <summary>
            /// 2-4 / !10-20
            /// </summary>
            EndsWith2_4, 
            /// <summary>
            /// !x1 + !EndsWith2_4
            /// </summary>
            AnyElse
        };


        //методы
        private static EndingVariant DetermineEndingVariant(int number)
        {
            EndingVariant variant;
            string numberString = number.ToString();

            if (!(numberString.Length > 1 && numberString[numberString.Length - 2] == '1') &&
                (numberString.Last() == '2' || numberString.Last() == '3' || numberString.Last() == '4'))
            {
                variant = EndingVariant.EndsWith2_4;
            }
            else if (numberString.Last() == '1')
            {
                variant = EndingVariant.One;
            }
            else
            {
                variant = EndingVariant.AnyElse;
            }
            return variant;
        }

        public static string DetermineEnding(int number, List<string> threeEndings = null)
        {
            if (threeEndings == null)
            {
                threeEndings = new List<string>(3) { "", "а", "ов" };
            }
            else if (threeEndings.Count < 3)
            {
                while (threeEndings.Count < 3)
                    threeEndings.Add(string.Empty);
            }

            EndingVariant variant = DetermineEndingVariant(number);

            if (variant == EndingVariant.One)
                return threeEndings[0];
            else if (variant == EndingVariant.EndsWith2_4)
                return threeEndings[1];
            else
                return threeEndings[2];
        }
    }
}