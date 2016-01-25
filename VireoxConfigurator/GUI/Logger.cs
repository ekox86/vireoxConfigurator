using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace VireoxConfigurator
{
    /// <summary>
    /// Classe che gestisce il logging
    /// </summary>
    class Logger
    {
        delegate void logd(string message);
        delegate void logc(string message, string color);
        delegate void logn(string message, string color, Nodo n);
        static Timer timer;
        static FlowDocument fdoc;
        static logd logdeleg = Log;
        static logc logcdeleg = Log;
        static logn logndeleg = Log;
        static Paragraph paragraphLog;
        static BrushConverter brushconverter = new BrushConverter();
        static FlowDocumentScrollViewer fscroll;
        static MainWindow window;
        static int linesPerSecond;
        /// <summary>
        /// Scrive un messaggio sul riquadro di log
        /// </summary>
        /// <param name="message"></param>
        public static void Log(string message)
        {
            if (linesPerSecond > 500)
                System.Threading.Thread.Sleep(1000);
            if (fdoc.Dispatcher.CheckAccess())
                log(message);
            else
                log2(message);
            linesPerSecond++;
        }
        public static void Log(string message,string color)
        {
            if (linesPerSecond > 500)
                System.Threading.Thread.Sleep(1000);
            if (fdoc.Dispatcher.CheckAccess())
                log(message, color);
            else
                log2(message, color);
            linesPerSecond++;
        }
        
        public static void Log(string message, string color, Nodo n)
        {
            if (linesPerSecond > 500)
                System.Threading.Thread.Sleep(1000);
            if (fdoc.Dispatcher.CheckAccess())
                log(message, color, n);
            else
                log2(message, color, n);
            linesPerSecond++;
        }
         static void log (string message, string color, Nodo n)
        {
            string s = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " : " + message;
            Paragraph par = new Paragraph();
            TextBlock tblock = new TextBlock(new Run(s));
            tblock.Tag = n;
            tblock.Cursor = Cursors.Hand;
            tblock.PreviewMouseDown += link_click;
            tblock.Padding = new Thickness(0);
            tblock.FontWeight=FontWeights.Bold;
            tblock.LineHeight = 10;
            tblock.TextWrapping = TextWrapping.NoWrap;
            tblock.Margin = new Thickness(1);
            par.Margin = new Thickness(2);
            par.Loaded += paragraph_Loaded;            
            par.Inlines.Add(tblock);
            fdoc.Blocks.Add(par);
        }

        private static void link_click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            TextBlock tb = sender as TextBlock;
            window.SelectItem((Nodo)tb.Tag);            
        }

        private static void log2(string message, string color)
        {
            fdoc.Dispatcher.BeginInvoke(logcdeleg, message,color);
        }
        private static void log2(string message, string color,Nodo n)
        {
            fdoc.Dispatcher.BeginInvoke(logndeleg, message, color,n);
        }

        public static void Clear()
        {
            fdoc.Blocks.Clear();
        }
        static void log(string message)
        {
            
            string s = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " : " + message;
            Paragraph par = new Paragraph();
            TextBlock tblock = new TextBlock(new Run(s));
            par.Inlines.Add(tblock);
            par.Margin = new Thickness(2);
            tblock.Padding = new Thickness(0);
            tblock.TextWrapping = TextWrapping.NoWrap;
            tblock.Margin = new Thickness(1);
            //tblock.LineHeight = 10;
            par.Loaded += paragraph_Loaded;            
            
            fdoc.Blocks.Add(par);
       //     scrollBar.ScrollToBottom();
        }
        static void log2(string message)
        {
            fdoc.Dispatcher.BeginInvoke(logdeleg, message);
        }
        static void log(string message, string c)
        {
            string s = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " : " + message;
            Paragraph par = new Paragraph();
            TextBlock tblock = new TextBlock(new Run(s));
            par.Inlines.Add(tblock);
            par.Margin = new Thickness(2);
            tblock.Padding = new Thickness(0);
            tblock.TextWrapping = TextWrapping.NoWrap;
            tblock.Foreground=(Brush)brushconverter.ConvertFromString(c);
            tblock.Margin = new Thickness(1);
            //tblock.LineHeight = 10;
            par.Loaded += paragraph_Loaded;           
            fdoc.Blocks.Add(par);
            //scrollBar.ScrollToBottom();
        }
        public static void setDoc(FlowDocumentScrollViewer fd,MainWindow m)
        {
            fscroll = fd;
            fdoc = fd.Document;
            paragraphLog = new Paragraph();
            fdoc.Blocks.Add(paragraphLog);
            fdoc.LineHeight = 10;
            window = m;
            timer = new Timer(1000);
            timer.Elapsed += Timer_Elapsed;
            timer.AutoReset = true;
            timer.Start();
            linesPerSecond = 0;
            //fdoc.FontFamily = new FontFamily("Lucida Console");
            //fdoc.FontSize = 10;
            //fdoc.LineStackingStrategy = System.Windows.LineStackingStrategy.BlockLineHeight;
            //fdoc.LineHeight = 10;
        }

        private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            linesPerSecond = 0;
        }

        internal static FlowDocument getDoc()
        {
            return fdoc;
        }
        static void paragraph_Loaded(object sender, RoutedEventArgs e)
        {
            Paragraph paragraph = (Paragraph)sender;
            paragraph.Loaded -= paragraph_Loaded;
            paragraph.BringIntoView();
        }
    }
}
