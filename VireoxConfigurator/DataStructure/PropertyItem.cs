using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace VireoxConfigurator
{
    public class PropertyItem : INotifyPropertyChanged
    {
        protected string name, value;
        protected proprietaType pdef;
        private PropertyItem pi;

        public event PropertyChangedEventHandler PropertyChanged;

        [XmlAttribute]
        public string Name { get { return name; } set { name = value; } }
        [XmlAttribute]
        public virtual string Value { get { return value; } set {
                int n = 0;
                if (value!=null)
                    n = value.IndexOf("---");
                if (n > 0)
                    this.value = value.Substring(0, n);
                else
                    this.value = value;
                OnPropertyChanged("Value");
            }
        }

        [XmlIgnore]
        public string[] Options { get { return pdef != null ? pdef.parametro : null; } }

        [XmlIgnore]
        public bool choiceEnabled { get { return pdef != null ? pdef.TipoDefinito : false; } }

        public PropertyItem() { }
        public PropertyItem(string name, string value)
        {
            this.name = name;
            this.value = value;
        }

        public PropertyItem(string name, string value, proprietaType pt)
        {
            this.name = name;
            this.Value = value;
            pdef = pt;
        }

        public PropertyItem(PropertyItem pi)
        {
            name = pi.name;
            value = pi.value;
            pdef = pi.pdef;
        }

        public void setPropertyDef(proprietaType pt)
        {
            pdef = pt;
        }
        protected void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));

        }

    }
}
