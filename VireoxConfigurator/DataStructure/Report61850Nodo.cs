using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
namespace VireoxConfigurator
{
        public class Report61850Nodo : Nodo
        {
            public Report61850Nodo()
            {
            }
            public Report61850Nodo(string name, Nodo father, bool abilitazione) : base(name, father, abilitazione)
            {
            }
        }
    
}
