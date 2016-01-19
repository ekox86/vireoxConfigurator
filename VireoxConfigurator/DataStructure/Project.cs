using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using System.Linq.Expressions;
using System.Linq;

namespace VireoxConfigurator
{
    public class Project : Nodo
    {
        PropertyList propertyValues;
        Dictionary<string, proprietaType> defs;
        StringCollection enVarProts,enComProts;
        int firstfreegmaddress;
        [XmlIgnore]
        public int FreeGMAddress
        {
            get
            {
                return firstfreegmaddress;
            }
            set
            {
                firstfreegmaddress = value;
            }
        }
        [XmlIgnore]
        public int release = 0;
        [XmlIgnore]
        public Nodo VarTree
        {
            get { return children[1]; } 
            set { }
        }
        [XmlIgnore]
        public Nodo ComTree
        {
            get { return children[0]; }
            set {  }
        }
        [XmlIgnore]
        public Nodo UsrTree
        {
            get { return children[2]; }
            set { }
        }
        [XmlElement(ElementName = "property")]
        public ObservableCollection<PropertyItem> propertylist
        {
            get
            {
                return propertyValues.Properties;
            }
            set
            {
                propertyValues.Properties = value;
            }
        }
        [XmlIgnore]
        public IDictionary<string, string> Properties { get { return propertyValues; } }
        [XmlIgnore]
        public StringCollection EnabledComProtocols { get { return enComProts; } }
        [XmlIgnore]
        public StringCollection EnabledVarProtocols { get { return enVarProts; } }
        public Project()
        {
            defs = ProjectDefinitions.Map;
            propertyValues = new PropertyList();
            enVarProts = new StringCollection();
            enComProts = new StringCollection();
            EnabledVarProtocols.Add("Variables");
            EnabledVarProtocols.Add("Common");
        }
        public Project(bool xml)
        {
            name = "Progetto";
            children.Add(new Nodo("Comunicazione", this, true));
            children.Add(new Nodo("Memoria Globale", this, true));
            children.Add(new Nodo("Utenti",this,true));
            propertyValues = new PropertyList();
            defs = ProjectDefinitions.Map;
            enVarProts = new StringCollection();
            enComProts = new StringCollection();
            EnabledVarProtocols.Add("Variables");
            EnabledVarProtocols.Add("Common");
        }
        public void setDefaultPropertyList()
        {
            propertyValues = ProjectDefinitions.getDefaultPropertyList();
        }
        /// <summary>
        /// Aggiunge una proprietà al progetto, se esiste già la chiave non viene inserita.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <summary>
        /// Ricostruisce i riferimenti dell'albero deserializzato a partire dal file XML (rapporti padre-figlio e riferimenti alle definizioni delle proprietà)
        /// </summary>
        public override void rebuild ()
        {
            base.rebuild();
            List<proprietaType> hs = defs.Values.Where(x => x.Visibile).ToList();
            foreach (PropertyItem pi in propertylist)
            {
                if (!defs.ContainsKey(pi.Name))
                {
                    Logger.Log("Attenzione, definizione della proprietà '" + pi.Name + "' non trovata per il progetto " , "Red");
                    continue;
                }
                pi.setPropertyDef(defs[pi.Name]);
                hs.Remove(defs[pi.Name]);
            }
            if (hs.Count != 0)
            {
                foreach (proprietaType pt in hs)
                {
                    PropertyItem pnew = new PropertyItem(pt.NomeVisualizzato, pt.DefaultValue, pt);
                    propertylist.Add(pnew);
                }
            }

        }

        internal void addPropertiesGPRJ(Dictionary<string, string> props)
        {
            foreach (proprietaType pt in ProjectDefinitions.Map.Values)
            {
                if (!pt.Visibile) continue;
                string v = null;
                if (props.ContainsKey(pt.NomeSalvato))
                    v = props[pt.NomeSalvato];
                propertylist.Add(new PropertyItem(pt.NomeVisualizzato, v, pt));               
            }
            updateEnabledProtocolList();
        }

        //public  void linkChannels()
        //{
        //    List<Canale> cni = new List<Canale>();
        //    comTree.getLeaves(cni);
        //    foreach(Canale c in cni)
        //    {
        //        canali.Add(c.ProtocolName+c.Name, c);
        //    }
        //    List<Flusso> flussi=new List<Flusso>();
        //    VarTree.getLeaves(flussi);
        //    foreach(Flusso f in flussi)
        //    {
        //        f.linkChannel(canali);
        //    }
        //}
        public void updateEnabledProtocolList()
        {
            enComProts.Clear();
            enVarProts.Clear();
            enVarProts.Add("Variables");
            enVarProts.Add("Common");
            foreach (var pr in VarDefinitions.Map)
            {
                string prName = pr.Key + " Enabled";
                if (Properties.ContainsKey(prName))
                {
                    if (!String.IsNullOrWhiteSpace(Properties[prName]) && Properties[prName].ToLower().Trim() == "true")
                    {
                        enVarProts.Add(pr.Key);
                        enComProts.Add(pr.Key);
                    }
                }
            }
        }
        public void setFreeGMAddress()
        {
            int max = 0;
            List<Variable> lst = new List<Variable>();
            getVariables(lst);
            foreach (Variable v in lst)
            {
                if (v.Address > max)
                    max = v.Address;
            }
            firstfreegmaddress = max + 1;
        }
    }

    [Serializable]
    class ProjectException : Exception
    {
        public ProjectException() { }

        public ProjectException(string message) :base(message) { }

        public ProjectException(string message, Exception inner) : base(message,inner) { }
    }

   
}