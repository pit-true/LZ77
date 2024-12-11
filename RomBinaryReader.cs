using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace GalapagosItemManager
{
    class RomBinaryReader
    {
        public static String GetRomName(byte[] RomAllData)
        {

            // 0xa8から表記されるROM名を取得
            int RomHeaderPos = Int32.Parse("a8", System.Globalization.NumberStyles.HexNumber);

            byte[] RomHeader = new byte[8];

            for (int i = 0; i < 8; i++)
            {
                RomHeader[i] = RomAllData[RomHeaderPos + i];

            }

            String RomName;

            switch (System.Text.Encoding.UTF8.GetString(RomHeader))
            {

                case "FIREBPRJ":
                    RomName = "ファイアレッド(J)";
                    break;

                case "LEAFBPGJ":
                    RomName = "リーフグリーン(J)";
                    break;

                case "RUBYAXVJ":
                    RomName = "ルビー(J)";
                    break;

                case "SAPPAXPJ":
                    RomName = "サファイア(J)";
                    break;

                case "EMERBPEJ":
                    RomName = "エメラルド(J)";
                    break;

                default:
                    RomName = "?????";
                    break;
            }

            return RomName;

        }
        public static String[] GetItemData(MainWindow main, byte[] RomAllData, int ItemIndex)
        {
            // 特定のアイテムのデータを取得する

            if (FileHandler.ExistsRom(main))
            {

                // 0x1c8からアイテムデータテーブルの始点が記述される
                //  -0x8000000で拡張領域にも対応済　デフォルトは0x3a06f8

                int ItemDataTablePointer = Int32.Parse("1c8", System.Globalization.NumberStyles.HexNumber);
                int Index = BitConverter.ToInt32(RomAllData, ItemDataTablePointer) - 0x8000000;
                Index += ItemIndex * 40;
                // 16進数
                //return ItemDataTablePos.ToString("X8").ToLower();

                String[] ItemData = new string[16];

                String ItemName;
                String ItemNameBin = "";
                String ItemNumber;
                int ItemAmount;
                int ItemEffectID;
                int AllowItemPossession;
                int CaptionPointer;
                int ItemEffectValue;
                String Caption="";
                int Unknown;
                int Pocket;
                int FieldType;
                int FieldPointer;
                int FieldAddress;
                int BattleType;
                int BattlePointer;
                int BattleAddress;
                int SpecialID;

                // FRのアイテム画像テーブル始点の参照先
                int PicTablePointer = Int32.Parse("981fc", System.Globalization.NumberStyles.HexNumber);
                int PicTableAddress = BitConverter.ToInt32(RomAllData, PicTablePointer) - 0x8000000;
                //throw new Exception("PicTableAddress:{PicTableAddress}");
                int PicIndex = PicTableAddress + ItemIndex * 8;
                int PicAddress = BitConverter.ToInt32(RomAllData, PicIndex) - 0x8000000;
                int PaletteAddress = BitConverter.ToInt32(RomAllData, PicIndex + 4) - 0x8000000;
                int i = 0;
                while (RomAllData[Index + i] != 0xff)
                {
                    ItemNameBin += RomAllData[Index + i].ToString("x2") + " ";
                    i++;
                }
                ItemNameBin += "ff";
                
                ItemName = FormatConverter.BinaryTextToText(ItemNameBin);

                ItemNumber = $"{RomAllData[Index + 11]:x2}{RomAllData[Index + 10]:x2}";

                byte lowByte = RomAllData[Index + 12];
                byte highByte = RomAllData[Index + 13];
                ItemAmount = (highByte << 8) | lowByte;

                ItemEffectID = RomAllData[Index + 14];
                ItemEffectValue = RomAllData[Index + 15];

                CaptionPointer = Index + 16;
                // -0x8000000で拡張領域にも対応済
                int CaptionIndex = BitConverter.ToInt32(RomAllData, CaptionPointer) - 0x8000000;

                String CaptionBin="";
                i = 0;

                while (RomAllData[CaptionIndex + i] != 0xff)
                {
                    CaptionBin += RomAllData[CaptionIndex + i].ToString("x2") +" ";
                    i++;
                }

                Caption = FormatConverter.BinaryTextToText(CaptionBin + "ff");

                AllowItemPossession = RomAllData[Index + 20];
                Unknown = RomAllData[Index + 21];
                Pocket = RomAllData[Index + 22];
                FieldType= RomAllData[Index + 23];

                FieldPointer = Index + 24;
                FieldAddress = BitConverter.ToInt32(RomAllData, FieldPointer) & 0x00FFFFFF;

                BattleType = RomAllData[Index + 28];

                BattlePointer = Index + 32;
                BattleAddress = BitConverter.ToInt32(RomAllData, BattlePointer) & 0x00FFFFFF;

                SpecialID = RomAllData[Index + 36];

                ItemData[0] = ItemName;
                ItemData[1] = ItemNumber;
                ItemData[2] = ItemAmount.ToString();
                ItemData[3] = ItemEffectID.ToString();
                ItemData[4] = ItemEffectValue.ToString();
                ItemData[5] = Caption;
                ItemData[6] = AllowItemPossession.ToString();
                ItemData[7] = Unknown.ToString();
                ItemData[8] = Pocket.ToString();
                ItemData[9] = FieldType.ToString();
                ItemData[10] = FieldAddress.ToString("x6");

                if (RomAllData[Index + 27] == 0x09)
                {
                    ItemData[10] = "09" + FieldAddress.ToString("x6");
                }

                ItemData[11] = BattleType.ToString();
                ItemData[12] = BattleAddress.ToString("x6");

                if (RomAllData[Index + 35] == 0x09)
                {
                    ItemData[10] = "09" + BattleAddress.ToString("x6");
                }

                ItemData[13] = SpecialID.ToString();
                ItemData[14] = PicAddress.ToString("x6");
                ItemData[15] = PaletteAddress.ToString("x6");

                return ItemData;
            }
            else
            {
                String[] NoItemData = new string[] { "", "", "", "", "", "", "", "", "", "", "", "", "", "" };
                return NoItemData;
            }
        }

        public static (byte[],byte[]) GetIconData(byte[] RomAllData, int ItemIndex)
        {
            // FRのアイテム画像テーブル始点の参照先
            int PicTablePointer = Int32.Parse("981fc", System.Globalization.NumberStyles.HexNumber);
            int PicTableAddress = BitConverter.ToInt32(RomAllData, PicTablePointer) - 0x8000000;
            
            int PicIndex = PicTableAddress + ItemIndex * 8;
            int PaletteIndex = PicIndex + 4;

            int PicAddress = BitConverter.ToInt32(RomAllData, PicIndex) - 0x8000000;
            int PaletteAddress = BitConverter.ToInt32(RomAllData, PaletteIndex) - 0x8000000;

            byte[] PicBytes = LZ77Handler.GetDecompressedData(RomAllData, PicAddress);
            byte[] PaletteBytes = LZ77Handler.GetDecompressedData(RomAllData, PaletteAddress);
            
            return (PicBytes, PaletteBytes);

        }
        public static String[] GetItemNamesList(byte[] RomAllData)
        {
            // 0x1c8から表記されるアイテムデータテーブルの始点を取得　デフォルトは0x3a06f8
            int ItemDataTablePointer = Int32.Parse("1c8", System.Globalization.NumberStyles.HexNumber);
            
            // 拡張領域にも対応済
            int Index = BitConverter.ToInt32(RomAllData, ItemDataTablePointer) - 0x8000000;
            
            // 0x9a2b2の個数を取得 半分になっているので2倍にする
            int TotalItemCount = RomAllData[Int32.Parse("9a2b2", System.Globalization.NumberStyles.HexNumber)] * 2;

            String[] ItemNames = new string[TotalItemCount + 1];

            for (int i = 0; i <= TotalItemCount; i++)
            {
                int j = 0;
                StringBuilder ItemNameBin = new StringBuilder();

                while (RomAllData[Index + j] != 0xff)
                {
                    string hexValue = RomAllData[Index + j].ToString("x2");
                    ItemNameBin.Append(hexValue+" ");
                    j++;
                }

                ItemNames[i] = FormatConverter.BinaryTextToText(ItemNameBin.ToString());
                Index += 40;
            }
            return ItemNames;
        }

        public static int GetItemDataOffset(byte[] RomAllData, int ItemIndex)
        {
            int ItemDataTablePointer = Int32.Parse("1c8", System.Globalization.NumberStyles.HexNumber);
            int Index = BitConverter.ToInt32(RomAllData, ItemDataTablePointer) - 0x8000000;
            Index += ItemIndex * 40;
            return Index;
        }

        public static int GetItemTextOffset(byte[] RomAllData, int ItemIndex)
        {
            int CaptionPointer = GetItemDataOffset(RomAllData, ItemIndex) + 16;
            int CaptionIndex = BitConverter.ToInt32(RomAllData, CaptionPointer) - 0x8000000;
            return CaptionIndex;
        }

        public static byte[] GetItemCaptionBytes(byte[] RomAllData, int CaptionOffset)
        {
            List<byte> captionList = new List<byte>();

            int i = 0;
            while (RomAllData[CaptionOffset + i] != 0xff)
            {
                captionList.Add(RomAllData[CaptionOffset + i]);
                i++;
            }

            return captionList.ToArray();
        }
    }
}

