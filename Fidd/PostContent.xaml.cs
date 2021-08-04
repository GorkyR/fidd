using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Reflection.Metadata;
using System.Text;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Fidd
{
    /// <summary>
    /// Interaction logic for PostContent.xaml
    /// </summary>
    public partial class PostContent : UserControl
    {
        const int max_width = 800;
        const int font_size = 20;
        static string css_style = @$"
            div.post-title, div.post-body {{ max-width: {max_width}px; margin: 0 auto; }}
            div.post-body {{ word-break: break-word; }}
            body {{ font-family: ""Segoe UI"", Verdana, Calibri, sans-serif; font-size: {font_size}px; margin: 30px; color: #222 }}
            pre, code {{ font-family: Fira Code, monospace; font-size: {font_size * 0.7}px; color: #333; background-color: #eee; }}
            pre {{ padding: 8px; border-radius: 8px; white-space: pre-wrap; border: 0.5px solid rgba(0, 0, 0, .125); }}
            a, a:visited {{ color: blue; text-decoration: none; }}
            div.post-title > a {{ color: #222; }}
            div.post-title > a:hover {{ color: blue;  }}
            div.post-title > a > h1 {{ font-size: {font_size * 1.8}px; font-weight: 800; }}

            div.post-body h1 {{ font-size: {font_size * 1.6 }px; }}
            div.post-body h2 {{ font-size: {font_size * 1.4 }px; }}
            div.post-body h3 {{ font-size: {font_size * 1.25}px; }}

            img, figure {{ max-width: 100%; display: block; border-radius: 5px; margin: 0 auto; }}
            img:only-child {{ display: block; margin: 0 auto; }}
            p > img {{ display: inline-block; }}
            figcaption {{ font-size: {font_size * 0.75}px !important; text-alignment: center !important;  }}

            blockquote {{ border-left: 1px solid #777777; padding-left: 16px; margin: 0; color: #444; }}
            li {{ margin-bottom: 16px; }}

            div.post-body div {{ padding-bottom: 4px !important; }}

            table {{ width: 100%; max-width: 100%; margin: auto; border-collapse: collapse; }}
            td, th {{ padding: 8px; border: .5px solid rgba(0, 0, 0, .125); }}

            /*
            table {{ display: block; max-width: 100% !important; border-collapse: collapse; margin-bottom: 16px; table-layout: fixed; }}
            table tbody, table div {{ display: block; max-width: 100%; }}
            table, th, td {{ border: 0px solid transparent; }}
            table td > * {{ max-width: 100%; }}
            /* table tr {{ display: flex; flex-wrap: wrap; }} /**
            /* table tr > * {{ flex: 1 1 350px; }}; /**

            table tr > td {{ display: inline-block; margin: 0 auto; }}
            table tr > td:nth-last-child(2), table tr > td:nth-last-child(2) ~ * {{ width: 50%;   max-width: 50%;   }}
            table tr > td:nth-last-child(3), table tr > td:nth-last-child(3) ~ * {{ width: 33.3%; max-width: 33.3%; }}
            table tr > td:nth-last-child(4), table tr > td:nth-last-child(4) ~ * {{ width: 25%;   max-width: 25%;   }}/**/
    	";

        Feed.Post _post = null;
        public Feed.Post Post
        {
            get => _post;
            set
            {
                string content = "<html></html>";
                if (value != null)
                {
                    content = @$"
                        <!doctype html>
                        <html>
                            <head>
                                <meta charset=""UTF-8"">
                                <meta http-equiv=""X-UA-Compatible"" content=""IE=11""/>
                                <style type=""text/css"">{css_style}</style>
                              </head>
                            <body>
                                <div class=""post-title""><a href=""{value.Link}""><h1>{value.Title}</h1></a></div>
                                <div class=""post-body"">{App.FeedManager.LoadPostContent(value)}</div>
                            </body>
                        </html>";
                }
                WebContent.NavigateToString(content);
                _post = value;
            }
        }

        public PostContent()
        {
            InitializeComponent();
            SetBrowserFeatureControl();
        }

        private void Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if (e.Uri != null)
            {
                e.Cancel = true;
                var url = e.Uri.ToString();
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url.Replace("&", "^&")}") { CreateNoWindow = true });
            }
        }

        /// https://stackoverflow.com/a/18333982/14410293
        private void SetBrowserFeatureControl()
        {
            /// http://msdn.microsoft.com/en-us/library/ee330720(v=vs.85).aspx

            // FeatureControl settings are per-process
            var fileName = System.IO.Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName);

            // make the control is not running inside Visual Studio Designer
            if (String.Compare(fileName, "devenv.exe", true) == 0 || String.Compare(fileName, "XDesProc.exe", true) == 0)
                return;

            SetBrowserFeatureControlKey("FEATURE_BROWSER_EMULATION", fileName, GetBrowserEmulationMode()); // Webpages containing standards-based !DOCTYPE directives are displayed in IE10 Standards mode.
            SetBrowserFeatureControlKey("FEATURE_AJAX_CONNECTIONEVENTS", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_ENABLE_CLIPCHILDREN_OPTIMIZATION", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_MANAGE_SCRIPT_CIRCULAR_REFS", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_DOMSTORAGE ", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_GPU_RENDERING ", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_IVIEWOBJECTDRAW_DMLT9_WITH_GDI  ", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_DISABLE_LEGACY_COMPRESSION", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_LOCALMACHINE_LOCKDOWN", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_BLOCK_LMZ_OBJECT", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_BLOCK_LMZ_SCRIPT", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_DISABLE_NAVIGATION_SOUNDS", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_SCRIPTURL_MITIGATION", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_SPELLCHECKING", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_STATUS_BAR_THROTTLING", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_TABBED_BROWSING", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_VALIDATE_NAVIGATE_URL", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_WEBOC_DOCUMENT_ZOOM", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_WEBOC_POPUPMANAGEMENT", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_WEBOC_MOVESIZECHILD", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_ADDON_MANAGEMENT", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_WEBSOCKET", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_WINDOW_RESTRICTIONS ", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_XMLHTTP", fileName, 1);
        }
        private void SetBrowserFeatureControlKey(string feature, string appName, uint value)
        {
            using (var key = Registry.CurrentUser.CreateSubKey(
                String.Concat(@"Software\Microsoft\Internet Explorer\Main\FeatureControl\", feature),
                RegistryKeyPermissionCheck.ReadWriteSubTree))
            {
                key.SetValue(appName, (UInt32)value, RegistryValueKind.DWord);
            }
        }
        private UInt32 GetBrowserEmulationMode()
        {
            int browserVersion = 7;
            using (var ieKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Internet Explorer",
                RegistryKeyPermissionCheck.ReadSubTree,
                System.Security.AccessControl.RegistryRights.QueryValues))
            {
                var version = ieKey.GetValue("svcVersion");
                if (null == version)
                {
                    version = ieKey.GetValue("Version");
                    if (null == version)
                        throw new ApplicationException("Microsoft Internet Explorer is required!");
                }
                int.TryParse(version.ToString().Split('.')[0], out browserVersion);
            }

            UInt32 mode = 11000; // Internet Explorer 11. Webpages containing standards-based !DOCTYPE directives are displayed in IE11 Standards mode. Default value for Internet Explorer 11.
            switch (browserVersion)
            {
                case 7:
                    mode = 7000; // Webpages containing standards-based !DOCTYPE directives are displayed in IE7 Standards mode. Default value for applications hosting the WebBrowser Control.
                    break;
                case 8:
                    mode = 8000; // Webpages containing standards-based !DOCTYPE directives are displayed in IE8 mode. Default value for Internet Explorer 8
                    break;
                case 9:
                    mode = 9000; // Internet Explorer 9. Webpages containing standards-based !DOCTYPE directives are displayed in IE9 mode. Default value for Internet Explorer 9.
                    break;
                case 10:
                    mode = 10000; // Internet Explorer 10. Webpages containing standards-based !DOCTYPE directives are displayed in IE10 mode. Default value for Internet Explorer 10.
                    break;
                default:
                    // use IE11 mode by default
                    break;
            }

            return mode;
        }
    }
}
