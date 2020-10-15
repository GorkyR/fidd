using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
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
        /*string CSS = @"
* { box-sizing: border-box; max-width: 100%; }

body { font: 18px/28px body, sans-serif; }
pre, code { font-family: mono, monospace; }

body {
  color: #000;
  font-feature-settings: ""kern"" 1,""liga"" 1,""calt"" 1;
  text-rendering: optimizeLegibility;
  margin: 0 auto;
}
.post { width: 544px !important; margin: 0px auto; }

a { color: inherit; text-decoration: none; border-bottom: 2px solid rgba(0, 0, 0, 0.2); }
a: hover { border-color: currentColor; }
p, blockquote { margin: 15px 0; }
h1 + p + blockquote { margin-bottom: 30px; margin-right: 1em; }
blockquote { padding-left: 1em; color: rgba(0, 0, 0, 0.55); }
blockquote::before { content: ""> ""; float: left; margin: 0 0 0 -1em; }

.quote-author { text-align: right; font-size: 15px; }
strong { font-weight: 700; }
h1, h2 { margin: 2.5em 0 0.5em; }
h1 { font-size: 1.7em; }
h2 { font-size: 1.4em; }
h1 + h2 { margin: -0.75em 0 0.9em; }
.title { font-size: 2.5em; line-height: 50px; margin: 1.5em 0 0.75em 0; }

p > img, .fig, figure { margin: 2em 0; }
img, figure { max-width: 100% !important; }

.fig, figure { text-align: center; font-size: 12px; line-height: 20px; font-style: italic; }
@media(min-width: 600px) {
  .fig, figure { width: 600px; margin-left: -28px; margin-right: -28px; }
}
.fig img, figure > img, figure > video, figure > a > img { margin: 0 auto 1em; display: block; border-radius: 3px; }
figure > video { max-width: 100%; }
.label { text-align: center; font-size: 12px; font-style: italic; margin: -1em 0 1em 0; }

code { font-style: normal; background: rgba(0, 0, 0, 0.06); padding: 2px 6px; border-radius: 4px; font-size: 17px; }
pre {
    font-size: 16px;
    background: rgba(0, 0, 0, 0.06);
    padding: 16px 30px 14px;
    margin: 1em -30px;
    border-radius: 8px;
    white-space: pre-wrap;
    word-wrap: break-word;
    font-style: normal;
}
pre > code { background: none; padding: 0; font-size: inherit; white-space: unset; }

ul { padding: 0 0 0 1em; list-style-type: square; }
ul > li, ol > li { margin: 0.5em 0; }

sup, sub, .note-ref, .note-number { vertical-align: baseline; position: relative; font-size: .7em; line-height: 1; }
sup, .note-ref, .note-number { bottom: 1.4ex; }
sub { top: .5ex; }

.footnote{ margin: 0 5px; }
.footnotes-br { width: 100px; height: 2px; background: #000000; margin-top: 5em; }
.footnotes > ol { padding-left: 1em; }

.notes { font-size: 0.8em; }
.note-number { margin-left: -1em; }

.date { color: rgba(0, 0, 0, 0.55); font-size: 14px; margin-left: 4px; }

.footer { color: rgba(0, 0, 0, 0.55); }
.footer { font-size: 16px; margin-bottom: 5em; }
.footer > .separator { margin: 0 4px; }
.footer > a { margin-right: 5px; }
.footer > a:hover { color: #000; }

.hoverable { object-fit: cover; object-position: center top; }
.hoverable.clicked { object-position: center bottom; }

@media(hover: hover) {
  .hoverable.clicked:hover { object-fit: cover; object-position: center top; }
  .hoverable: hover { object-position: center bottom; }
}
";*/

        Feed.Post _post = null;
        public Feed.Post Post
        {
            get => _post;
            set
            {
                /*if (value == null)
                {
                    WebContent.NavigateToString("<html></html>");
                }
                else
                {
                    WebContent.NavigateToString($@"
<html>
    <head>
        <meta charset=""UTF-8""/>
        <style>{CSS}</style>
    </head>
    <body>
        <h1>{value.Title}</h1>
        <div class=""post"">
            {App.Manager.LoadPostContent(value)}
        </div>
    </body>
</html>");
                }
                _post = value;
                */
            }
        }

        public PostContent()
        {
            InitializeComponent();
        }

        private void LinkClicked(object sender, NavigatingCancelEventArgs e)
        {
            if (e.Uri != null)
            {
                e.Cancel = true;
                var url = e.Uri.ToString();
                Process.Start("explorer", url);
            }
        }
    }
}
