using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.ObjectModel;
using System.Xml.Serialization;
using System.ComponentModel;

namespace VireoxConfigurator
{
    public class Nodo : INotifyPropertyChanged

    {
        protected string name;
        protected Nodo father;
        protected ObservableCollection<Nodo> children;
        protected bool abilitazione;
        public event PropertyChangedEventHandler PropertyChanged;

        [XmlIgnore]
        public int Depth
        {
            get
            {
                int n = 0;
                Nodo f = this;
                while (f.father!=null&& !Regex.IsMatch(f.father.name,"Memoria Globale|Comunicazione"))
                {
                    n++;
                    f = f.father;
                }
                return n;
            }
        }
        [XmlIgnore]
        public NodoView ViewNode
        {
            get
            {
                return new NodoView(this);
            }
        }
        [XmlIgnore]
        public virtual string Path
        {
            get
            {
                string v = name;
                Nodo f = father;
                while (f.father != null)
                {
                    v = f.Name + "~" + v;
                    f = f.Father;
                }
                return v;
            }
        }
        [XmlAttribute]
        public virtual string Name
        {
            get
            {
                return name;
            }
            set {
                name = value;
                OnPropertyChanged("Name");
            }
        }
        [XmlAttribute]
        public virtual bool Enabled { get { return abilitazione; } set { abilitazione = value; OnPropertyChanged("Enabled"); } }
        /// <summary>
        /// Restituisce il nodo padre.
        /// </summary>
        [XmlIgnore]
        public Nodo Father
        {
            get { return father; }
            set { father = value; }
        }
        /// <summary>
        /// Restituisce i nodi figli. Nel caso sia una Variable, i figli sono tutti dei Flussi. Nel caso di Nodo, possono essere Nodo, Variabile,Flusso, Canale.
        /// </summary>
        [XmlElement(Type = typeof(Nodo)), XmlElement(Type = typeof(Variable)), XmlElement(Type = typeof(Foglia)), XmlElement(Type = typeof(Flusso)), XmlElement(Type = typeof(Canale)), XmlElement(Type = typeof(VarNodo)), XmlElement(Type = typeof(ComNodo))]
        public virtual ObservableCollection<Nodo> Children
        {
            get { return children; }
            set { children = value; OnPropertyChanged("Children"); }
        }
        public Nodo() {
            name = "Nuovo Nodo";
            children = new ObservableCollection<Nodo>();
            abilitazione = true;
        }
        public Nodo(string name, Nodo father, bool abilitazione)
        {
            this.name = name;
            this.father = father;
            this.abilitazione = abilitazione;
            children = new ObservableCollection<Nodo>();
        }

        internal void Append(ObservableCollection<Nodo> nodes)
        {
            foreach (var child in nodes)
            {
                Children.Add(child);
            }
        }

        /// <summary>
        /// Carica l'albero nei figli di questo nodo, a partire da un file CSV.
        /// </summary>
        /// <param name="fileName">File CSV del quale viene effettuata l'importazione dell'albero</param>
        public void loadTreefromCSVFile(string fileName)
        {
            StreamReader sr = new StreamReader(fileName);
            List<List<string>> groupMap = new List<List<string>>();
            Father.Remove(this);
            //Father.Children = new ObservableCollection<Nodo>();
            try
            {
                string[] intestazioni = sr.ReadLine().Split('#');
                List<string> gruppi = new List<string>();
                foreach (string i in intestazioni)
                {
                    //gruppi.Add(i.Substring(0, i.IndexOf(';')));
                    List<string> l = new List<string>();
                    l.Add(i.Split(';').ElementAt(1));
                    groupMap.Add(l);
                }
                intestazioni = sr.ReadLine().Split(new string[] { ";#;" }, StringSplitOptions.None);
                for (int j = 0; j < intestazioni.Length; j++)
                {
                    groupMap[j].AddRange(intestazioni[j].Split(';').ToList());
                }
                int nIndent = 0;
                foreach (string s in groupMap[0])       //conto il numero di indentazioni
                {
                    if (Regex.IsMatch(s, "Nome Variabile"))
                        nIndent++;
                }
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    List<string> groups = line.Split(new string[] { ";#;" }, StringSplitOptions.None).ToList();
                    Variable var = Father.addVarCSV(groups[0], nIndent);
                    var.addFluxes(groups.GetRange(1, groups.Count - 1), groupMap.GetRange(1, groupMap.Count - 1));
                }
            }
            catch (Exception e)
            {
                Logger.Log("Errore di importazione CSV: " + e.Message, "Red");
            }
            finally
            {
                sr.Close();
            }
        }

