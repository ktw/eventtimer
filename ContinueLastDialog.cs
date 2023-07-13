using System;
using Gtk;

namespace EventTimer
{
    public class ContinueLastDialog : MessageDialog
    {
        public ContinueLastDialog() : this(new Builder("Windows.glade")) {
        }

        private ContinueLastDialog(Builder builder) : base(builder.GetRawOwnedObject("ContinueLastDialog"))
        {
            builder.Autoconnect(this);
            Destroyed += Window_Destroyed;
            DeleteEvent += Window_DeleteEvent;
        }

        private async void Window_Destroyed(object sender, EventArgs a)
        {
        }

        private async void Window_DeleteEvent(object sender, DeleteEventArgs a)
        {
            //Application.Quit();
        }
    }
}