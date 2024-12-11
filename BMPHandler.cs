using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace GalapagosItemManager
{
    internal class BMPHandler
    {
        public static byte[] GetFullBmp(byte[] picBytes, byte[] paletteBytes)
        {
            byte[] fullBmp = new byte[406]; // アイテム画像(bmp)は406byteで固定

            // BMP画像のヘッダー
            byte[] header = { 0x42, 0x4d,               // 42 4D(2byte) どのBMPでも固定
                              0x96, 0x01, 0x00, 0x00,   // ファイルサイズ(4byte) アイテム画像は0x196(406)byte
                              0x00, 0x00, 0x00, 0x00,   // 予約領域(4byte)
                              0x76, 0x00, 0x00, 0x00,   // 画像データの開始番地(4byte) 0x76で固定
                              0x28, 0x00, 0x00, 0x00,   // ヘッダーサイズ(4byte) 0x28(40)byteで固定
                              0x18, 0x00, 0x00, 0x00,   // Width(px)(4byte) アイテム画像は0x18byte(24px)
                              0x18, 0x00, 0x00, 0x00,   // Height(px)(4byte) アイテム画像は0x18byte(24px)
                              0x01, 0x00,               // 面の数(2byte) 0x1で固定
                              0x04, 0x00,               // 1ピクセル当たりのビット数(使用可能な色数)(2byte) 0x4(4bit=16色)
                              0x00, 0x00, 0x00, 0x00,   // 圧縮形式(4byte) 圧縮無しなので0
                              0xc4, 0x0e, 0x00, 0x00,   // 画像のデータサイズ(4byte) 0xec4(3780)= 96dpi
                              0xc4, 0x0e, 0x00, 0x00,   // 横の解像度(4byte) 0xec4(3780)= 96dpi
                              0xc4, 0x0e, 0x00, 0x00,   // 横の解像度(4byte) 0xec4(3780)= 96dpi
                              0x10, 0x00, 0x00, 0x00,   // パレットテーブル(4byte) 0x10 = 16色 しかし0x0になってるのも見る
                              0x10, 0x00, 0x00, 0x00,   // パレットインデックス(4byte) 0x10 = 16色 しかし0x0になってるのも見る
                            };

            byte[] colorPalette = new byte[64]; // 16色 * 4 = 64byte

            // B, G, R, 予約領域(0)の順
            for (int i = 0; i < 16; i++)
            {
                // paletteBytes は 2 バイトずつ格納されているので、2 バイトずつ処理
                byte[] color = ExtractRGB(new byte[] { paletteBytes[i * 2], paletteBytes[i * 2 + 1] });

                colorPalette[i * 4] = color[0];         // Blue
                colorPalette[i * 4 + 1] = color[1];     // Green
                colorPalette[i * 4 + 2] = color[2];     // Red
                colorPalette[i * 4 + 3] = 0x0;          // 予約領域 (固定で 0x0 を設定)
            }

            byte[] reverse = new byte[288];

            for (int i = 0; i < 288; i++)
            {
                byte originalByte = picBytes[i];
                byte upperNibble = (byte)((originalByte & 0xF0) >> 4); // 上位4ビットを取得して右にシフト
                byte lowerNibble = (byte)((originalByte & 0x0F) << 4); // 下位4ビットを取得して左にシフト
                reverse[i] = (byte)(upperNibble | lowerNibble);       // 上位と下位を入れ替えて結合
            }

            byte[] pic = new byte[288];

            int srcCnt = 220;
            int bmpCnt = 0;

            for (int k = 0; k < 3; k++)
            {
                for (int j = 0; j < 3; j++)
                {
                    int srcIndex = srcCnt + j * 32;
                    int bmpIndex = bmpCnt + j * 4;

                    for (int i = 0; i < 8; i++)
                    {
                        Array.Copy(reverse, srcIndex, pic, bmpIndex, 4);
                        srcIndex -= 4;
                        bmpIndex += 12;
                    }
                }
                srcCnt -= 96;
                bmpCnt += 96;
            }

            Buffer.BlockCopy(header, 0, fullBmp, 0, header.Length);
            Buffer.BlockCopy(colorPalette, 0, fullBmp, header.Length, colorPalette.Length);
            Buffer.BlockCopy(pic, 0, fullBmp, header.Length + colorPalette.Length, pic.Length);

            return fullBmp;

        }
        public static byte[] ExtractRGB(byte[] data)
        {
            // 2バイトのデータを15ビットの整数として取得（15ビット目は無視）
            int value = ((data[1] & 0x7F) << 8) | data[0];

            // 各色の値を5ビットから8ビットに変換
            byte Red = (byte)((value & 0x1F) << 3);
            byte Green = (byte)(((value >> 5) & 0x1F) << 3);
            byte Blue = (byte)(((value >> 10) & 0x1F) << 3);

            // RGBの値を配列に格納して返す
            return new byte[] { Blue, Green, Red };
        }

        public static BitmapImage ByteArrayToBitmapImage(byte[] imageData)
        {
            using (MemoryStream ms = new MemoryStream(imageData))
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = ms;
                bitmap.CacheOption = BitmapCacheOption.OnLoad; // ストリームを閉じた後もデータを保持
                bitmap.EndInit();
                return bitmap;
            }
        }
    }
}
