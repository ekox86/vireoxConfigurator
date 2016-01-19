using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Serialization;
using System.Windows.Documents;
using System.Xml;
using System.Net;

namespace VireoxConfigurator
{
    public class StartupClass
    {
        //[System.STAThreadAttribute()]
        //[System.Diagnostics.DebuggerNonUserCodeAttribute()]
        //[System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        //public static void Main()
        //{
        //    //Logger.setDoc(new FlowDocument());
        //    loadStaticDefinitions();
        //    VireoxConfigurator.App app = new VireoxConfigurator.App();
        //    app.InitializeComponent();
        //    app.Run();

        //}

        public static void loadStaticDefinitions()
        {
            try {
                XmlSerializer xSer = new XmlSerializer(typeof(VireoxConfigurator.root));
                //root definitions = getRemoteDefinitions(xSer,"http://93.62.113.78/StaticDefinitions.xml");                
                root definitions = (root)xSer.Deserialize(new StreamReader(".\\Resources\\StaticDefinitions.xml"));
                ComDefinitions.load(definitions);
                VarDefinitions.load(definitions);
                ProjectDefinitions.load(definitions);
            }
            catch (Exception e)
            {
                Logger.Log("Errore durante caricamento del file delle definizioni: " + e.Message,"Red");
                Logger.Log("L'applicazione non funzionerà correttamente. " + e.Message, "Red");
            }
        }
        static root getRemoteDefinitions(XmlSerializer xser,string url)
        {
            try {
                WebClient client = new WebClient();                
                Stream stream = client.OpenRead("http://93.62.113.78/StaticDefinitions.xml");
                root definitions = (root)xser.Deserialize(stream);
                stream.Close();               
                return definitions;
            }
            catch(Exception e)
            {
                return null;
            }
        }

    }
}
