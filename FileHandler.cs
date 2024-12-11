using GalapagosItemManager;
using System;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using Newtonsoft.Json;

public class FileHandler
{
    public static byte[] OpenRom()
    {
        //OpenFileDialogクラスのインスタンスを作成
        OpenFileDialog ofd = new OpenFileDialog();

        //[ファイルの種類]に表示される選択肢を指定する
        //指定しないとすべてのファイルが表示される
        ofd.Filter = "ROMファイル(*.gba)|*.gba";

        //[ファイルの種類]ではじめに選択されるものを指定する
        ofd.FilterIndex = 1;

        //タイトルを設定する
        ofd.Title = "開くファイルを選択してください";

        //ダイアログボックスを閉じる前に現在のディレクトリを復元するようにする
        ofd.RestoreDirectory = true;

        //存在しないファイルの名前が指定されたとき警告を表示する
        //デフォルトでTrueなので指定する必要はない
        //ofd.CheckFileExists = true;

        //存在しないパスが指定されたとき警告を表示する
        //デフォルトでTrueなので指定する必要はない
        //ofd.CheckPathExists = true;

        //オープンファイルダイアログを表示する
        DialogResult result = ofd.ShowDialog();

        // ユーザーが「キャンセル」をクリックした場合、直ちにnullを返す
        if (result != DialogResult.OK)
        {
            return null;
        }

        if (ofd.FileName != "") // 空じゃなければ読み込みへ
        {
            using (FileStream fs = new FileStream(
                ofd.FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))

            using (StreamReader sr = new StreamReader(fs))
            {

                try
                {

                    MainWindow.FileName = ofd.FileName;
                    MainWindow.FileSize = (int)fs.Length;

                    byte[] buf = new byte[MainWindow.FileSize];

                    int readSize;
                    int remain = MainWindow.FileSize;
                    int busPos = 0;

                    while (remain > 0)
                    {
                        // 1024byteずつ読み込む
                        readSize = fs.Read(buf, busPos, Math.Min(1024, remain));

                        busPos += readSize;
                        remain -= readSize;
                    }
                    return buf;
                }
                catch (Exception) { return null; }
            }
        }
        else { return null; }
    }
    public static void SaveRom(MainWindow main, byte[] ChangedRomAllData, TabType tabType)
    {
        if (ExistsRom(main))
        {
            if (main.RomNameField.Text != "")
            {
                try
                {
                    // 保存
                    System.IO.FileStream br = new System.IO.FileStream(
                    MainWindow.FileName,
                    System.IO.FileMode.Create,
                    System.IO.FileAccess.ReadWrite);
                    br.Write(ChangedRomAllData, 0, MainWindow.FileSize);
                    br.Close();
                    System.Windows.Forms.MessageBox.Show(tabType.ToString()+"を保存しました", "Successed!");
                }
                catch (Exception)
                {
                    System.Media.SystemSounds.Hand.Play();
                    System.Windows.Forms.MessageBox.Show("多分他のツールによって共有違反が発生しています\n思い当たるツールを閉じてみてください", "エラー");
                }

            }
            else
            {
                System.Media.SystemSounds.Hand.Play();
                System.Windows.Forms.MessageBox.Show("テキストボックスに何も書かれていません", "エラー");
            }
        }
    }

    public static void ReloadRom(MainWindow main)
    {
        if (ExistsRom(main))
        {

            if (MainWindow.FileName != "") // 空じゃなければ読み込みへ
            {
                 /*if (result == System.Windows.Forms.DialogResult.OK)
                {

                }
                 */
                FileStream fs = new FileStream(
                    MainWindow.FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                try // 例外処理
                {
                    StreamReader sr = new StreamReader(fs);

                    try
                    {

                        MainWindow.FileSize = (int)fs.Length;
                        byte[] buf = new byte[MainWindow.FileSize];

                        int readSize;
                        int remain = MainWindow.FileSize;
                        int busPos = 0;

                        while (remain > 0)
                        {
                            // 1024byteずつ読み込む
                            readSize = fs.Read(buf, busPos, Math.Min(1024, remain));

                            busPos += readSize;
                            remain -= readSize;
                        }
                    }
                    finally
                    {
                        if (sr != null)
                        {
                            sr.Dispose();
                        }
                    }
                }
                finally
                {
                    if (fs != null)
                    {
                        fs.Dispose();
                    }
                }
            }
        }
    }
    public static bool ExistsRom(MainWindow main)
    {

        if (main.RomNameField != null && main.RomNameField.Text != "ROMファイル")
        {
            return true;
        }
        else
        {
            System.Media.SystemSounds.Hand.Play();
            System.Windows.Forms.MessageBox.Show("ROMが読み込まれていません", "エラー");
            return false;
        }
    }
    public static Dictionary<string, string> LoadItemEffects()
    {
        string filePath = @"setting\ItemEffect.json";
        string jsonContent = File.ReadAllText(filePath);
        var itemEffects = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonContent);
        return itemEffects;
    }
}
