using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace GalapagosItemManager
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        internal static bool SpecialFlag = false;   // F9 XXのxmlファイルを読むか否かのフラグ
        internal static bool KeepBinFlag = false;   // <>で囲まれているか否かのフラグ
        private static byte[] RomAllData;          // RomAllData[0]~RomAllData[fileSize-1]までバイナリが入っている
        internal static String FileName;            // ファイル名 書き込みや再読込で必要
        internal static int FileSize;               // ファイルサイズ 書き込みで必要
        internal static string[] ItemNames;
        public ObservableCollection<string> Items { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> ItemEffects { get; set; } = new ObservableCollection<string>();
        public ViewModel ItemManagerViewModel { get; private set; }
        private ViewModel ViewModel { get; set; }
        public int ItemEffectsListHeight { get; set; }

        //現在のタブの種類を保持
        private TabType currentTabType;

        public class MyCommands
        {

            // Ctrl + OでROMの読み込み
            public static RoutedUICommand OpenRom = new RoutedUICommand("OpenRom", "OpenRom", typeof(MyCommands), new InputGestureCollection { new KeyGesture(Key.O, ModifierKeys.Control) });

            // Ctrl + SでROMの保存
            public static RoutedUICommand SaveRom = new RoutedUICommand("SaveRom", "SaveRom", typeof(MyCommands), new InputGestureCollection { new KeyGesture(Key.S, ModifierKeys.Control) });

            // Ctrl + RでROMの再読み込み
            public static RoutedUICommand ReloadRom = new RoutedUICommand("ReloadRom", "ReloadRom", typeof(MyCommands), new InputGestureCollection { new KeyGesture(Key.R, ModifierKeys.Control) });

        }

        public MainWindow()
        {
            // ショートカット設定
            this.CommandBindings.Add(new CommandBinding(MyCommands.OpenRom, delegate { AddKey('O'); }));
            this.CommandBindings.Add(new CommandBinding(MyCommands.SaveRom, delegate { AddKey('S'); }));
            this.CommandBindings.Add(new CommandBinding(MyCommands.ReloadRom, delegate { AddKey('R'); }));

            InitializeComponent();
            XmlDictionaryMapper.LoadDictionaries();
            ViewModel = new ViewModel();
            DataContext = ViewModel;

        }

        // MainWindowを呼び出すためにおく
        public MainWindow MainWindowPointer;

        public void AddKey(char key)
        {

            switch (key)
            {
                case 'O':
                    OpenRom_Click(null, new RoutedEventArgs());
                    break;

                case 'S':
                    SaveRom_Click(null, new RoutedEventArgs());
                    break;

                case 'R':
                    ReloadRom_Click(null, new RoutedEventArgs());
                    break;
            }

        }
        private void checkBox2_Checked(object sender, RoutedEventArgs e)
        {
            this.Topmost = true;
        }
        private void checkBox2_Unchecked(object sender, RoutedEventArgs e)
        {
            this.Topmost = false;
        }

        private void OpenRom_Click(object sender, RoutedEventArgs e)
        {
            RomAllData = FileHandler.OpenRom();
            if(RomAllData == null) { return; }
            RomNameField.Text = RomBinaryReader.GetRomName(RomAllData);
            UpdateItemsList();
        }
        private void SaveRom_Click(object sender, RoutedEventArgs e)
        {

            if (currentTabType == TabType.ItemInfo)
            {
                if (ItemsListBox.SelectedIndex != -1)
                {
                    String[] ItemData = new String[14];

                    String endChar = FormatConverter.BinaryTextToText("ff");

                    if (!ItemNameTextBox.Text.EndsWith(endChar))
                    {
                        ItemNameTextBox.Text += endChar;
                    }
                    if (FormatConverter.HexStringToByteArray(FormatConverter.TextToBinaryText(ItemNameTextBox.Text)).Length <= 10)
                    {
                        ItemData[0] = ItemNameTextBox.Text;
                        ItemData[1] = ViewModel.ItemCode;
                        ItemData[2] = AmountTextBox.Text;
                        ItemData[3] = ItemEffectsComboBox.SelectedIndex.ToString();
                        ItemData[4] = EffectValue.Text;
                        ItemData[5] = ItemCaptionTextBox.Text;
                        ItemData[6] = AllowItemPossession.SelectedIndex.ToString();
                        ItemData[7] = UnknownValue.Text;
                        ItemData[8] = (Pocket.SelectedIndex + 1).ToString();

                        if (Pocket.SelectedIndex == 2)
                        {
                            ItemData[9] = SpecialID.Text;
                        }
                        else
                        {
                            ItemData[9] = FieldType.SelectedIndex.ToString();
                        }

                        ItemData[10] = FieldAddress.Text;
                        ItemData[11] = BattleType.SelectedIndex.ToString();
                        ItemData[12] = BattleAddress.Text;
                        ItemData[13] = SpecialID.Text;

                        byte[] ChangedRomAllData = RomBinaryWriter.WriteItemInfo(RomAllData, ItemsListBox.SelectedIndex, ItemData);
                        FileHandler.SaveRom(this, ChangedRomAllData, currentTabType);
                        ReloadRom_Click(null, new RoutedEventArgs());
                    }
                    else
                    {
                        System.Media.SystemSounds.Hand.Play();
                        System.Windows.Forms.MessageBox.Show("アイテム名は+"+endChar+"+を含め10byte以内に収めてください", "エラー");
                    }

                }
                else
                {
                    System.Media.SystemSounds.Hand.Play();
                    System.Windows.Forms.MessageBox.Show("アイテムが選択されていません", "エラー");
                }
            }
            
        }

        private void ReloadRom_Click(object sender, RoutedEventArgs e)
        {
            FileHandler.ReloadRom(this);
            UpdateItemsList();
        }

        public void UpdateItemsList()
        {
            int selectedIndex = ItemsListBox.SelectedIndex;
            
            // アイテム名一覧取得とリスト追加
            ItemNames = RomBinaryReader.GetItemNamesList(RomAllData);
            
            // 選択をクリア
            ItemsListBox.SelectedItem = null;

            Items.Clear();
            ItemEffects.Clear();

            foreach (string ItemName in ItemNames)
            {
                Items.Add(ItemName);
            }

            ItemsListBox.ItemsSource = Items;

            var itemEffects = FileHandler.LoadItemEffects();

            int ItemCount = 0;

            foreach (var itemEffect in itemEffects)
            {
                String ItemEffectContent = $"{itemEffect.Key}: {itemEffect.Value}";
                ItemEffects.Add(ItemEffectContent);
                ItemCount++;
            }
            ItemEffectsListHeight = ItemCount * 22;
            ItemEffectsComboBox.ItemsSource = ItemEffects;
            ItemsListBox.SelectedIndex = selectedIndex;
        }

        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //offset.Text = listBox.SelectedItem.ToString();
        }

        private void offset_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void RomNameField_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = SearchTextBox.Text;

            if (string.IsNullOrEmpty(searchText))
            {
                ItemsListBox.ItemsSource = Items;
            }
            else
            {
                var filteredItems = new ObservableCollection<string>(Items.Where(item => item.Contains(searchText) || item.Contains(searchText.ToUpper())));
                ItemsListBox.ItemsSource = filteredItems;
            }
        }

        private void ItemsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = ItemsListBox.SelectedItem as string;
            
            if (selectedItem != null)
            {
                int selectedIndex = 0;
                
                for (int i = 0; i < ItemNames.Length; i++)
                {
                    if (ItemNames[i] == selectedItem)
                    {
                        selectedIndex = i;
                        break;
                    }
                }
                
                String[] ItemData = RomBinaryReader.GetItemData(this, RomAllData, selectedIndex);
                
                ItemNameTextBox.Text = ItemData[0];
                ViewModel.ItemCode  = ItemData[1];
                AmountTextBox.Text = ItemData[2];
                ItemEffectsComboBox.SelectedIndex = int.Parse(ItemData[3]);
                EffectValue.Text = ItemData[4];
                ItemCaptionTextBox.Text = ItemData[5];
                AllowItemPossession.SelectedIndex = int.Parse(ItemData[6]);
                UnknownValue.Text = ItemData[7];
                Pocket.SelectedIndex = int.Parse(ItemData[8]) -1;

                if (3 == int.Parse(ItemData[8]))
                {
                    FieldType.SelectedIndex = 5;
                }
                else
                {
                    FieldType.SelectedIndex = int.Parse(ItemData[9]);
                }

                FieldAddress.Text = ItemData[10];
                BattleType.SelectedIndex = int.Parse(ItemData[11]);
                BattleAddress.Text = ItemData[12];
                SpecialID.Text = ItemData[13];
                
                // アイテム画像取得
                byte[] pic, palette;
                (pic, palette) = RomBinaryReader.GetIconData(RomAllData, selectedIndex);
                
                // Byte配列にBMPのバイナリデータを取得
                byte[] picture = BMPHandler.GetFullBmp(pic, palette);
                
                // 以前のリソースを解放
                if (ImageControl.Source is BitmapImage oldImage)
                {
                    oldImage.StreamSource?.Dispose(); // 古いストリームを解放
                }
                ImageControl.Source = null;
                /*
                // Byte配列からImageSourceを生成
                BitmapImage image = new BitmapImage();
                using (MemoryStream memoryStream = new MemoryStream(picture))
                {
                    memoryStream.Position = 0; // ストリームの位置をリセット
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad; // ストリームを解放するため
                    image.StreamSource = memoryStream;
                    image.EndInit();
                    image.Freeze(); // スレッドセーフにする
                }
                
                // WPFのImageコントロールに表示
                ImageControl.Source = image;
                RenderOptions.SetBitmapScalingMode(ImageControl, BitmapScalingMode.NearestNeighbor);*/
            }
        }
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ItemNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void ItemCaptionTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        private void AmountTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!ViewModel.ValidateAmount(AmountTextBox.Text))
            {
                AmountTextBox.Text = "9999";
            }
        }

        private void ComboBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {

        }

        private void EffectValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!ViewModel.ValidateEffectValue(EffectValue.Text))
            {
                EffectValue.Text = "255";
            }
        }

        private void UnknownValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            ViewModel.ValidateByte(UnknownValue.Text);
        }

        private void SpecialID_TextChanged(object sender, TextChangedEventArgs e)
        {
            ViewModel.ValidateByte(SpecialID.Text);
        }

        private void FieldAddress_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!ViewModel.IsValidCustomHex(FieldAddress.Text))
            {
                MessageBox.Show("エラー: 入力値は16進数の3byte以内で入力してください。\n拡張領域の場合は先頭が\n1@@@@@@か9@@@@@@か\n01@@@@@@か09@@@@@@の形式である必要があります。", "入力エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                FieldAddress.Text = FieldAddress.Text.Substring(2);
            }
        }

        private void BattleAddress_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!ViewModel.IsValidCustomHex(BattleAddress.Text))
            {
                MessageBox.Show("エラー: 入力値は16進数の3byte以内で入力してください。\n拡張領域の場合は先頭が\n1@@@@@@か9@@@@@@か\n01@@@@@@か09@@@@@@の形式である必要があります。", "入力エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                FieldAddress.Text = "";
            }
        }

        private void Pocket_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Pocket.SelectedIndex == 2)
            {
                FieldType.SelectedIndex = 5;
            }
        }

        private void Tab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is TabItem addedTabItem)
            {
                switch (addedTabItem.Header)
                {
                    case "アイテム情報":
                        currentTabType = TabType.ItemInfo;
                        break;
                    case "TabItem":
                        currentTabType = TabType.PcItemInfo;
                        break;
                }
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            int index = Int32.Parse("a1ebb0", System.Globalization.NumberStyles.HexNumber);
            int index2 = Int32.Parse("a1eb80", System.Globalization.NumberStyles.HexNumber);

            var selectedItem = ItemsListBox.SelectedItem as string;

            byte[] pic, palette;

            int selectedIndex = 0;

                for (int i = 0; i < ItemNames.Length; i++)
                {
                    if (ItemNames[i] == selectedItem)
                    {
                        selectedIndex = i;
                        break;
                    }
                }

            (pic, palette) = RomBinaryReader.GetIconData(RomAllData, selectedIndex);

            //IconPalette.Text = RomBinaryReader.GetIconData(RomAllData, selectedIndex).ToString();

            //IconPic.Text = RomBinaryReader.GetIconData(RomAllData,selectedIndex).ToString("x2");

            Byte[] picture = BMPHandler.GetFullBmp(pic,palette);
            
            ItemCaptionTextBox.Text = "";
            
            for (int i = 0; i < picture.Length; i++) {
                ItemCaptionTextBox.Text += picture[i].ToString("x2") + " ";
            }



            //ItemCaptionTextBox.Text = LZ77Handler.GetDecompressedLength(RomAllData, selectedIndex).ToString("x2");
            /*
            for (int i=0; i< DecompressedLength; i++)
            {
                RomAllData[index + i] = pic[i];
            }
            for (int i = 0; i < 32; i++)
            {
                RomAllData[index2 + i] = palette[i];
            }

            FileHandler.SaveRom(this, RomAllData, currentTabType);
            ReloadRom_Click(null, new RoutedEventArgs());
            */
        }
    }
}
