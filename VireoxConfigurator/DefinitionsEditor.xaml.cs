using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace VireoxConfigurator
{
    /// <summary>
    /// Logica di interazione per DefinitionsEditor.xaml
    /// </summary>
    public partial class DefinitionsEditor : Window
    {
        public DefinitionsEditor()
        {
            InitializeComponent();
            
        }

        private void btn_flussi_Click(object sender, RoutedEventArgs e)
        {
            listBox.ItemsSource = VarDefinitions.Map.Values;
        }

        private void btn_canali_Click(object sender, RoutedEventArgs e)
        {
            listBox.ItemsSource = ComDefinitions.Map.Values;
        }

        private void btn_progetto_Click(object sender, RoutedEventArgs e)
        {
            //ProjectDefinitions[] pList = new ProjectDefinitions[1];
           
            //listBox.ItemsSource = null;           
        }

    }
}
