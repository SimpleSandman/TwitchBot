using CefSharp;
using CefSharp.Wpf;

namespace TwitchBotWpf.Handlers
{
    public class MenuHandler : IContextMenuHandler
    {
        public void OnBeforeContextMenu(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model)
        {
            if (model.Count > 0)
            {
                model.AddSeparator();
            }

            model.AddItem((CefMenuCommand)26501, "Show DevTools");
            model.AddItem((CefMenuCommand)26502, "Close DevTools");
        }

        public bool OnContextMenuCommand(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, CefMenuCommand commandId, CefEventFlags eventFlags)
        {
            switch (commandId)
            {
                case (CefMenuCommand)26501:
                    browser.GetHost().ShowDevTools();
                    return true;
                case (CefMenuCommand)26502:
                    browser.GetHost().CloseDevTools();
                    return true;
                case CefMenuCommand.Back:
                    browser.GoBack();
                    return true;
                case CefMenuCommand.Forward:
                    browser.GoForward();
                    return true;
                case CefMenuCommand.Print:
                    browser.GetHost().Print();
                    return true;
                case CefMenuCommand.ViewSource:
                    browser.FocusedFrame.ViewSource();
                    return true;
            }

            return false;
        }

        public void OnContextMenuDismissed(IWebBrowser browserControl, IBrowser browser, IFrame frame)
        {
            var chromiumWebBrowser = (ChromiumWebBrowser)browserControl;

            chromiumWebBrowser.Dispatcher.Invoke(() =>
            {
                chromiumWebBrowser.ContextMenu = null;
            });
        }

        public bool RunContextMenu(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model, IRunContextMenuCallback callback)
        {
            return false;
        }
    }
}
