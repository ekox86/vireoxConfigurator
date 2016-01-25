using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
namespace VireoxConfigurator

{
    
    public class Report61850 : Foglia
    {
        [XmlAttribute]
        public override string ProtocolName
        {
            get { return protocolname; }
            set
            {
                protocolname = value;
                if (VarDefinitions.Map.ContainsKey(protocolname))
                {
                    propertyDefinitions = VarDefinitions.Map[protocolname];
                }
                OnPropertyChanged("ProtocolName");
            }
        }
        public Report61850() 
        {
            
        }
        public override void setDefaultPropertyList()
        {
            propertyValues = propertyDefinitions.getDefaultPropertyList();
        }
        public Report61850(string name, Nodo father, bool abilitazione) : base(name, father, abilitazione)
        {

        }
 
    }
}