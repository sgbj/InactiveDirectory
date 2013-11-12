using Awesomium.Core;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace InactiveDirectory
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void webControl_DocumentReady(object sender, Awesomium.Core.UrlEventArgs e)
        {
            using (dynamic app = (JSObject)webControl.CreateGlobalJavascriptObject("app"))
            {
                app.minimize = (JavascriptAsynchMethodEventHandler)minimize;
                app.maximize = (JavascriptAsynchMethodEventHandler)maximize;
                app.close = (JavascriptAsynchMethodEventHandler)close;
                app.getEntries = (JavascriptMethodEventHandler)getEntries;
            }
            webControl.ExecuteJavascript("loadEntries()");
        }

        private void webControl_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(this);
            var menuLeft = (int)webControl.ExecuteJavascriptWithResult("$('#menuLeft').width()");
            var menuRight = (int)webControl.ExecuteJavascriptWithResult("$('#menuRight').width()");
            var toolbar = (int)webControl.ExecuteJavascriptWithResult("$('#toolbar').height()");
            if (pos.X >= menuLeft && pos.X <= Width - menuRight && pos.Y < toolbar)
            {
                DragMove();
                e.Handled = true;
            }
        }

        private void minimize(object sender, JavascriptMethodEventArgs e)
        {
            WindowState = System.Windows.WindowState.Minimized;
        }

        private void maximize(object sender, JavascriptMethodEventArgs e)
        {
            webControl.ExecuteJavascript("$('#maximize').toggleClass('fa-expand fa-compress')");
            WindowState = WindowState == System.Windows.WindowState.Maximized ? System.Windows.WindowState.Normal : System.Windows.WindowState.Maximized;
        }

        private void close(object sender, JavascriptMethodEventArgs e)
        {
            Close();
        }

        private void getEntries(object sender, JavascriptMethodEventArgs e)
        {
            try
            {
                using (var dir = new DirectoryEntry((string)e.Arguments[0]))
                {
                    e.Result = new JSValue(dir.Children.OfType<DirectoryEntry>().Select(de => 
                    {
                        dynamic entry = new JSObject();
                        //entry.count = de.Children.OfType<DirectoryEntry>().Count();
                        entry.count = 1;
                        entry.name = de.Name;
                        return new JSValue((JSObject)entry);
                    }).ToArray());
                }
            }
            catch
            {
                e.Result = new JSValue();
            }
        }
    }
}
