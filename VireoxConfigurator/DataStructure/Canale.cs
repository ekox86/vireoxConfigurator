using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace VireoxConfigurator
{
    public class Canale : Foglia
    {
        [XmlAttribute]
        public override string ProtocolName
        {
            get { return protocolname; }
            set
            {
                protocolname = value;
                if (ComDefinitions.Map.ContainsKey(protocolname))
                {
                    propertyDefinitions = ComDefinitions.Map[protocolname];                    
                }
            }
        }
        public override void setDefaultPropertyList()
        {
            propertyValues = propertyDefinitions.getDefaultPropertyList();
        }
        public Canale() {  }

        public Canale(string name, Nodo father, bool abilitazione) : base(name, father, abilitazione)
        {
        }

        public Canale(string name,Nodo father, string[] fields, List<string> map)
        {
            this.father = father;
            protocolname = map[0];
            propertyDefinitions = ComDefinitions.Map[map[0]];
            propertyValues = new PropertyList();
            this.name = name;
            foreach (var pt in propertyDefinitions)
            {
                if (!pt.Value.Visibile) continue;
                int i = map.FindIndex(x => x == pt.Key);
                if (pt.Key == "Abilitato")
                {
                    if (!Boolean.TryParse(fields[i - 1], out abilitazione)) abilitazione = false;
                }
                if (i > 0)
                {
                    propertyValues.Add(pt.Key, (fields[i - 1]).Replace("%sc%",";"), pt.Value);
                }
                else
                    propertyValues.Add(pt.Key, "", pt.Value);
            }
        
        }

        public override Nodo Clone()
        {
            Canale f = new Canale(name, null, true);
            f.propertyDefinitions = propertyDefinitions;
            f.protocolname = protocolname;
            f.abilitazione = abilitazione;
            f.propertyValues = (PropertyList)propertyValues.Clone();
            return f;
        }
    }
}
