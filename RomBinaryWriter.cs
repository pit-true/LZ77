using System;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace GalapagosItemManager
{
    internal class RomBinaryWriter
    {

        public static byte[] WriteItemInfo(byte[] RomAllData, int ItemIndex, String[] ItemData)
        {
            byte[] ChangedRomAllData = RomAllData;
            int Index = RomBinaryReader.GetItemDataOffset(RomAllData, ItemIndex);

            byte[] ItemName = FormatConverter.HexStringToByteArray(FormatConverter.TextToBinaryText(ItemData[0]));
            int i = 0;

            while (ItemName[i] != 0xff) {
                ChangedRomAllData[Index + i] = ItemName[i];
                i++;
            }
            ChangedRomAllData[Index + i] = 0xff;

            if(RomAllData[Index + 10] == 0x00 && RomAllData[Index + 11] == 0x00 && ItemIndex != 0) {
                //アイテム：？？？？？？？？はアイテムコードが0000になっているので、自動で直す
                byte[] ItemCode = BitConverter.GetBytes(ItemIndex);
                ChangedRomAllData[Index + 10] = ItemCode[0];
                ChangedRomAllData[Index + 11] = ItemCode[1];
            }
            
            byte[] Amount = BitConverter.GetBytes(int.Parse(ItemData[2]));
            ChangedRomAllData[Index + 12] = Amount[0];
            ChangedRomAllData[Index + 13] = Amount[1];

            // 所持効果ID
            ChangedRomAllData[Index + 14] = (byte)int.Parse(ItemData[3]);
            // 効果値
            ChangedRomAllData[Index + 15] = (byte)int.Parse(ItemData[4]);

            // ItemData[5]はテキストの参照アドレスではなくテキストそのものなので先にテキストアドレスを取得する
            int CaptionOffset = RomBinaryReader.GetItemTextOffset(RomAllData, ItemIndex);
            byte[] OldCaptionBytes = RomBinaryReader.GetItemCaptionBytes(RomAllData, CaptionOffset);

            String endChar = FormatConverter.BinaryTextToText("ff");

            if (!ItemData[5].EndsWith(endChar))
            {
                ItemData[5] += endChar;
            }

            byte[] NewItemCaptionBytes = FormatConverter.HexStringToByteArray(FormatConverter.TextToBinaryText(ItemData[5]));

            if (OldCaptionBytes.Length >= NewItemCaptionBytes.Length)
            {
                // サイズが少ないので参照先を変更しなくてよい
                i = 0;
                while (NewItemCaptionBytes[i] != 0xff) {
                    ChangedRomAllData[CaptionOffset + i] = NewItemCaptionBytes[i];
                    i++;
                }
                ChangedRomAllData[CaptionOffset + i] = 0xff;
            }
            else
            {
                int SearchNum = NewItemCaptionBytes.Length - OldCaptionBytes.Length;
                i = OldCaptionBytes.Length + 1;

                for (int j = 0; ChangedRomAllData[CaptionOffset + i] == 0xff && j < SearchNum; j++) {
                    i++;
                }

                if (i >= NewItemCaptionBytes.Length)
                {
                    // 後ろが空き容量なので参照先を変更しなくてよい
                    i = 0;
                    while (NewItemCaptionBytes[i] != 0xff)
                    {
                        ChangedRomAllData[CaptionOffset + i] = NewItemCaptionBytes[i];
                        i++;
                    }
                    ChangedRomAllData[CaptionOffset + i] = 0xff;
                }
                else
                {
                    // 参照先を変える処理を書く
                    System.Windows.Forms.MessageBox.Show("多いね");
                    byte[] CaptionBytes = BitConverter.GetBytes(CaptionOffset);
                }
            }

            // 所持可否
            ChangedRomAllData[Index + 20] = (byte)int.Parse(ItemData[6]);

            // 不明値
            ChangedRomAllData[Index + 21] = byte.Parse(ItemData[7], System.Globalization.NumberStyles.HexNumber);

            // ポケット
            ChangedRomAllData[Index + 22] = (byte)int.Parse(ItemData[8]);

            // フィールド使用タイプ
            ChangedRomAllData[Index + 23] = (byte)int.Parse(ItemData[9]);

            // フィールド使用アドレス
            byte[] FieldTypeBytes = FormatConverter.ConvertToLittleEndianByteArray(ItemData[10]);
            ChangedRomAllData[Index + 24] = FieldTypeBytes[0];
            ChangedRomAllData[Index + 25] = FieldTypeBytes[1];
            ChangedRomAllData[Index + 26] = FieldTypeBytes[2];

            if (FieldTypeBytes[3] == 0x11)
            {
                ChangedRomAllData[Index + 27] = 0x09;
            }
            else if(FieldTypeBytes[3] == 0x1)
            {
                ChangedRomAllData[Index + 27] = 0x09;
            }
            else if (FieldTypeBytes[3] == 0x0)
            {
                ChangedRomAllData[Index + 27] = 0x08;
            }
            else
            {
                ChangedRomAllData[Index + 27] = FieldTypeBytes[3];
            }

            // バトル使用タイプ
            ChangedRomAllData[Index + 28] = (byte)int.Parse(ItemData[11]);

            // バトル使用アドレス
            byte[] BattleTypeBytes = FormatConverter.ConvertToLittleEndianByteArray(ItemData[12]);
            ChangedRomAllData[Index + 29] = BattleTypeBytes[0];
            ChangedRomAllData[Index + 30] = BattleTypeBytes[1];
            ChangedRomAllData[Index + 31] = BattleTypeBytes[2];

            //もしチェックボックスにチェックが入っていれば、09を代入
            //if()
            //ChangedRomAllData[Index + 32] = 0x09;

            // 特殊ID
            ChangedRomAllData[Index + 36] = byte.Parse(ItemData[13], System.Globalization.NumberStyles.HexNumber);

            return ChangedRomAllData;
        }

        public static byte[] WritePcItemInfo(byte[] RomAllData)
        {
            byte[] ChangedRomAllData = RomAllData;
            // _romAllDataを使ってPCアイテム情報を書き込む処理をここに書く
            return ChangedRomAllData;
        }

    }
}
