using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace VireoxConfigurator
{
    public class Foglia : Nodo
    {

        protected PropertyList propertyValues;
        protected Protocol propertyDefinitions;
        protected string protocolname;
        [XmlAttribute]
        public virtual string ProtocolName
        {
            get { return protocolname; }
            set
            {
                protocolname = value;
                OnPropertyChanged("ProtocolName");
            }
        }
        [XmlIgnore]
        public Protocol ProtocolDefs
        {
            get { return propertyDefinitions; }
        }
        [XmlIgnore]
        public IDictionary<string, string> Properties { get { return propertyValues; } }
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
                OnPropertyChanged("propertylist");
            }
        }
        public string this[string index]
        {
            get
            {
                if (!propertyValues.ContainsKey(index))
                    return null;
                else
                    return propertyValues[index];
            }

            set
            {
                propertyValues[index] = value;
            }
        }
        public Foglia()
        {
            propertyValues = new PropertyList();
        }
        public Foglia(string name, Nodo father, bool abilitazione) : base(name, father, abilitazione) {
            propertyValues = new PropertyList();
        }
        public override void getLeaves(List<Foglia> lst)
        {
            lst.Add(this);
        }
        public override void getLeaves(List<Foglia> lst,bool checkEnabled)
        {
            if (checkEnabled && !Enabled) return;
            lst.Add(this);
        }
        public override void getProtocolLeaves(List<Foglia> lst, string protocolName)
        {
            if (protocolName == this.protocolname)
                lst.Add(this);
        }
        public override void rebuild()
        {
            List<proprietaType> hs = propertyDefinitions.Values.Where(x => x.Visibile).ToList();
            foreach (PropertyItem pi in propertylist)
            {
                if (!propertyDefinitions.ContainsKey(pi.Name))
                {
                    Logger.Log("Attenzione, definizione della proprietà '" + pi.Name + "' non trovata per il protocollo " + protocolname, "Red");
                    continue;
                }
                pi.setPropertyDef(propertyDefinitions[pi.Name]);
                hs.Remove(propertyDefinitions[pi.Name]);
            }
            if (hs.Count!=0)
            {
                foreach (proprietaType pt in hs) {
                    PropertyItem pnew = new PropertyItem(pt.NomeVisualizzato, pt.DefaultValue, pt);
                    propertylist.Add(pnew);
                 }
            }
        }        
        public virtual void setDefaultPropertyList() { }

        public override int getMaxDepth()
        {
            return 1;
        }
        public override int countLeaves()
        {
            return 1;
        }
        public override int countVariables()
        {
            throw new Exception("Errore: funzione countVariables() chiamata da un nodo foglia");
        }

        public override Nodo Clone()
        {            
            Foglia f = new Foglia(name, null, true);
            f.propertyDefinitions = propertyDefinitions;
            f.protocolname = protocolname;
            f.abilitazione = abilitazione;
            f.propertyValues = (PropertyList)propertyValues.Clone();
            return f;
        }
    }

}