        internal void addVarNodeGPRJ(string path, Dictionary<string, string> props)
        {
            string[] pathFields = path.Split(';');
            Nodo actualNode = this, nextNode = null;
            for (int i = 0; i < pathFields.Length; i++)
            {
                nextNode = actualNode.Children.FirstOrDefault(x => x.name == pathFields[i]);
                if (nextNode != null)
                {
                    actualNode = nextNode;
                    continue;
                }
                else if (i != pathFields.Length - 1)
                {
                    actualNode.Append(nextNode = new VarNodo(pathFields[i], actualNode, true));
                }
                else {
                    switch (props["Tipo"])
                    {
                        case "Nodo":
                            actualNode.Append(nextNode = new VarNodo(pathFields[i], actualNode, Boolean.Parse(props["Enabled"].ToLower())));
                            break;
                        case "Variabile":
                            actualNode.Append(nextNode = new Variable(pathFields[i], actualNode, Boolean.Parse(props["Enabled"]), Int32.Parse(props["Addres"])));
                            break;
                        case "Protocollo":
                            actualNode.Append(nextNode = new Flusso(pathFields[i], actualNode, Boolean.Parse(props["Enabled"])));
                            Flusso f = nextNode as Flusso;
                            f.ProtocolName = props["SottoTipo"];
                            if (f.ProtocolDefs == null)
                            {
                                var pname = VarDefinitions.Map.Keys.FirstOrDefault(x => Regex.IsMatch(props["SottoTipo"], x, RegexOptions.IgnoreCase));
                                if (pname != null)
                                    f.ProtocolName = pname;
                                else
                                {
                                    Logger.Log("Protocollo non trovato per il flusso " + f.Path, "Red");
                                    continue;
                                }
                            }
                            foreach (var pt in f.ProtocolDefs)
                            {
                                if (!pt.Value.Visibile) continue;
                                string v = null;
                                if (props.ContainsKey(pt.Value.NomeSalvato))
                                    v = props[pt.Value.NomeSalvato];
                                f.propertylist.Add(new PropertyItem(pt.Value.NomeVisualizzato, v, pt.Value));
                            }
                            break;
                    }
                }
                actualNode = nextNode;
            }
        }

