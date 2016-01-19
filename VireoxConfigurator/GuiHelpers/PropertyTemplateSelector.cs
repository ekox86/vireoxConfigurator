using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Text.RegularExpressions;

namespace VireoxConfigurator
{
        public class PropertyTemplateSelector : DataTemplateSelector
        {
            public override DataTemplate SelectTemplate(object item, DependencyObject container)
            {
                FrameworkElement element = container as FrameworkElement;
            
                if (element != null && item != null && item is PropertyItem)
                {
                    PropertyItem property = item as PropertyItem;                    
                    if (property.choiceEnabled)
                    {
                        if (isBoolean(property.Options))
                            return element.FindResource("checkboxTemplate") as DataTemplate;
                        else
                            return element.FindResource("comboboxTemplate") as DataTemplate;
                    }
                    else
                        return  element.FindResource("textboxTemplate") as DataTemplate;
                }

                return null;
            }

        private bool isBoolean(string[] options)
        {
            if (options==null || options.Length != 2)
                return false;
            else
                return Regex.IsMatch(options[0], "false|true", RegexOptions.IgnoreCase) && Regex.IsMatch(options[1], "false|true", RegexOptions.IgnoreCase);
        }    
    }
}
