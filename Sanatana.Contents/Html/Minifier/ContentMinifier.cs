using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Contents.Html.Minifier
{
    public class ContentMinifier
    {

        //methods
        public static string Minify(string htmlText, int targetLength, ContentMinifyMode mode)
        {
            Dictionary<int, string> tags = new Dictionary<int, string>();
            
            StringBuilder builder = RemoveTags(htmlText, tags);
            
            builder = MinifyText(builder, targetLength, mode);
            
            builder = RestoreTags(tags, builder);

            return builder.ToString();
        }

        private static StringBuilder MinifyText(
            StringBuilder builder, int targetLength, ContentMinifyMode mode)
        {
            if (builder.Length <= targetLength)
            {
                return builder;
            }
            
            if (mode != ContentMinifyMode.Strict)
            {
                targetLength = FindCutPosition(builder, targetLength, mode);
            }
            
            if (builder.Length > targetLength)
            {
                int lengthToCut = builder.Length - targetLength;
                builder = builder.Remove(targetLength, lengthToCut);
            }

            return builder;
        }

        private static int FindCutPosition(
            StringBuilder builder, int targetLength, ContentMinifyMode mode)
        {
            int? индексТочкиСправа = null;
            int? индексТочкиСлева = null;
            int? расстояниеДоТочкиСправа = null;
            int? расстояниеДоТочкиСлева = null;

            // ищем точку справа
            for (int i = targetLength; i < builder.Length; i++)
            {
                if (builder[i] == '.')
                {
                    индексТочкиСправа = i;
                    расстояниеДоТочкиСправа = i - targetLength;
                    break;
                }
            }

            // ищем точку слева
            if (mode == ContentMinifyMode.ToClosestDot)
            {
                for (int i = targetLength; i >= 0; i--)
                {
                    if (builder[i] == '.')
                    {
                        индексТочкиСлева = i;
                        расстояниеДоТочкиСлева = targetLength - i;
                        break;
                    }
                }
            }

            // ищем ближайшую точку
            if (индексТочкиСправа != null || индексТочкиСлева != null)
            {
                if (индексТочкиСправа == null || расстояниеДоТочкиСлева < расстояниеДоТочкиСправа)
                {
                    targetLength = индексТочкиСлева.Value + 1;
                }
                else
                {
                    targetLength = индексТочкиСправа.Value + 1;
                }
            }

            return targetLength;
        }

        private static StringBuilder RemoveTags(string htmlText, Dictionary<int, string> tags)
        {
            StringBuilder builder = new StringBuilder(htmlText);
            
            int? индексЗакрытия = null;

            for (int i = htmlText.Length - 1; i >= 0; i--)
            {
                if (htmlText[i] == '>')
                {
                    индексЗакрытия = i;
                }
                else if (htmlText[i] == '<' && индексЗакрытия != null)
                {
                    int длина = индексЗакрытия.Value - i + 1;
                    string тег = htmlText.Substring(i, длина);
                    tags.Add(i, тег);

                    builder = builder.Remove(i, длина);

                    индексЗакрытия = null;
                }
            }

            return builder;
        }

        private static StringBuilder RestoreTags(Dictionary<int, string> tags, StringBuilder builder)
        {
            List<MinifyHtmlTag> незакрытыеТеги = new List<MinifyHtmlTag>();
            int[] позицииТегов = tags.Keys.ToArray();

            for (int i = позицииТегов.Length - 1; i >= 0; i--)
            {
                int позиция = позицииТегов[i];
                if (позицииТегов[i] < builder.Length)
                {
                    builder = builder.Insert(позицииТегов[i], tags[позиция]);

                    // Изменяем список незакрытых тегов
                    MinifyHtmlTag новыйТег = new MinifyHtmlTag(tags[позиция]);
                    if (!string.IsNullOrEmpty(новыйТег.Name))
                    {
                        if (новыйТег.MinifyTagType == MinifyTagType.Opening)
                        {
                            незакрытыеТеги.Add(новыйТег);
                        }
                        else if (новыйТег.MinifyTagType == MinifyTagType.Closing)
                        {
                            int индексОткрытогоТега = незакрытыеТеги.FindLastIndex(p => p.Name == новыйТег.Name);
                            if (индексОткрытогоТега != -1)
                                незакрытыеТеги.RemoveAt(индексОткрытогоТега);
                        }
                    }
                }
                else
                {
                    break;
                }
            }

            for (int i = незакрытыеТеги.Count - 1; i >= 0; i--)
            {
                builder.Append(незакрытыеТеги[i].CreateClosingTag());
            }

            return builder;
        }

    }
}
