using System;
using System.Windows;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Media;
using System.Linq;
using System.Windows.Input;
using Microsoft.Win32;
using System.Xml.Serialization;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace VireoxConfigurator
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window,IDisposable
    {
        delegate void mergeDirDeleg(string master,string backup,string filter);
        public static string argFile=null;
        public Project[] progetti;
        BackgroundWorker openFile_bw;
        BackgroundWorker bw_save, export_bw,exportReboot_bw;
        string actualprojectFileName="", currentDir,saveDir,exportDir,configDir;
        Nodo importcsvnode,exportcsvnode;
        mergeDirDeleg mergeDirMethod;
        bool gridChanged = false;
        bool projectChanged = false;
        ObservableCollection<string> mru;
        string currentFileName;
        DefinitionsEditor defEditor;
        public MainWindow()
        {
            currentDir = System.AppDomain.CurrentDomain.BaseDirectory;
            saveDir = System.IO.Path.Combine(currentDir, "Project\\Save");
            exportDir = System.IO.Path.Combine(currentDir, "Project\\Export");
            configDir= System.IO.Path.Combine(currentDir, "Config");
            mergeDirMethod = mergeDirectory;
            mru = FileManager.getLastProjects();
            //if (cmdlineargs.Length>1)
            //{
            //    string filecmdline="";
            //    for (int i = 1; i < cmdlineargs.Length; i++)
            //    {
            //        filecmdline += cmdlineargs[i];
            //        if (i != cmdlineargs.Length - 1)
            //            filecmdline += " ";
            //    }                
            //    updateMRU(filecmdline);
            //}
            openFile_bw = new BackgroundWorker();
            openFile_bw.DoWork += Bw_openfile_DoWork;
            openFile_bw.RunWorkerCompleted += Bw_openfile_RunWorkerCompleted;
            bw_save = new BackgroundWorker();
            bw_save.DoWork += Bw_save_DoWork;
            bw_save.RunWorkerCompleted += Bw_save_RunWorkerCompleted;
            export_bw = new BackgroundWorker();
            export_bw.DoWork += Bw_export_DoWork;
            export_bw.RunWorkerCompleted += Bw_export_RunWorkerCompleted;
            exportReboot_bw = new BackgroundWorker();
            exportReboot_bw.DoWork += Bw_exportreboot_DoWork;
            exportReboot_bw.RunWorkerCompleted += Bw_exportreboot_RunWorkerCompleted;
            progetti = new Project[1];            
            InitializeComponent();
            Logger.setDoc(LogBox,this);
            checkDefinitions();         
            StartupClass.loadStaticDefinitions();
            LastProjects.ItemsSource = mru;
        }
        void checkDefinitions()
        {
            try {
                WebClient client = new WebClient();
                DateTime lastUpdate = DateTime.Parse(client.DownloadString("http://93.62.113.78/vireoxConfig/lastupdate.txt"));
                if (lastUpdate > File.GetLastWriteTime(".\\Resources\\StaticDefinitions.xml")) {
                    if (MessageBox.Show("Una nuova versione del file delle definizioni è disponibile. Vuoi scaricarla?", "Avviso", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        client.DownloadFile("http://93.62.113.78/vireoxConfig/StaticDefinitions.xml", "StaticDefinitions.xml");
                        File.Copy("StaticDefinitions.xml", ".\\Resources\\StaticDefinitions.xml", true);
                        File.Delete("StaticDefinitions.xml");
                        Logger.Log("Il file delle definizioni è stato correttamente aggiornato alla versione più recente.");
                    }
                    else
                    {
                        Logger.Log("Il file delle definizioni non è aggiornato alla versione più recente.");
                    }
                }
                else
                {
                    Logger.Log("Il file delle definizioni è aggiornato alla versione più recente.");
                }
            }
            catch (WebException we)
            {
                Logger.Log("Non è stato possibile controllare online il file delle definizioni: " + we.Message,"Red");
            }
            catch (IOException ioe)
            {
                Logger.Log("Non è stato possibile sovrascrivere il file delle definizioni: " + ioe.Message, "Red");
            }
            catch (Exception ge)
            {
                Logger.Log("Problema durante il controllo del file delle definizioni " + ge.Message, "Red");
            }

            
        }
        private void Bw_exportreboot_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            hideWaitingPopup();
            int errors = (int)e.Result;
            if (errors > 0)
            {
                if (MessageBox.Show("Sono presenti " + errors + " errori. Continuare?", "Avviso", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    exportReboot_bw.RunWorkerAsync("confirmed");
                }
            }
            else
            {
               
            }
        }
        private void Bw_exportreboot_DoWork(object sender, DoWorkEventArgs e)
        {
            try {
                if (e.Argument == null)
                {
                    ConfigChecker cch = new ConfigChecker(progetti[0]);
                    cch.checkAll();
                    if (cch.errors > 0)
                    {
                        e.Result = cch.errors;
                        return;
                    }
                    Logger.Log("Verifica terminata");
                }
                if ((string)e.Argument != "onlyreboot")
                {
                    FileManager.generateConf(progetti[0]);
                    if (progetti[0].Properties["Ridondanza Enabled"].ToLower() == "true")
                    {
                        this.Dispatcher.Invoke(mergeDirMethod, progetti[0].Properties["MasterSharedFolder"], progetti[0].Properties["BackupSharedFolder"], "*");
                    }
                    Logger.Log("Configuration  successfully exported ");
                }
                rebootGW(progetti[0].Properties["IPMaster"],progetti[0].Properties["Port Master"],true);
                if (progetti[0].Properties["Ridondanza Enabled"].ToLower()=="true")
                {
                    rebootGW(progetti[0].Properties["IPBackup"], progetti[0].Properties["Port Backup"], false);
                }
                e.Result = 0;
            }
            catch (ConfigCheckerException cce)
            {
                e.Result = 0;
                return;
            }
            catch (Exception xe)
            {
                Logger.Log("Error exporting configuration and rebooting gateway: " + xe.Message,"Red");
                e.Result = 0;
                return;

            }
        }
        private void rebootGW(string ip, string port, bool isMaster)
        {
            string gwName = isMaster ? "Master" : "Slave";
            if (isMaster && !Regex.IsMatch(progetti[0].Properties["IPMaster"], "localhost|127\\.0\\.0\\.1")) {
                if (MessageBox.Show("L'ip del master non coincide con quello del localhost. Proseguire con il riavvio?", "Avviso", MessageBoxButton.YesNo) == MessageBoxResult.No)
                    return;
            }
            if (ip == null || port == null)
            {               
                Logger.Log("Impossibile riavviare gateway "+gwName+", ip/porta non definiti.", "Red");
                return;
            }
            int p;            
            try {
                if (isMaster)
                {
                    string StarterPath = System.IO.Path.Combine(currentDir, "Starter.exe");
                    Process[] pName = Process.GetProcessesByName("Starter");
                    if (pName.Length == 0)
                    {
                        ProcessStartInfo psi = new ProcessStartInfo();
                        psi.WorkingDirectory = currentDir;
                        psi.FileName = StarterPath;
                        psi.UseShellExecute = true;
                        psi.Verb = "runas";
                        Process.Start(psi);
                        Thread.Sleep(2000);
                    }
                }
                if (!Int32.TryParse(port, out p))
                    throw new Exception("Porta non valida");
                System.Net.WebClient client = new System.Net.WebClient();
                Logger.Log("Avvio del Gateway "+ip, "DarkSlateBlue");
                client.DownloadStringAsync(new Uri("http://"+ip+":"+port+"/index.php?file=RestartGPM.bat"));
                Logger.Log("Richiesta inviata allo starter", "DarkOrchid");
                Logger.Log("Riavvio terminato");
            }
            catch (Exception e)
            {
                Logger.Log("Errore durante il riavvio del gateway " + gwName + " : " + e.Message);
            }

        }
        private void Bw_importCSV_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Nodo n = importcsvnode.Father;
            if (e.Result == null)
                Logger.Log("Error loading file ", "Red");
            else
            {
                Nodo vn = e.Result as Nodo;
                n.Remove(importcsvnode);
                n.Append(vn.Children);
                foreach (var child in vn.Children)
                    child.Father = n;
                Logger.Log("File importato correttamente.");
            }                
            hideWaitingPopup();
        }
        private void Bw_importCSV_DoWork(object sender, DoWorkEventArgs e)
        {
            try {
                if (importcsvnode.GetType()==typeof(VarNodo))
                    e.Result= FileManager.importCSVVar((string)e.Argument);
                else
                    e.Result= FileManager.importCSVCOM((string)e.Argument);
            }
            catch (Exception xe)
            {
                e.Result = null;
                return;
            }
        }
        private void Bw_export_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //Logger.Log( e.Result );
            hideWaitingPopup();
            int errors = (int)e.Result;
            if (errors >0)
            {
                if (MessageBox.Show("Sono presenti " + errors + " errori. Continuare?","",MessageBoxButton.YesNo)==MessageBoxResult.Yes)
                {
                    export_bw.RunWorkerAsync("confirmed");
                    return;
                }
            }
            if (progetti[0].Properties["Ridondanza Enabled"].ToLower()=="true")
            {
                Logger.Log("Avvio del merging delle librerie...");
                mergeDirectory(progetti[0].Properties["MasterSharedFolder"], progetti[0].Properties["BackupSharedFolder"]);
                Logger.Log("Merging delle librerie terminato.");
            }
        }
        private void Bw_export_DoWork(object sender, DoWorkEventArgs e)
        {
            try {
                if (e.Argument == null)
                {
                    ConfigChecker cch = new ConfigChecker(progetti[0]);
                    cch.checkAll();
                    if (cch.errors > 0)
                    {
                        e.Result = cch.errors;
                        return;
                    }
                }                
                Logger.Log("Verifica terminata");
                FileManager.generateConf(progetti[0]);
                Logger.Log("Esportazione terminata correttamente.");
                e.Result = 0;
            }
            catch (ConfigCheckerException cce)
            {
                Logger.Log("Esportazione annullata.");
                e.Result = 0;
                return;
            }
            catch (Exception xe)
            {
                Logger.Log ("Errore durante l'esportazione della configurazione " + xe.Message,"Red");
                e.Result = 0;
                return;
            }
        }
        private void Bw_openfile_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try {
                albero.ItemsSource = null;
                albero.ItemsSource = progetti;
                albero.Resources["varProtocols"] = progetti[0].EnabledVarProtocols;
                albero.Resources["comProtocols"] = progetti[0].EnabledComProtocols;
                if (!progetti[0].Properties.ContainsKey("Script"))
                    progetti[0].Properties["Script"]="";
                textEditor.Text = progetti[0].Properties["Script"];
                textEditor.UpdateLayout();
                Logger.Log((string)e.Result);
                hideWaitingPopup();
            }
            catch (Exception xe)
            {
                Logger.Log(xe.Message +  "  " +xe.StackTrace, "Red");
                hideWaitingPopup();
            }
        }
        private void Bw_openfile_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                progetti[0] = FileManager.openProject((string)e.Argument);
                e.Result = "File " + e.Argument + " correctly loaded.";
                projectChanged = false;

            }
            catch (Exception xe)
            {
                e.Result = "Error loading file " + e.Argument + ":" + xe.Message;
                return;
            }
        }   
        private void Bw_save_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Logger.Log( e.Result as string);
            hideWaitingPopup();
        }
        private void Bw_save_DoWork(object sender, DoWorkEventArgs e)
        {
            try {
                FileManager.save((string)e.Argument, progetti[0]);
                projectChanged = false;
                e.Result = "File  " + e.Argument + " correctly saved." ;
            }
             catch (Exception ex)
            {
                e.Result = "Error saving file " + e.Argument + ":" + ex.Message;
                return;
            }
        }
        private void Bw_check_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Logger.Log("Verifica terminata");
        }
        private void Bw_check_DoWork(object sender, DoWorkEventArgs e)
        {
            try {
                ConfigChecker cch = new ConfigChecker(progetti[0]);
                cch.checkAll();
            }
            catch(Exception ce) {
                Logger.Log("Errore ConfigChecker" + ce.Message + " " + ce.StackTrace, "Red");
            }
        }        
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            //Application.Current.Shutdown();
        }

        private void window_Loaded(object sender, RoutedEventArgs e)
        {
            if (mru.Count!=0)
            {
                mbar_aprifile_click(mru[0], null);
            }
        }
        private void openScriptClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.CheckFileExists = true;
            dlg.DefaultExt = ".php";
            dlg.InitialDirectory = configDir;
            dlg.Filter = "Script PHP|*.php";
            if (dlg.ShowDialog() ?? false)
            {
                currentFileName = dlg.FileName;
                if (!String.IsNullOrWhiteSpace(textEditor.Text))
                    if (MessageBox.Show("Aprire un nuovo script comporterà la perdita di modifiche non salvate. Vuoi continuare?", "Avviso", MessageBoxButton.YesNo) == MessageBoxResult.No)
                        return;
                textEditor.Load(currentFileName);
            }
        }

        private void MenuItem_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            defEditor = new DefinitionsEditor();
            defEditor.Show();
        }
        private void keypress_f1(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F1)
                mbar_guida_click(null, null);
        }
        private void exportScriptClick(object sender, RoutedEventArgs e)
        {
            progetti[0].Properties["Script"] = textEditor.Text;
            FileManager.writeScript(progetti[0]);
            //textEditor.Save(configDir + "\\user_logics.php");
        }



        private void saveNameScriptClick(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.DefaultExt = ".php";
            dlg.InitialDirectory = configDir;
            dlg.Filter="Script PHP|*.php";
            if (dlg.ShowDialog() ?? false)
            {
                currentFileName = dlg.FileName;
                textEditor.Save(currentFileName);
            }
            else
            {
                return;
            }
        }
        void closeActualProject()
        {
            actualprojectFileName = null;
            textEditor.Clear();
            if (!projectChanged || progetti[0] == null) return;
            if (MessageBox.Show("Vuoi salvare il progetto?", "Conferma", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {               
                mbar_salva_click(null, null);
            }       
        }        
        private void lbl_clearLog_Click(object sender, RoutedEventArgs e)
        {
            Logger.Clear();
        }
        private void window_Closing(object sender, CancelEventArgs e)
        {
            closeActualProject();
            FileManager.setLastProjects(mru);
        }
        public virtual void Dispose()
        {
            ((IDisposable)bw_save).Dispose();
            ((IDisposable)openFile_bw).Dispose();
            ((IDisposable)export_bw).Dispose();
        }
        public virtual void updateMRU(string p)
        {
            int index = mru.IndexOf(p);
            if (index != -1)
                mru.RemoveAt(index);
            mru.Insert(0, p);
            if (mru.Count>5)
            {
                string s = mru[mru.Count - 1];
                mru.Remove(s);
            }                
        }

        public  void mergeDirectory(string sourcePath, string targetPath, string Filter="*")
        {
            DirectoryInfo source;
            DirectoryInfo target;
            if (string.IsNullOrWhiteSpace(sourcePath))
                sourcePath = currentDir;
            if (string.IsNullOrWhiteSpace(targetPath))
            {
                Logger.Log("Errore: backup directory non configurata. Il merging delle librerie non sarà eseguito.", "Red");
                return;
            }
            try
            {
                source = new DirectoryInfo(sourcePath + "\\libs");
                target = new DirectoryInfo(targetPath + "\\libs");
                if (source.FullName.ToLower() == target.FullName.ToLower())
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Eccezione sulle cartelle libs del master e dello slave: " + ex.Message, "Red");
                return;
            }

            // Check if the target directory exists, if not, create it.
            if (Directory.Exists(target.FullName) == false)
            {
                try
                {
                    Directory.CreateDirectory(target.FullName);
                }
                catch (Exception ex)
                {
                    Logger.Log("Eccezione sulla creazione della cartella libs del backup gateway: " + ex.Message, "Red");
                    return;
                }
            }

            // Copy each file into it's new directory.
            foreach (FileInfo fiSource in source.GetFiles(Filter))
            {
                try
                {
                    String destFile = Path.Combine(target.FullName, fiSource.Name);
                    if (!File.Exists(destFile))
                    {
                        //Se non esiste lo copio
                        Logger.Log("Copio " + fiSource.Name + " nella cartella delle libs del Backup perchè non esiste", "Green");
                        fiSource.CopyTo(destFile, true);
                    }
                    else
                    {
                        FileInfo fiTarget = new FileInfo(destFile);
                        if (fiSource.LastWriteTime != fiTarget.LastWriteTime)
                        {
                            //se la data dell'ultima modifica è diversa chiedo cosa vuole fare
                            string recente = "più";
                            if (fiSource.LastWriteTime < fiTarget.LastWriteTime) recente = "meno";
                            MessageBoxResult mbr=MessageBox.Show("Il file libs/" + fiSource.Name.ToUpper() + " del GPM primario è " + recente + " recente dello stesso sul GPM secondario. Usare il primario?", "Avviso", MessageBoxButton.YesNo);
                            if (mbr==MessageBoxResult.Yes)
                            {
                                Logger.Log("Copio " + fiSource.Name + " nella cartella delle libs del Backup", "Green");
                                fiSource.CopyTo(destFile, true);
                            }
                            else 
                            {
                                Logger.Log("Copio " + fiSource.Name + " nella cartella delle libs del Master", "Green");
                                fiTarget.CopyTo(fiSource.FullName, true);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                        Logger.Log("Eccezione sul merge del file " + fiSource.Name.ToUpper() + ": " + ex.Message, "Red");
                    continue;
                }
            }
        }


    }
}
