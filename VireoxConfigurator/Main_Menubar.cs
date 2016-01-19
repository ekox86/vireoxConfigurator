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
using System.Reflection;
using System.Diagnostics;

namespace VireoxConfigurator
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IDisposable
    {

        private void mbar_guida_click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(currentDir + "\\help\\GPM Gateway Help.chm");
            }
            catch (Exception ce)
            {
                Logger.Log(ce.Message, "Red");
            }
        }
        private void mbar_salvaconnome_click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (gridChanged || progetti[0].Properties["Script"] != textEditor.Text)
                {
                    if (MessageBox.Show("Applicare le modifiche alla schermata corrente prima di salvare?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        //DataGrid dg = detailPresenter.GetChildOfType<DataGrid>();
                        btn_save_Click(detailPresenter, null);
                        progetti[0].Properties["Script"] = textEditor.Text;
                    }
                }
                SaveFileDialog sfd = new SaveFileDialog();
                int newRelease = progetti[0].release + 1;
                if (!String.IsNullOrEmpty(progetti[0].Properties["Nuovo progetto"]))
                    sfd.FileName = progetti[0].Properties["Nuovo progetto"] + " - r" + newRelease.ToString("00"); 
                if (Directory.Exists(saveDir))
                    sfd.InitialDirectory = saveDir;
                sfd.AddExtension = true;
                sfd.DefaultExt = ".xml";
                sfd.Filter = "Vireox Project File|*.vrx";
                if (!(bool)sfd.ShowDialog()) return;
                progetti[0].release = newRelease;
                actualprojectFileName = sfd.FileName;
                updateMRU(sfd.FileName);
                showWaitingPopup("Saving file " + System.IO.Path.GetFileName(sfd.FileName) + "...");
                progetti[0].Properties["Script"] = textEditor.Text;
                bw_save.RunWorkerAsync(sfd.FileName);
            }
            catch (Exception xe)
            {
                System.Windows.MessageBox.Show(xe.Message);
            }
        }
        private void mbar_aprifile_click(object sender, RoutedEventArgs e)
        {
            string fileName;
            try
            {
                if (sender.GetType() == typeof(string))
                    fileName = sender as string;
                else
                {
                    OpenFileDialog ofd = new OpenFileDialog();
                    ofd.CheckFileExists = true;
                    if (Directory.Exists(saveDir))
                        ofd.InitialDirectory = saveDir;
                    else
                        MessageBox.Show("Attenzione, la directory " + saveDir + " non esiste.");
                    ofd.DefaultExt = ".gprj";
                    ofd.Filter = "GPM Project Files|*.gprj;*.xml;*.vrx";
                    if (!(bool)ofd.ShowDialog()) return;
                    fileName = ofd.FileName;
                }
                closeActualProject();
                actualprojectFileName = fileName;
                if (progetti[0] != null) progetti[0].Children.Clear();
                updateMRU(fileName);
                openFile_bw.RunWorkerAsync(fileName);
                showWaitingPopup("Opening file " + System.IO.Path.GetFileName(fileName) + "...");
            }
            catch (Exception xe)
            {
                System.Windows.MessageBox.Show(xe.Message);
                throw;
            }
        }
        private void mbar_verificaConf_click(object sender, RoutedEventArgs e)
        {
            if (progetti[0] == null)
            {
                Logger.Log("Nessun progetto caricato.");
                return;
            }
            BackgroundWorker bw_check = new BackgroundWorker();
            bw_check.DoWork += Bw_check_DoWork;
            bw_check.RunWorkerCompleted += Bw_check_RunWorkerCompleted;
            bw_check.RunWorkerAsync();

        }
        private void mbar_nuovoprogetto_click(object sender, RoutedEventArgs e)
        {
            closeActualProject();
            albero.ItemsSource = "";
            progetti[0] = new Project(true);
            progetti[0].setDefaultPropertyList();
            albero.ItemsSource = progetti;
            albero.Resources["varProtocols"] = progetti[0].EnabledVarProtocols;
            albero.Resources["comProtocols"] = progetti[0].EnabledComProtocols;
        }
        private void mbar_salva_click(object sender, RoutedEventArgs e)
        {
            if (gridChanged || progetti[0].Properties["Script"] != textEditor.Text)
            {
                if (MessageBox.Show("Applicare le modifiche alla schermata corrente prima di salvare?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    //DataGrid dg = detailPresenter.GetChildOfType<DataGrid>();
                    btn_save_Click(detailPresenter, null);
                    progetti[0].Properties["Script"] = textEditor.Text;
                }
            }
            if (String.IsNullOrWhiteSpace(actualprojectFileName) || System.IO.Path.GetExtension(actualprojectFileName).ToLower() == ".gprj")
            {
                mbar_salvaconnome_click(sender, e);
            }
            else
            {
                showWaitingPopup("Saving file " + System.IO.Path.GetFileName(actualprojectFileName) + "...");
                progetti[0].Properties["Script"] = textEditor.Text;
                bw_save.RunWorkerAsync(actualprojectFileName);
            }
        }
        private void mbar_esporta_click(object sender, RoutedEventArgs e)
        {
            if (gridChanged || progetti[0].Properties["Script"]!=textEditor.Text)
            {
                if (MessageBox.Show("Modifiche non salvate. Vuoi salvarle?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    //DataGrid dg = detailPresenter.GetChildOfType<DataGrid>();
                    btn_save_Click(detailPresenter, null);
                    progetti[0].Properties["Script"] = textEditor.Text;
                }
            }

            try
            {
                export_bw.RunWorkerAsync();
            }
            catch (Exception xe)
            {
                System.Windows.MessageBox.Show(xe.Message);
                throw;
            }
        }
        private void mbar_esportaeriavvia_click(object sender, RoutedEventArgs e)
        {
            if (gridChanged || progetti[0].Properties["Script"] != textEditor.Text)
            {
                if (MessageBox.Show("Modifiche non salvate. Vuoi salvarle?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    //DataGrid dg = detailPresenter.GetChildOfType<DataGrid>();
                    btn_save_Click(detailPresenter, null);
                    progetti[0].Properties["Script"] = textEditor.Text;
                }
            }
            exportReboot_bw.RunWorkerAsync();
        }
        private void mbaritem_help_click(object sender, RoutedEventArgs e)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fileVersionInfo.ProductVersion;
            MessageBox.Show("Version: " + version,"Informazioni su VireoxConfigurator");
        }
        private void mbar_riavvia_click(object sender, RoutedEventArgs e)
        {
            exportReboot_bw.RunWorkerAsync("onlyreboot");
        }
        private void LastProjects_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mi = e.OriginalSource as MenuItem;
            if (!File.Exists(mi.Header as string)) return;
            mbar_aprifile_click(mi.Header, null);

        }
        private void esportazioni_click(object sender, RoutedEventArgs e)
        {
            try {
                Process.Start("explorer.exe", exportDir);
            }
            catch (Exception ce)
            {
                Logger.Log(ce.Message, "Red");
            }
        }
    }
}
