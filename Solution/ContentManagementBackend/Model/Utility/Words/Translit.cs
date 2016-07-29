using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace ContentManagementBackend
{
    public static class Translit
    {
        // Запрещены символы ыа ыу ые Ъ Ь ' ` _
        
        //поля
        public enum SymbolType { Numbers, WhiteSpace, Letters, SpecialSymbols }
        static Dictionary<string, string> _iso = new Dictionary<string, string>(); //ISO 9-95
      

        //инициализация
        static Translit()
        {
            _iso.Add(" ", "-");
            
            _iso.Add("ч", "ch");
            _iso.Add("ц", "c"); //cz

            _iso.Add("щ", "shh");
            _iso.Add("ш", "sh");
            _iso.Add("с", "s");

            _iso.Add("ё", "yo");
            _iso.Add("я", "ya");
            _iso.Add("ю", "yu");
            _iso.Add("э", "ye"); //e`
            //_iso.Add("ы", "y'");
            _iso.Add("ы", "y");  //y`
            _iso.Add("а", "a");
            _iso.Add("у", "u");
            _iso.Add("е", "e");

            _iso.Add("ж", "zh");
            _iso.Add("з", "z");

            _iso.Add("б", "b");
            _iso.Add("в", "v");
            _iso.Add("г", "g");
            _iso.Add("д", "d");
            _iso.Add("и", "i");
            _iso.Add("й", "j");
            _iso.Add("к", "k");
            _iso.Add("л", "l");
            _iso.Add("м", "m");
            _iso.Add("н", "n");
            _iso.Add("о", "o");
            _iso.Add("п", "p");
            _iso.Add("р", "r");
            _iso.Add("т", "t");
            _iso.Add("ф", "f");
            _iso.Add("х", "x");            
            //_iso.Add("ъ", "'");
            //_iso.Add("ь", "`");
                        
            //_iso.Add("Щ", "SHH");
            //_iso.Add("Ш", "SH");
            //_iso.Add("С", "S");

            //_iso.Add("Ч", "CH");
            //_iso.Add("Ц", "C");

            //_iso.Add("Ж", "ZH");
            //_iso.Add("З", "Z");

            //_iso.Add("Ё", "YO");
            //_iso.Add("Ю", "YU");
            //_iso.Add("Я", "YA");
            //_iso.Add("Э", "YE");
            //_iso.Add("Ы", "Y'");
            //_iso.Add("О", "O");
            //_iso.Add("У", "U");
            //_iso.Add("А", "A");
            //_iso.Add("Е", "E");

            //_iso.Add("Б", "B");
            //_iso.Add("В", "V");
            //_iso.Add("Г", "G");
            //_iso.Add("Д", "D");
            //_iso.Add("И", "I");
            //_iso.Add("Й", "J");
            //_iso.Add("К", "K");
            //_iso.Add("Л", "L");
            //_iso.Add("М", "M");
            //_iso.Add("Н", "N");
            //_iso.Add("П", "P");
            //_iso.Add("Р", "R");
            //_iso.Add("Т", "T");
            //_iso.Add("Ф", "F");
            //_iso.Add("Х", "X");
            //_iso.Add("Ъ", "'");
            //_iso.Add("Ь", "`");
        }
        

        //методы
        public static string RussianToTranslitUrl(string input)
        {
            if (input == null)
                return null;

            StringBuilder новаяСтрока = new StringBuilder(input.ToLower()); 

            foreach (KeyValuePair<string, string> буква in _iso)
                новаяСтрока = новаяСтрока.Replace(буква.Key, буква.Value);
            //новаяСтрока = новаяСтрока.Replace("ъ", "");
            //новаяСтрока = новаяСтрока.Replace("ь", "");

            string допустимыеСимволы = "0123456789abcdefghijklmnopqrstuvwxyz-";
            char минус = '-';
            for (int i = 0; i < новаяСтрока.Length; i++)
            {
                if (!допустимыеСимволы.Contains(новаяСтрока[i]))
                {
                    новаяСтрока = новаяСтрока.Remove(i, 1);
                    i--;
                }
                else if (новаяСтрока[i] == минус && i + 1 < новаяСтрока.Length && новаяСтрока[i + 1] == минус)
                {
                    новаяСтрока = новаяСтрока.Remove(i, 1);
                    i--;
                }
            }
            
            if (новаяСтрока.Length > 1000)
                новаяСтрока = новаяСтрока.Remove(1000, новаяСтрока.Length - 1000);
                 
            return новаяСтрока.ToString().Trim('-');
        }

        public static string TranslitUrlToRussian(string input)
        {
            if (input == null)
                return null;

            StringBuilder новаяСтрока = new StringBuilder(input.ToLower());

            foreach (KeyValuePair<string, string> буква in _iso)
                новаяСтрока = новаяСтрока.Replace(буква.Value, буква.Key);
            
            return новаяСтрока.ToString().Trim();
        }

        public static string RetainAllowedSymbols(string input, List<SymbolType> символы)
        {
            StringBuilder новаяСтрока = new StringBuilder(input);
            символы = символы.Distinct().ToList();

            bool символРазрешён;
            for (int i = новаяСтрока.Length - 1; i >= 0; i--)
            {
                символРазрешён = false;
                foreach (SymbolType тип in символы)
                {
                    switch (тип)
                    {
                        case SymbolType.Letters:
                            символРазрешён = char.IsLetter(новаяСтрока[i]);
                            break;
                        case SymbolType.WhiteSpace:
                            символРазрешён = char.IsWhiteSpace(новаяСтрока[i]);
                            break;
                        case SymbolType.SpecialSymbols:
                            символРазрешён = !char.IsLetterOrDigit(новаяСтрока[i]) &&
                                !char.IsWhiteSpace(новаяСтрока[i]);
                            break;
                        case SymbolType.Numbers:
                            символРазрешён = char.IsDigit(новаяСтрока[i]);
                            break;
                    }
                }

                if (!символРазрешён)
                    новаяСтрока = новаяСтрока.Remove(i, 1);
            }

            return новаяСтрока.ToString().Trim();            
        }

        public static string RetainAllowedSymbols(string input, SymbolType символы)
        {
            return RetainAllowedSymbols(input, new List<SymbolType>() { символы });
        }

        public static string DeleteProhibitedSymbols(string input, List<SymbolType> символы)
        {
            StringBuilder новаяСтрока = new StringBuilder(input);
            символы = символы.Distinct().ToList();

            bool символЗапрещён;
            for (int i = новаяСтрока.Length - 1; i >= 0; i--)
            {
                символЗапрещён = false;
                foreach (SymbolType тип in символы)
                {
                    switch (тип)
                    {
                        case SymbolType.Letters:
                            символЗапрещён = char.IsLetter(новаяСтрока[i]);
                            break;
                        case SymbolType.WhiteSpace:
                            символЗапрещён = char.IsWhiteSpace(новаяСтрока[i]);
                            break;
                        case SymbolType.SpecialSymbols:
                            символЗапрещён = !char.IsLetterOrDigit(новаяСтрока[i]) &&
                                !char.IsWhiteSpace(новаяСтрока[i]);
                            break;
                        case SymbolType.Numbers:
                            символЗапрещён = char.IsDigit(новаяСтрока[i]);
                            break;
                    }
                }

                if (символЗапрещён)
                    новаяСтрока = новаяСтрока.Remove(i, 1);
            }

            return новаяСтрока.ToString().Trim();
        }

        public static string DeleteProhibitedSymbols(string input, SymbolType символы)
        {
            return DeleteProhibitedSymbols(input, new List<SymbolType>() { символы });
        }
        
        

    }
}