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
        Nodo clipboardNode;
        bool copy;
        private void albero_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

            this.Cursor = Cursors.Wait;
            if (gridChanged)
            {
                if (MessageBox.Show("Modifiche non salvate. Vuoi salvarle?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    //DataGrid dg = detailPresenter.GetChildOfType<DataGrid>();
                    btn_save_Click(detailPresenter, null);
                }
            }
            if (e.OldValue!=null && e.OldValue.GetType() == typeof(Project))
                progetti[0].updateEnabledProtocolList();
            detailPresenter.Content = e.NewValue;
            gridChanged = false;
            this.Cursor = Cursors.Arrow;
        }
        private void mitem_Remove_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mi = sender as MenuItem;
            Nodo n = mi.DataContext as Nodo;
            if (n.GetType() == typeof(Variable))
            {
                Variable v = (Variable)n;
                if (v.Address == progetti[0].FreeGMAddress - 1)
                    progetti[0].FreeGMAddress--;
            }
            Nodo padre = n.Father;
            if (n.Children != null)
                n.Children.Clear();
            padre.Remove(n);
        }
        //private void mitem_rename_click(object sender, RoutedEventArgs e)
        //{
        //    rename_Popup.DataContext = ((MenuItem)sender).DataContext;
        //    rename_Popup.IsOpen = true;
        //}
        private void mitem_Disable_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mi = sender as MenuItem;
            Nodo n = mi.DataContext as Nodo;
            n.Toggle();
        }
        private void mitem_newLeaf_Click(object sender, RoutedEventArgs e)
        {
            MenuItem tb = sender as MenuItem;
            string prName = ((MenuItem)e.OriginalSource).Header as string;
            if (Regex.IsMatch(prName, "Aggiungi")) return;
            Foglia f;
            Nodo father;
            Type type = tb.DataContext.GetType();
            if (type == typeof(Variable))
            {
                father = tb.DataContext as Variable;
                f = new Flusso();
                f.Name = prName;
                f.ProtocolName = prName;
                f.setDefaultPropertyList();
                f.Father = father;
                father.Append(f);
                // newLeaf_popup.IsOpen = false;
                //cb_newFluxProtocol.ItemsSource = VarDefinitions.Map.Keys;
            }
            else
            {
                father = tb.DataContext as ComNodo;
                f = new Canale();
                f.Name = prName;
                f.ProtocolName = prName;
                f.setDefaultPropertyList();
                f.Father = father;
                father.Append(f);
            }
            ContextMenu cm = tb.Parent as ContextMenu;
            Grid g = cm.PlacementTarget as Grid;
            TreeViewItem tvi = getParentOfType<TreeViewItem>(g);
            if (!tvi.IsExpanded)
                tvi.IsExpanded = true;
            tvi.UpdateLayout();
            TreeViewItem tvd = SelectItem(f);
            enableRenameMode(tvd);
        }
        private void mitem_newReport61850(object sender, RoutedEventArgs e)
        {
            MenuItem tb = sender as MenuItem;
            Report61850 f;
            Nodo father=tb.DataContext as Nodo;
            f = new Report61850();
            f.ProtocolName = "Reports61850";
            f.setDefaultPropertyList();
            f.Father = father;
            father.Append(f);
            ContextMenu cm = tb.Parent as ContextMenu;
            Grid g = cm.PlacementTarget as Grid;
            TreeViewItem tvi = getParentOfType<TreeViewItem>(g);
            if (!tvi.IsExpanded)
                tvi.IsExpanded = true;
            tvi.UpdateLayout();
            TreeViewItem tvd = SelectItem(f);
            enableRenameMode(tvd);
        }
        private void mitem_newnode_click(object sender, RoutedEventArgs e)
        {
            MenuItem mi = sender as MenuItem;
            Nodo father = mi.DataContext as Nodo, n = null;
            Type tipo = mi.DataContext.GetType();
            int i = 0;
            string newName="Nuovo Gruppo";
            while (father.Children.FirstOrDefault(x => x.Name == newName) != null)
            {
                i++;
                newName = String.Format("Nuovo Gruppo ({0})", i);      
            }
            if (tipo == typeof(VarNodo) || father.Name == "Memoria Globale" || father.Name=="Altro")
                n = new VarNodo(newName,father,true);
            else if (tipo == typeof(ComNodo) || father.Name == "Comunicazione")
                n = new ComNodo(newName,father,true);
            else if (tipo == typeof(Report61850Nodo) || father.Name == "IEC 61850 - Reports")
                n = new Report61850Nodo(newName, father, true);
            n.Father = father;
            ContextMenu cm = mi.Parent as ContextMenu;
            Grid g = cm.PlacementTarget as Grid;
            TreeViewItem tvi = getParentOfType<TreeViewItem>(g);
            father.Children.Add(n);
            if (!tvi.IsExpanded)
                tvi.IsExpanded = true;
            tvi.UpdateLayout();
            //TreeViewItem tvd = SelectItem(n);
        }
        private void mitem_newVariable_click(object sender, RoutedEventArgs e)
        {
            MenuItem tb = sender as MenuItem;
            VarNodo father = tb.DataContext as VarNodo;
            string newName = "Nuova Variabile";
            int i = 0;
            while (father.Children.FirstOrDefault(x => x.Name == newName) != null)
            {
                i++;
                newName = String.Format("Nuova Variabile ({0})", i);
            }
            Variable nn = new Variable(newName, father, true, progetti[0].FreeGMAddress++);
            nn.Father = father;
            ContextMenu cm = tb.Parent as ContextMenu;
            Grid g = cm.PlacementTarget as Grid;
            TreeViewItem tvi = getParentOfType<TreeViewItem>(g);
            father.Children.Add(nn);
            if (!tvi.IsExpanded)
                tvi.IsExpanded = true;
            tvi.UpdateLayout();
            //TreeViewItem tvd= SelectItem(nn);
            //enableRenameMode(tvd);
            //nn.Father = father;
            //newVariable_popup.DataContext = nn;
            //newVariable_popup.IsOpen = true;
        }
        private void mitem_readdress_click(object sender, RoutedEventArgs e)
        {
            Nodo v = (Nodo)((MenuItem)sender).DataContext;
            tb_startAddress.Text =((v.getLevel()-2)*10000).ToString();
            readdress_popup.IsOpen = true;
            readdress_popup.DataContext = ((MenuItem)sender).DataContext;
        }
        private void mitem_importcsvnode_click(object sender, RoutedEventArgs e)
        {
            try
            {
                MenuItem mi = sender as MenuItem;
                ContextMenu m = mi.Parent as ContextMenu;
                Type tp = mi.DataContext.GetType();
                if (tp == typeof(VarNodo))
                    importcsvnode = mi.DataContext as VarNodo;
                else
                    importcsvnode = mi.DataContext as ComNodo;
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.DefaultExt = ".csv";
                ofd.CheckFileExists = true;
                if (Directory.Exists(exportDir))
                    ofd.InitialDirectory = exportDir;
                ofd.FileName = "*" + importcsvnode.Name + "*";
                ofd.Filter = "csv files|*.csv";
                ofd.Multiselect = false;
                ofd.CheckFileExists = true;
                if (!(bool)ofd.ShowDialog()) return;
                showWaitingPopup("Importing file " + System.IO.Path.GetFileName(ofd.FileName) + "...");
                //n.loadTreefromCSVFile(ofd.FileName);
                BackgroundWorker bw_importCSV = new BackgroundWorker();
                bw_importCSV.DoWork += Bw_importCSV_DoWork;
                bw_importCSV.RunWorkerCompleted += Bw_importCSV_RunWorkerCompleted;
                bw_importCSV.RunWorkerAsync(ofd.FileName);
                hideWaitingPopup();
            }
            catch (Exception xe)
            {
                System.Windows.MessageBox.Show(xe.Message);
            }
        }
        private void mitem_exportcsvnode_click(object sender, RoutedEventArgs e)
        {
            try
            {
                MenuItem mi = sender as MenuItem;
                ContextMenu m = mi.Parent as ContextMenu;
                Type tp = mi.DataContext.GetType();
                exportcsvnode = mi.DataContext as Nodo;
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.DefaultExt = ".csv";
                sfd.CheckFileExists = false;
                if (Directory.Exists(exportDir))
                    sfd.InitialDirectory = exportDir;
                sfd.FileName = exportcsvnode.Name +" - "+ (exportcsvnode.GetType()==typeof(ComNodo)?"Com":"Var");
                sfd.Filter = "csv files|*.csv";
                if (!(bool)sfd.ShowDialog()) return;
                showWaitingPopup("Exporting file " + System.IO.Path.GetFileName(sfd.FileName) + "...");
                //n.loadTreefromCSVFile(ofd.FileName);
                BackgroundWorker bw_exportCSV = new BackgroundWorker();
                bw_exportCSV.DoWork += Bw_exportCSV_DoWork;
                bw_exportCSV.RunWorkerCompleted += Bw_exportCSV_RunWorkerCompleted;
                bw_exportCSV.RunWorkerAsync(sfd.FileName);
                hideWaitingPopup();
            }
            catch (Exception xe)
            {
                System.Windows.MessageBox.Show(xe.Message);
            }

        }
        private void Bw_exportCSV_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Logger.Log((string)e.Result);
        }
        private void Bw_exportCSV_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                exportcsvnode.exportCSV(e.Argument as string);
                e.Result = "File esportato correttamente";
            }
            catch (Exception ce)
            {
                e.Result = "Errore durante l'esportazione: " + ce.Message + ce.StackTrace;
            }
        }
        private void mitem_rename_click(object sender, RoutedEventArgs e)
        {
            MenuItem mi = sender as MenuItem;
            ContextMenu cm = mi.Parent as ContextMenu;
            Grid g = cm.PlacementTarget as Grid;
            TextBox tbox = g.GetChildOfType<TextBox>();
            TextBlock tblock = g.GetChildOfType<TextBlock>();
            tblock.Visibility = Visibility.Hidden;
            tbox.Visibility = Visibility.Visible;
            tbox.BorderThickness = new Thickness(1);
            tbox.Focus();
            tbox.SelectAll();
        }
        private void enableRenameMode(TreeViewItem tvi)
        {
            TextBox tbox = tvi.GetChildOfType<TextBox>();
            TextBlock tblock = tvi.GetChildOfType<TextBlock>();
            tblock.Visibility = Visibility.Hidden;
            tbox.Visibility = Visibility.Visible;
            tbox.BorderThickness = new Thickness(1);
            tbox.Focus();
            tbox.SelectAll();
        }
        private void tBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox t = sender as TextBox;
            Grid g = t.Parent as Grid;
            TextBlock tblock = g.GetChildOfType<TextBlock>();
            t.BorderThickness = new Thickness(0);
            t.Visibility = Visibility.Hidden;
            tblock.Visibility = Visibility.Visible;
        }
        private void rename_f2(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != Key.F2) return;
            if (albero.SelectedItem == null || Regex.IsMatch(albero.SelectedItem.ToString(),"Progetto|Comunicazione|Memoria Globale")) return;
            TreeViewItem tvi = SelectItem((Nodo) albero.SelectedItem);
            enableRenameMode(tvi);
        }
        public TreeViewItem SelectItem(Nodo node)
        {
            Stack<Nodo> nodesStack = new Stack<Nodo>();
            Nodo currentNode = node;
            while (currentNode != null)
            {
                nodesStack.Push(currentNode);
                currentNode = currentNode.Father;
            }
            TreeViewItem currentItem = albero.ItemContainerGenerator.ContainerFromItem(albero.Items[0]) as TreeViewItem;
            currentNode = nodesStack.Pop();

            if (currentItem.Header != currentNode)
                return null;
            while (nodesStack.Count > 0)
            {
                if (currentItem.IsExpanded == false)
                {
                    currentItem.IsExpanded = true;
                    currentItem.UpdateLayout();
                }
                currentNode = nodesStack.Pop();
                foreach (Nodo innerItem in currentItem.Items)
                {
                    if (innerItem == currentNode)
                    {
                        currentItem = currentItem.ItemContainerGenerator.ContainerFromItem(innerItem) as TreeViewItem;
                        break;
                    }
                }
            }
            currentItem.IsSelected = true;
            return currentItem;
        }
        private T FindVisualChild<T>(Visual visual) where T : Visual
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(visual); i++)
            {
                Visual child = (Visual)VisualTreeHelper.GetChild(visual, i);
                if (child != null)
                {
                    T correctlyTyped = child as T;
                    if (correctlyTyped != null)
                    {
                        return correctlyTyped;
                    }

                    T descendent = FindVisualChild<T>(child);
                    if (descendent != null)
                    {
                        return descendent;
                    }
                }
            }

            return null;
        }

        //private TreeViewItem     getItembyPath(string path) {
        //    try {
        //        string[] fields = path.Split('~');
        //        object it = null;
        //        ItemCollection items = albero.Items;
        //        Nodo nd = progetti[0].Children.FirstOrDefault(x => x.Name == fields[0]);
        //        var tvi = albero.items
        //        for (int i = 1; i < fields.Length; i++) {
        //            foreach (var item in items)
        //            {
        //                Nodo n = item as Nodo;
        //                if (n.Name == fields[i])
        //                {
        //                    it = item;

        //                    break;
        //                }
        //            }
        //            //tvi = (TreeViewItem)tvi.ItemContainerGenerator.ContainerFromItem(it);
        //            //items = tvi.Items;
        //        }
        //        return (TreeViewItem)tvi;
        //    }
        //    catch(Exception e)
        //    {
        //        MessageBox.Show(e.Message);
        //        return null;
        //    }
        //}

        public static T getParentOfType<T>(DependencyObject child) where T : DependencyObject
        {
            //get parent item
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            //we've reached the end of the tree
            if (parentObject == null) return null;

            //check if the parent matches the type we're looking for
            T parent = parentObject as T;
            if (parent != null)
                return parent;
            else
                return getParentOfType<T>(parentObject);
        }


        /*FUNZIONI DI CLIPBOARD*/
        private void incolla_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (clipboardNode==null )
            {
                e.CanExecute = false;
                return;
            }
            TreeView tv = sender as TreeView;
            Nodo selected = (Nodo)tv.SelectedItem;
            if (selected.checkAncestor(clipboardNode) && !copy)
            {
                e.CanExecute = false;
                return;
            }
            Type t = tv.SelectedItem.GetType(), ctype = clipboardNode.GetType(); ;
            if (t==typeof(Variable))
            {
                if (ctype == typeof(Flusso))
                    e.CanExecute = true;
                else
                    e.CanExecute = false;
                return;
            }
            if (t==typeof(VarNodo))
            {
                if (ctype == typeof(VarNodo) || ctype == typeof(Variable))
                    e.CanExecute = true;
                else
                    e.CanExecute = false;
                return;
            }
            if (t==typeof(ComNodo))
            {
                if (ctype == typeof(ComNodo) || ctype == typeof(Canale))
                    e.CanExecute = true;
                else
                    e.CanExecute = false;
                return;
            }
            if (t==typeof(Nodo)) { e.CanExecute = false; }
        }

        private void tagliacopia_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
        private void taglia_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            TreeView tv = sender as TreeView;
            Nodo  n = tv.SelectedItem as Nodo;
            clipboardNode = n;
            copy = false;
        }
        private void copia_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            TreeView tv = sender as TreeView;
            Nodo n = tv.SelectedItem as Nodo;
            clipboardNode = n;
            copy = true;
        }
          
        private void incolla_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            TreeView tv = sender as TreeView;
            Nodo v = tv.SelectedItem as Nodo;
            if (copy)
                v.Append(clipboardNode.Clone());
            else
            {
                var n = clipboardNode;
                clipboardNode.Father.Remove(n);
                v.Append(clipboardNode);
                clipboardNode = null;
            }
        }

    }

  
}