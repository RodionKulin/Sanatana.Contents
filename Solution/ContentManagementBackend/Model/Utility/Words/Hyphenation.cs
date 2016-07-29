//using System;
//using System.Collections.Generic;
//using System.Drawing;
//using System.Linq;
//using System.Text;
//using System.Web;

//namespace Common.Utility
//{
//    public class Hyphenation : IDisposable
//    {
//        Graphics _G;
//        int _МаксСтрок;
//        Font _Шрифт;
//        int _МаксШирина;


//        //инициализация
//        public Hyphenation(Font шрифт, int максШирина, int максСтрок = int.MaxValue)
//        {
//            _G = Graphics.FromImage(new Bitmap(1, 1));
//            _Шрифт = шрифт;
//            _МаксШирина = максШирина;
//            _МаксСтрок = максСтрок;
//        }

//        //IDisposable
//        public void Dispose()
//        {
//            _G.Dispose();
//        }


//        //метод
//        public string ПеренестиТекстПоСловам(string input, bool добавлятьПоБуквам)
//        {
//            int всегоСтрок = 0;
//            if (_МаксСтрок <= 0)
//                _МаксСтрок = int.MaxValue;
//            if (string.IsNullOrEmpty(input))
//                return string.Empty;

//            StringBuilder строкаСПереносами = new StringBuilder();
//            input = input.Replace("\r\n", string.Empty).Replace("\r", string.Empty);
//            string[] слова = input.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
//            int длинаТекста = 0;

//            if (string.IsNullOrEmpty(input))
//                return string.Empty;

//            всегоСтрок = 1;
//            for (int i = 0; i < слова.Length; i++)
//            {
//                int длинаСлова = (int)_G.MeasureString(слова[i], _Шрифт).Width;
                
//                // Добавить по буквам
//                if (добавлятьПоБуквам || длинаСлова > _МаксШирина)
//                {
//                    ДобавитьПоБуквам(слова[i], ref строкаСПереносами, ref всегоСтрок);
//                    if (всегоСтрок > _МаксСтрок)
//                    {
//                        всегоСтрок--;
//                        break;
//                    }
//                    ДобавитьПробел(строкаСПереносами, длинаТекста);
//                }
//                // Добавить слово целиком
//                else
//                {
//                    строкаСПереносами.Append(слова[i]);
//                    длинаТекста = (int)_G.MeasureString(строкаСПереносами.ToString(), _Шрифт).Width;

//                    // Начать новую строку
//                    if (длинаТекста > _МаксШирина)
//                    {
//                        int началоСлова = строкаСПереносами.Length - слова[i].Length;
//                        строкаСПереносами.Remove(началоСлова, слова[i].Length);
//                        всегоСтрок++;

//                        if (всегоСтрок > _МаксСтрок)
//                        {
//                            всегоСтрок--;
//                            break;
//                        }
//                        else
//                        {
//                            строкаСПереносами.Append("\r\n");
//                            i--;
//                        }
//                    }
//                    // Добавить пробел между словами
//                    else if (i + 1 < слова.Length)
//                        ДобавитьПробел(строкаСПереносами, длинаТекста);
//                }
//            }

//            return строкаСПереносами.ToString();
//        }

//        private void ДобавитьПробел(StringBuilder строкаСПереносами, int длинаТекста)
//        {
//            строкаСПереносами.Append(" ");
//            длинаТекста = (int)_G.MeasureString(строкаСПереносами.ToString(), _Шрифт).Width;
//            if (длинаТекста > _МаксШирина)
//            {
//                int началоСлова = строкаСПереносами.Length - 1;
//                строкаСПереносами.Remove(началоСлова, 1);
//            }
//        }

//        private void ДобавитьПоБуквам(string слово, ref StringBuilder строкаСПереносами, ref int всегоСтрок)
//        {
//            int длинаТекста = 0;

//            for (int i = 0; i < слово.Length; i++)
//            {
//                строкаСПереносами.Append(слово[i]);
//                длинаТекста = (int)_G.MeasureString(строкаСПереносами.ToString(), _Шрифт).Width;

//                if (длинаТекста > _МаксШирина)
//                {
//                    строкаСПереносами.Remove(i, 1);
//                    всегоСтрок++;

//                    if (всегоСтрок > _МаксСтрок)
//                    {
//                        break;
//                    }
//                    else
//                    {
//                        строкаСПереносами.Append("\r\n");
//                        i--;
//                    }
//                }
//            }

//        }

//    } 
//}