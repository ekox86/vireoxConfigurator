using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Xml.Serialization;
using System.IO;
using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace VireoxConfigurator
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IDisposable
    {
        private string arg;

        public MainWindow(string arg)
        {
            this.arg = arg;
        }

        private void btn_save_Click(object sender, RoutedEventArgs e)
        {
            var elements = detailPresenter.FindAllVisualDescendants().Where(x => Regex.IsMatch(x.Name, "editMethod"));
            foreach (var element in elements)
            {
                BindingExpression binding = null;
                if (element.GetType() == typeof(ComboBox))
                    binding = element.GetBindingExpression(ComboBox.TextProperty);
                if (element.GetType() == typeof(TextBox))
                    binding = element.GetBindingExpression(TextBox.TextProperty);
                if (element.GetType() == typeof(CheckBox))
                    binding = element.GetBindingExpression(CheckBox.IsCheckedProperty);
                if (binding != null && (bool)(typeof(BindingExpression).GetProperty("IsDirty").GetValue(binding, null)))
                    binding.UpdateSource();
            }
            DataGrid propGrid = (DataGrid)detailPresenter.GetChildOfType<DataGrid>();
            if (propGrid!=null)
                propGrid.Items.Refresh();
            gridChanged = false;
            projectChanged = true;
            
        }
        private void btn_saveProject_Click(object sender, RoutedEventArgs e)
        {
            btn_save_Click(sender, e);
            progetti[0].updateEnabledProtocolList();
        }
        private void btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            var p = detailPresenter.FindAllVisualDescendants().Where(x => x.Name == "propGrid");
            DataGrid propgrid = p.First() as DataGrid;
            propgrid.Items.Refresh();
        }
        private void combobox_editMethod_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.Key != Key.Return) return;
            ComboBox cb = sender as ComboBox;
            BindingExpression binding = cb.GetBindingExpression(ComboBox.TextProperty);
            binding.UpdateSource();
            projectChanged = true;
        }
        private void textbox_editMethod_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Return) return;
            TextBox cb = sender as TextBox;
            BindingExpression binding = cb.GetBindingExpression(TextBox.TextProperty);
            binding.UpdateSource();
            projectChanged = true;
        }
        private void checkbox_editMethod_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Return) return;
            CheckBox cb = sender as CheckBox;
            BindingExpression binding = cb.GetBindingExpression(CheckBox.IsCheckedProperty);
            binding.UpdateSource();
            projectChanged = true;
        }
        private void combobox_editMethod_LostFocus(object sender, RoutedEventArgs e)
        {
            ComboBox cbox = sender as ComboBox;
            var binding= cbox.GetBindingExpression(ComboBox.TextProperty);
            if ((bool)(typeof(BindingExpression).GetProperty("IsDirty").GetValue(binding, null)))
            {
                cbox.Background = Brushes.Yellow;
                gridChanged = true;
            }
        }
        private void textbox_editMethod_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox cbox = sender as TextBox;
            var binding = cbox.GetBindingExpression(TextBox.TextProperty);
            if ((bool)(typeof(BindingExpression).GetProperty("IsDirty").GetValue(binding, null)))
            {
                cbox.Background = Brushes.Yellow;
                gridChanged = true;
            }
        }
        private void checkBox_editMethod_LostFocus(object sender, RoutedEventArgs e)
        {
            CheckBox cbox = sender as CheckBox;
            WrapPanel cell = cbox.Parent as WrapPanel;
            var binding=cbox.GetBindingExpression(CheckBox.IsCheckedProperty);
            if ((bool)(typeof(BindingExpression).GetProperty("IsDirty").GetValue(binding, null)))
            {
                cell.Background = Brushes.Yellow;
                gridChanged = true;
            }
        }
        private void checkbox_leaf_LostFocus(object sender, RoutedEventArgs e)
        {
            CheckBox cbox = sender as CheckBox;
            WrapPanel cell = cbox.Parent as WrapPanel;
            var binding = cbox.GetBindingExpression(CheckBox.IsCheckedProperty);
            if ((bool)(typeof(BindingExpression).GetProperty("IsDirty").GetValue(binding, null)))
            {
                //cell.Background = Brushes.Yellow;
                gridChanged = true;
            }
        }
        private void textbox_leaf_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox cbox = sender as TextBox;
            var binding =cbox.GetBindingExpression(TextBox.TextProperty);
            if ((bool)(typeof(BindingExpression).GetProperty("IsDirty").GetValue(binding, null)))
            {
                gridChanged = true;
            }
        }
        private void combobox_leaf_LostFocus(object sender, RoutedEventArgs e)
        {
            ComboBox cbox = sender as ComboBox;
            var binding = cbox.GetBindingExpression(ComboBox.TextProperty);
            if ((bool)(typeof(BindingExpression).GetProperty("IsDirty").GetValue(binding, null)))
            {
                gridChanged = true;
            }
        }
    }
}