        internal void addComNodeGPRJ(string path, Dictionary<string, string> props)
        {
            string[] pathFields = path.Split(';');
            Nodo actualNode = this, nextNode = null;
            for (int i = 0; i < pathFields.Length; i++)
            {
                nextNode = actualNode.Children.FirstOrDefault(x => x.name == pathFields[i]);
                if (nextNode != null)
                {
                    actualNode = nextNode;
                    continue;
                }
                else if (i != pathFields.Length - 1)
                {
                    actualNode.Append(nextNode = new ComNodo(pathFields[i], actualNode, true));
                }
                else
                {
                    switch (props["Tipo"])
                    {
                        case "Nodo":
                            actualNode.Append(nextNode = new ComNodo(pathFields[i], actualNode, Boolean.Parse(props["Enabled"].ToLower())));
                            break;
                        case "Comunication":
                            actualNode.Append(nextNode = new Canale(pathFields[i], actualNode, Boolean.Parse(props["Enabled"].ToLower())));
                            Canale c = nextNode as Canale;
                            c.ProtocolName = props["SottoTipo"];
                            if (c.ProtocolDefs == null)
                            {
                                var pname = ComDefinitions.Map.Keys.FirstOrDefault(x => Regex.IsMatch(props["SottoTipo"], x, RegexOptions.IgnoreCase));
                                if (pname != null)
                                    c.ProtocolName = pname;
                                else
                                {
                                    Logger.Log("Protocollo non trovato per il canale " + c.Path, "Red");
                                    continue;
                                }
                            }
                            foreach (var pt in c.ProtocolDefs)
                            {
                                if (!pt.Value.Visibile) continue;
                                //if (Regex.IsMatch(pt.Value.name, "SottoTipo|Tipo|NameNodo|Enabled|PathNode|Addres|GPMExportFile|Percorso|^a$", RegexOptions.IgnoreCase)) continue;
                                string v = null;
                                if (props.ContainsKey(pt.Value.NomeSalvato))
                                    v = props[pt.Value.NomeSalvato];
                                c.propertylist.Add(new PropertyItem(pt.Value.NomeVisualizzato, v, pt.Value));
                            }
                            break;
                    }
                }
                actualNode = nextNode;
            }

        }
        /// <summary>
        /// Aggiunge una variabile al nodo a partire dalla stringa path e dal numero di indentazioni
        /// </summary>
        /// <param name="pathString">Stringa CSV contenente il path, GMAddress e abilitazione</param>
        /// <param name="nIndent">Numero di indentazioni della variabile</param>
        /// <returns></returns>
        public Variable addVarCSV(string pathString, int nIndent)
        {
            Nodo n = this, m = null;
            string[] fields = pathString.Split(';');
            int GMAddress = Int32.Parse(fields[nIndent]);
            bool abilitazione = Boolean.Parse(fields[nIndent + 1]);
            for (int i = 0; i < nIndent; i++)
            {
                m = n.Children.FirstOrDefault(x => x.Name == fields[i]); //cerco tra i figli del nodo
                if (m == null)    //se il nodo non è tra i figli lo aggiungo
                {
                    if (i != nIndent - 1 && !String.IsNullOrWhiteSpace(fields[i+1]))   //se non è tra i figli ma non è all'ultimo livello d'indentazione lo aggiungo e scorro al successivo.
                    {
                        m = new VarNodo(fields[i], n, abilitazione);
                        n.Children.Add(m);
                        n = m;
                    }
                    else
                    {
                        m = new Variable(fields[i], n, abilitazione, GMAddress);
                        n.Children.Add(m);
                        return (Variable)m;
                    }
                }
                else                   //Se il figlio c'è già scorro al livello successivo d'indentazione
                {
                    n = m;
                }
            }
            Logger.Log("Errore: variabile duplicata nel file CSV: " + pathString,"Red");
            return null;
        }
        internal void addComCSV(List<string> groups, List<List<string>> groupMap)
        {
            Nodo n = this, m = null;
            string[] path = groups[0].Split(';');
            string fieldString = "";
            for (int i = 0; i < path.Length; i++)
            {
                m = n.Children.FirstOrDefault(x => x.Name == path[i]);
                if (m == null)
                {
                    if (i == path.Length - 1 || String.IsNullOrWhiteSpace(path[i + 1]))
                    {
                        int c = 1;
                        for (; c < groups.Count; c++)
                        {
                            if (!String.IsNullOrWhiteSpace(groups[c].Replace(";", "")))
                            {
                                fieldString = groups[c];
                                break;
                            }
                        }
                        m = new Canale(path[i], n, fieldString.Split(';'), groupMap[c]);
                        n.Children.Add(m);
                        return;
                    }
                    else
                    {
                        m = new ComNodo(path[i], n, true);
                        n.Append(m);
                        n = m;
                    }
                }
                else
                {
                    n = m;
                }
            }
        }
        /// <summary>
        /// Funzione ricorsiva che ricostruisce i riferimenti dei figli del nodo al padre, serve per ricostruire i path
        /// </summary>
        public virtual void rebuild()
        {
            if (children == null || children.Count == 0) return;
            foreach (Nodo n in children)
            {
                n.Father = this;
                n.rebuild();
            }

        }
        /// <summary>
        /// Restituisce tutte le foglie in fondo a questo ramo dell'albero.
        /// </summary>
        /// <param name="lst">Lista nella quale vengono salvate le foglie</param>
        public virtual void getLeaves(List<Foglia> lst) {
            if (children != null && children.Count > 0)
            {
                foreach (var child in children)
                {
                    child.getLeaves(lst);
                }
            }
        }
        public virtual void getLeaves(List<Foglia> lst, bool checkEnabled)
        {
            if (checkEnabled && !Enabled) return;
            if (children != null && children.Count > 0)
            {
                foreach (var child in children)
                {
                    child.getLeaves(lst, checkEnabled);
                }
            }
        }
        /// <summary>
        /// Restituisce tutte le foglie di un certo protocollo in fondo a questo ramo dell'albero.
        /// </summary>
        /// <param name="lst">Lista nella quale vengono salvate le foglie</param>
        /// <param name="protocolName">Nome del protocollo del quale recuperare le foglie.</param>
        public virtual void getProtocolLeaves(List<Foglia> lst, string protocolName)
        {
            getLeaves(lst);
        }
        /// <summary>
        /// Restituisce tutte le variabili in fondo a questo ramo dell'albero.
        /// </summary>
        /// <param name="lst">Lista nella quale vengono salvate le variabili</param>
        public virtual void getVariables(List<Variable> lst, bool checkEnabled = false)
        {
            if (checkEnabled && !Enabled) return;
            if (children != null && children.Count > 0)
            {
                foreach (var child in children)
                {
                    child.getVariables(lst, checkEnabled);
                }
            }
        }
        public override string ToString()
        {
            return Name;
        }
        /// <summary>
        /// Aggiunge un figlio al nodo
        /// </summary>
        /// <param name="n"></param>
        public virtual void Append(Nodo n)
        {
            children.Add(n);
            n.father = this;
        }

        protected void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        internal void Toggle()
        {
            Enabled = Enabled ? false : true;
        }

        public void Remove(Nodo n)
        {
            children.Remove(n);
        }

        public virtual int getMaxDepth()
        {
            int m = 0, d;
            foreach (Nodo c in children)
            {
                d = c.getMaxDepth() + 1;
                if (d > m)
                    m = d;
            }
            return m;
        }

        public virtual int countLeaves()
        {
            int leaves=0;
            foreach (var c in children)
            {
                leaves += c.countLeaves();
            }
            return leaves;
        }
        public virtual int countVariables()
        {
            int vars = 0;
            foreach (var c in children)
            {
                vars += c.countVariables();
            }
            return vars;
        }

        public virtual void exportCSV(string file) { throw new NotImplementedException(); }

        public virtual Nodo Clone()
        {
            Nodo n = new Nodo(name, null, abilitazione);
            foreach (Nodo child in children)
            {
                n.Append((Nodo)(child.Clone()));
            }
            n.rebuild();
            return n;
        }

        public bool checkAncestor(Nodo n)
        {
            Nodo f = this;
            while (f!=null)
            {
                if (f == n) return true;
                f = f.father;
            }
            return false;
        }

        public int getLevel()
        {
            Nodo n = this;
            int c = 0;
            while (n!=null)
            {
                n = n.Father;
                c++;
            }
            return c;
        }
    }



}
