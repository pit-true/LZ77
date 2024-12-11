using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace GalapagosItemManager
{
    public class ViewModel : INotifyPropertyChanged
    {
        private string _ItemCode;

        public string ItemCode
        {
            get { return _ItemCode; }
            set
            {
                _ItemCode = value;
                OnPropertyChanged(nameof(ItemCode));
            }
        }

        private ObservableCollection<string> _Items;

        public ObservableCollection<string> Items
        {
            get { return _Items; }
            set
            {
                _Items = value;
                OnPropertyChanged(nameof(Items));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ViewModel()
        {
            Items = new ObservableCollection<string>();
        }

        private readonly ObservableCollection<string> _ItemEffects = new ObservableCollection<string>();

        public ObservableCollection<string> ItemEffects
        {
            get { return _ItemEffects; }
            set
            {
                _Items = value;
                OnPropertyChanged(nameof(ItemEffects));
            }
        }


        public void UpdateItemsList(byte[] romAllData)
        {
            var itemNames = RomBinaryReader.GetItemNamesList(romAllData);
            Items.Clear();

            foreach (string itemName in itemNames)
            {
                Items.Add(itemName);
            }
        }

        public static bool IsValidHexByte(string hex)
        {
            // 16進数の形式になっているか確認
            if (int.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out int value))
            {
                // 1バイトの範囲内であることを確認 (0x00 ～ 0xFF)
                return value >= 0x00 && value <= 0xFF;
            }

            // 形式が違うまたは範囲外の場合
            return false;
        }

        public static bool IsValidCustomHex(string hex)
        {
            int len = hex.Length;

            // 文字列全体が16進数であるかを確認
            if (!System.Text.RegularExpressions.Regex.IsMatch(hex, @"\A\b[0-9a-fA-F]+\b\Z"))
            {
                return false;
            }

            // 長さが6以下の場合はそのまま通す
            if (len <= 6)
            {
                return true;
            }
            // 長さが7の場合は先頭が1であることを確認
            else if (len == 7)
            {
                return hex.StartsWith("0") || hex.StartsWith("1") || hex.StartsWith("9");
            }
            // 長さが8の場合は先頭が01または09であることを確認
            else if (len == 8)
            {
                return hex.StartsWith("01") || hex.StartsWith("09");
            }
            else
            {
                // それ以外の場合はエラー
                return false;
            }
        }

        public static void ValidateByte(String text)
        {
            if (!IsValidHexByte(text))
            {
                MessageBox.Show("エラー: 入力値は16進数で、1バイトの範囲内（0x00～0xFF）である必要があります。", "入力エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public static bool ValidateEffectValue(String text)
        {
            if (int.TryParse(text, System.Globalization.NumberStyles.Integer, null, out int value))
            {
                if (!(value >= 00 && value <= 255))
                {
                    MessageBox.Show("エラー: 入力値は1バイトの範囲内（0～255）である必要があります。", "入力エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
                else { return true; }
            }
            else
            {
                MessageBox.Show("エラー: 入力値は1バイトの範囲内（0～255）である必要があります。", "入力エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
        public static bool ValidateAmount(String text)
        {
            if (int.TryParse(text, System.Globalization.NumberStyles.Integer, null, out int value))
            {
                if (!(value >= 00 && value <= 65535))
                {
                    MessageBox.Show("エラー: 入力値は2バイトの範囲内（0～65535）である必要があります。", "入力エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
                /* 多分不要　きんのたまはショップに売られていないが、10000円に設定されているため警告が出てしまう
                else {
                    if (value >= 10000) {
                        MessageBox.Show("警告: 入力値が10000を超えると\nショップ販売時に金額の4桁目が「?」と表示されます。", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return true;
                    }
                    else
                    {
                        return true;
                    }
                }*/
                return true;
            }
                else
                {
                MessageBox.Show("エラー: 入力値は1バイトの範囲内（0～65535）である必要があります。", "入力エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

    }
}