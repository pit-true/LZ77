using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace GalapagosItemManager
{
    class XmlDictionaryMapper
    {
        public static XElement MojiXml { get; set; }
        public static XElement SpecialXml { get; set; }
        public static Dictionary<string, string> MojiDictionary { get; set; }
        public static Dictionary<string, string> SpecialDictionary { get; set; }

        public static string GetID(String moji)
        {
            if (MainWindow.SpecialFlag)
            {
                /*
                specialFlagが立っている場合にspecial.xmlを読み込み
                []で囲まれた文字をF9 XXで始まるコードに変換する
                */
                return SpecialDictionary.FirstOrDefault(x => x.Value == moji).Key ?? "";
            }
            else if (MainWindow.KeepBinFlag)
            {
                // そのまま返す <01>を01とする
                return moji;
            }
            else
            {
                // 通常の処理
                return MojiDictionary.FirstOrDefault(x => x.Value == moji).Key ?? "";
            }
        }

        public static string GetMoji(string ID)
        {
            // F9 XXで始まる特殊記号はspecial.xmlを読み込む
            Dictionary<string, string> targetDictionary = MainWindow.SpecialFlag ? SpecialDictionary : MojiDictionary;
            return targetDictionary.TryGetValue(ID, out string value) ? value : "";
        }

        public static void LoadDictionaries()
        {
            MojiDictionary = LoadDictionaryFromXml(@"setting\moji.xml");
            SpecialDictionary = LoadDictionaryFromXml(@"setting\special.xml");
        }

        private static Dictionary<string, string> LoadDictionaryFromXml(string path)
        {
            var xelm = XElement.Load(path);
            var dictionary = new Dictionary<string, string>();

            foreach (var element in xelm.Elements("node"))
            {
                string key = element.Element("ID").Value;
                string value = element.Element("moji").Value;

                if (!dictionary.ContainsKey(key))
                {
                    dictionary.Add(key, value);
                }
            }

            return dictionary;
        }

        public static bool ExistsMoji(String ID)
        { 
            var xelm = XElement.Load(@"setting\moji.xml");

            var emp = (
                from p in xelm.Elements("node")
                where p.Element("ID").Value == ID
                select p).SingleOrDefault();

            try
            {
                return !emp.IsEmpty;
            }
            catch
            {
                return false;
            }

        }
    }
}