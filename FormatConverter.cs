using System;
using System.Text;
using System.Text.RegularExpressions;

namespace GalapagosItemManager
{

    public static class FormatConverter
    {
        static String tempString;

        public static String NormalizeText(String inputText)
        {   //TextToBinary()に投げる用の関数

            String normalizedText = inputText;

            normalizedText = normalizedText.Replace("\r\n", "");
            normalizedText = normalizedText.Replace("¥", @"\");
            normalizedText = Regex.Replace(normalizedText, "[０-９]", m => ((char)(m.Value[0] - '０' + '0')).ToString());
            normalizedText = normalizedText.Replace(" ", "　");

            return normalizedText;
        }

        public static String TextToBinaryText(String inputText)
        {
            // 入力されたテキストをバイナリ(String型)にして返す関数

            String binaryText = ""; // 返り値
            String moji = "";       // XML内で辞書検索するためのもの
            String targetText = "";  // 1文字ずつ組み立ててmojiに代入する用変数

            // 成形してから投げる
            String processedText = NormalizeText(inputText);


            // 文字変換処理

            for (int i = 0; i < processedText.Length; i++)
            {

                // []で囲まれた文字を特殊記号(F9 XX)に変換する
                if (processedText[i] == '[')
                {
                    MainWindow.SpecialFlag = true;
                }
                else if (processedText[i] == ']')
                {
                    MainWindow.SpecialFlag = false;
                }
                // <>で囲まれた1byte文字(01など)をそのまま出力する(int型整数をtoStringするようなもの)
                else if (processedText[i] == '<')
                {
                    MainWindow.KeepBinFlag = true;
                }
                else if (processedText[i] == '>')
                {
                    MainWindow.KeepBinFlag = false;
                }

                // keepBinFlagがtrueなら例えば0と1を01のように纏める
                else if (MainWindow.KeepBinFlag)
                {
                    moji = processedText[i].ToString() + processedText[i + 1].ToString();
                    i++;
                    tempString = XmlDictionaryMapper.GetID(moji);
                    binaryText += tempString + " ";
                }
                // \n, \p, \mを纏めたあと変換する
                else if (processedText[i] == '\\' && (processedText[i + 1] == 'n' || processedText[i + 1] == 'p' || processedText[i + 1] == 'm'))
                {
                    moji = processedText[i].ToString() + processedText[i + 1].ToString();
                    i++;
                    tempString = XmlDictionaryMapper.GetID(moji);
                    binaryText += tempString + " ";
                }
                // {Lv}, {PP}, {ID.}, {No}などを纏めた後変換する
                else if (processedText[i] == '{')
                {
                    for (; processedText[i] != '}'; i++)
                    {
                        targetText += processedText[i].ToString();
                    }
                    targetText += '}';
                    moji = targetText;
                    tempString = XmlDictionaryMapper.GetID(moji);
                    binaryText += tempString + " ";
                }
                // 何もなければ普通に変換
                else
                {
                    moji = processedText[i].ToString();
                    tempString = XmlDictionaryMapper.GetID(moji);
                    binaryText += tempString + " ";
                }
            }

            binaryText = binaryText.Replace("  ", " ");
            binaryText = binaryText.TrimEnd();

            return binaryText;
        }

        public static string BinaryTextToText(string inputBinaryText)
        {
            string changedText = "";
            string hex, secondHex, thirdHex = "", fourthHex = "", fifthHex = "";
            string binaryText = inputBinaryText.Replace("  ", " ").ToLower();
            binaryText = Regex.Replace(binaryText, @"[^0-9a-f ]+", "");
            bool InsertSpecialBracketFlag = false;

            for (int i = 0; i < binaryText.Length; i++)
            {
                if (binaryText[i] == ' ') continue;

                hex = binaryText[i].ToString() + binaryText[i + 1].ToString();

                bool loadTextScriptParamFlag = false;
                bool load2byteParamFlag = false;
                bool load3byteParamFlag = false;
                string tempString = "";

                if (hex == "fc" && i + 3 <= binaryText.Length)
                {
                    secondHex = binaryText[i + 3].ToString() + binaryText[i + 4].ToString();

                    if (i + 6 <= binaryText.Length)
                    {
                        thirdHex = binaryText[i + 6].ToString() + binaryText[i + 7].ToString();

                        if (XmlDictionaryMapper.ExistsMoji(hex + " " + secondHex + " " + thirdHex))
                        {
                            hex = hex + " " + secondHex + " " + thirdHex;
                            i += 6;
                        }
                        else
                        {
                            hex = hex + " " + secondHex;
                            i += 3;

                            if (secondHex == "04")
                            {
                                thirdHex = binaryText[i + 6].ToString() + binaryText[i + 7].ToString();
                                fourthHex = binaryText[i + 9].ToString() + binaryText[i + 10].ToString();
                                fifthHex = binaryText[i + 12].ToString() + binaryText[i + 13].ToString();
                                i += 9;
                                load3byteParamFlag = true;
                            }
                            else if (secondHex == "0b" || secondHex == "10")
                            {
                                thirdHex = binaryText[i + 6].ToString() + binaryText[i + 7].ToString();
                                fourthHex = binaryText[i + 9].ToString() + binaryText[i + 10].ToString();
                                i += 3;
                                load2byteParamFlag = true;
                            }
                            else if (secondHex == "07" || secondHex == "09" || secondHex == "0a" || secondHex == "0f" || secondHex == "17" || secondHex == "18")
                            {
                                i += 3;
                            }
                            else
                            {
                                i += 6;
                                loadTextScriptParamFlag = true;
                            }
                        }
                    }
                    else
                    {
                        hex = hex + " " + secondHex;
                        i += 3;
                    }
                }
                else if (hex == "fd" && i + 3 <= binaryText.Length)
                {
                    secondHex = binaryText[i + 3].ToString() + binaryText[i + 4].ToString();
                    hex = hex + " " + secondHex;
                    i += 3;
                }
                else if (hex == "f9" && i + 3 <= binaryText.Length)
                {
                    secondHex = binaryText[i + 3].ToString() + binaryText[i + 4].ToString();
                    int.TryParse(secondHex, System.Globalization.NumberStyles.HexNumber, null, out int secondHexValue);
                    if (!(secondHexValue >= 0 && secondHexValue <= 0x16))
                    {
                        MainWindow.SpecialFlag = true;
                        hex = hex + " " + secondHex;
                        i += 3;

                        if (!InsertSpecialBracketFlag)
                        {
                            changedText += '[';
                            InsertSpecialBracketFlag = true;
                        }
                    }

                    if (!MainWindow.SpecialFlag)
                    {
                        hex = hex + " " + secondHex;
                        i += 3;
                    }
                }

                if (!loadTextScriptParamFlag)
                {
                    tempString = XmlDictionaryMapper.GetMoji(hex);
                }
                else
                {
                    tempString = XmlDictionaryMapper.GetMoji(hex) + '<' + thirdHex + '>';

                    if (load2byteParamFlag)
                    {
                        tempString += '<' + fourthHex + '>';
                        load2byteParamFlag = false;
                    }
                    else if (load3byteParamFlag)
                    {
                        tempString += '<' + fourthHex + '>' + '<' + fifthHex + '>';
                        load3byteParamFlag = false;
                    }
                    loadTextScriptParamFlag = false;
                }

                changedText += tempString;

                if (MainWindow.SpecialFlag && (i > binaryText.LastIndexOf("f9")))
                {
                    changedText += ']';
                    InsertSpecialBracketFlag = false;
                    MainWindow.SpecialFlag = false;
                }

                i += 2;
            }

            changedText = changedText.Replace("\\n", "\\n\n");
            changedText = changedText.Replace("\\m", "\\m\n");
            changedText = changedText.Replace("\\p", "\\p\n");

            return changedText;
        }
        public static int judgeOffset(String o)
        {
            String offset = o;
            bool oFlag = false;

            if (offset.Length == 8)
            {

                if ((o[0] == '0' && o[1] == 'x') || (o[0] == '0' && o[1] == '0') || (o[0] == '0' && o[1] == '8'))
                {   // 先頭が0xか00か08ならOK
                    offset = "";
                    // 最初の0xなどを削ってoffsetへ代入
                    for (int i = 0; i < o.Length - 2; i++)
                    {
                        offset += o[i + 2];
                    }
                }
                else if (o[0] == '0' && o[1] == '9')
                {   // 先頭が09なら01@@@@@@にする必要がある
                    offset = "";
                    offset += o[0];
                    offset += '1';
                    // offsetへ代入
                    for (int i = 2; i < o.Length; i++)
                    {
                        offset += o[i];
                    }
                }
                else if (o[0] == '0' && o[1] == '1')
                {
                    offset = "";
                    // 拡張領域01@@@@@@なのでそのままoffsetへ代入
                    for (int i = 0; i < o.Length; i++)
                    {
                        offset += o[i];
                    }
                }
            }
            else if (offset.Length == 10)
            {
                if (o[0] == '0' && o[1] == 'x' && ((o[2] == '0' && o[3] == '0') || (o[2] == '0' && o[3] == '8')))
                {   // 0x00@@@@@@, 0x08@@@@@@ならOK
                    offset = "";
                    // 最初の0x08などを削ってoffsetへ代入
                    for (int i = 0; i < o.Length - 4; i++)
                    {
                        offset += o[i + 4];
                    }
                }
                else if (o[0] == '0' && o[1] == 'x' && o[2] == '0' && o[3] == '1')
                {   // こっちは拡張領域、先頭が0xで次が01ならOK
                    offset = "";
                    // 最初の0xを削ってoffsetへ代入
                    for (int i = 0; i < o.Length - 2; i++)
                    {
                        offset += o[i + 2];
                    }
                }
            }
            else
            {
                // ******より短くても問題ないので続行
            }
            // A~Fまでをa~fに置換する
            offset = offset.ToLower();
            offset = offset.Replace("0x", "");

            foreach (char c in offset)
            {
                if (c == '0' || c == '1' || c == '2' || c == '3')
                {
                }
                else if (c == '4' || c == '5' || c == '6' || c == '7')
                {
                }
                else if (c == '8' || c == '9' || c == 'a' || c == 'b')
                {
                }
                else if (c == 'c' || c == 'd' || c == 'e' || c == 'f')
                {
                }
                else
                {
                    oFlag = true;
                }
            }

            if (oFlag && offset.Length <= 8)
            {
                System.Media.SystemSounds.Hand.Play();
                System.Windows.MessageBox.Show("アドレスに使えない値が含まれています", "エラー");
            }
            

            // offsetを10進数に直す
            try
            {
                if (offset.Length > 8)
                {
                    System.Media.SystemSounds.Hand.Play();
                    System.Windows.MessageBox.Show("オフセットが長すぎます\n    ****** 0x******\n00****** 08******\n拡張領域なら\n01****** 0x01******\n09****** 0x09******の\nいずれかの形で入力してください", "エラー");

                    return -1;
                }
                else {
                    return Int32.Parse(offset, System.Globalization.NumberStyles.HexNumber);
                }
            }
            // 例外処理 オフセット以外の平仮名や漢字が書かれていた場合は-1を返す
            catch (Exception)
            {
                return -1;
            }
        }

        public static byte[] HexStringToByteArray(string hexValues)
        {
            // 全角スペースとを半角スペースに、A~Fまでをa~fに置換する
            // 文字に空白や16進数以外が含まれていたら削除する
            hexValues = Regex.Replace(hexValues.ToLower(), "[^0-9a-f ]", "");
            string[] hexValuesSplit = hexValues.Split(' ');

            byte[] byteArray = new byte[hexValuesSplit.Length];

            for (int i = 0; i < hexValuesSplit.Length; i++)
            {
                byteArray[i] = Convert.ToByte(hexValuesSplit[i], 16);
            }

            return byteArray;
        }
        public static byte[] ConvertToLittleEndianByteArray(string inputValue)
        {
            // 入力値の桁数を確認します。
            if (inputValue.Length == 7)
            {
                if (inputValue.StartsWith("8") || inputValue.StartsWith("9"))
                {
                    // 先頭に '0' を追加します。
                    inputValue = "0" + inputValue;
                }
            }

            // 入力値を整数として解析します。
            int number = int.Parse(inputValue, System.Globalization.NumberStyles.HexNumber);

            if (inputValue.Length == 8)
            {
                // 0x08000000を足す条件を確認します。
                if (!inputValue.StartsWith("00") && !inputValue.StartsWith("01"))
                {
                    number += 134217728;
                }
            }

            if (inputValue.Length == 6)
            {
                number += 134217728;
            }

            // バイト配列に整数をリトルエンディアン形式で挿入します。
            byte[] result = new byte[4];
            result[0] = (byte)(number & 0xFF);
            result[1] = (byte)((number >> 8) & 0xFF);
            result[2] = (byte)((number >> 16) & 0xFF);
            result[3] = (byte)((number >> 24) & 0xFF);

            return result;
        }

        public static int SearchBytes(this byte[] text, byte[] pattern, int foundIndex)
        {
            int patternLen = pattern.Length, textLen = text.Length;

            // 移動量テーブルの作成
            int[] qs_table = new int[byte.MaxValue + 1];

            // デフォルト（パターン中に存在しないキャラクタが比較範囲の直後にあった）の場合、
            // 次の比較範囲はそのキャラクタの次。（＝比較範囲ずらし幅はパターン長＋１）
            for (int i = qs_table.Length; i-- > 0;) qs_table[i] = patternLen + 1;

            // パターンに存在するキャラクタが比較範囲の直後にあった場合、
            // 次の比較範囲は、そのキャラクタとパターン中のキャラクタを一致させる位置に。
            for (int n = 0; n < patternLen; ++n) qs_table[pattern[n]] = patternLen - n;

            int pos;

            // 移動量テーブルを用いて、文章の末尾に達しない範囲で比較を繰り返す
            for (pos = foundIndex; pos < textLen - patternLen; pos += qs_table[text[pos + patternLen]])
            {
                // 一致するか比較。一致したら、そのときの比較位置を返す。
                if (CompareBytes(text, pos, pattern, patternLen)) return pos;
            }

            // 文章の末尾がまだ未比較なら、そこも比較しておく
            if (pos == textLen - patternLen)
            {
                // 一致するか比較。一致したら、そのときの比較位置を返す。
                if (CompareBytes(text, pos, pattern, patternLen)) return pos;
            }

            // 一致する位置はなかった。
            return -1;
        }

        static bool CompareBytes(byte[] text, int pos, byte[] pattern, int patternLen)
        {
            for (int comparer = 0; comparer < patternLen; ++comparer)
            {
                if (text[comparer + pos] != pattern[comparer]) return false;
            }
            return true;
        }
    }
}

