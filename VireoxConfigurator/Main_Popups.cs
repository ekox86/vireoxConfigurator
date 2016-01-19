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
        private void tb_startAddress_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Return) return;
            btn_okReAddress_Click(null, null);
        }


        private void btn_noReAddress_Click(object sender, RoutedEventArgs e)
        {
            readdress_popup.DataContext = null;
            readdress_popup.IsOpen = false;
        }
        private void btn_okReAddress_Click(object sender, RoutedEventArgs e)
        {
            int value;
            if (!Int32.TryParse(tb_startAddress.Text, out value))
            {
                MessageBox.Show("Formato dell'indirizzo non valido");
                return;
            }
            Nodo n = readdress_popup.DataContext as Nodo;
            List<Variable> vars = new List<Variable>();
            n.getVariables(vars);
            foreach (Variable v in vars)
            {
                v.Address = value++;                
            }
            progetti[0].setFreeGMAddress();
            readdress_popup.IsOpen = false;
        }


 
        /// <summary>
        /// Mostra il popup di attesa durante un'operazione lunga (ad es. apertura o salvataggio file)
        /// </summary>
        /// <param name="message">Messaggio da visualizzare durante l'attesa</param>
        private void showWaitingPopup(string message)
        {
            waitingLabel.Content = message;
            waiting.IsOpen = true;
        }
        /// <summary>
        /// Nasconde il popup di attesa quando l'operazione è terminata.
        /// </summary>
        private void hideWaitingPopup()
        {
            waitingLabel.Content = "";
            waiting.IsOpen = false;
        }
    }
}
