using System.Threading.Tasks;
using System.ComponentModel;
using System;
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;

// dotnet publish --runtime linux-x64 --self-contained -p:PublishSingleFile=true

namespace EventTimer
{
    class MainWindow : Window
    {
        private readonly SignalPlayer _signalPlayer = new SignalPlayer();
        private Settings _settings;
        [UI] private Button _buttonOk = null;
        [UI] private Entry _inputHour = null;
        [UI] private Entry _inputMinute = null;
        [UI] private Button _buttonTestAudio = null;

        public MainWindow() : this(new Builder("Windows.glade")) { }

        private MainWindow(Builder builder) : base(builder.GetRawOwnedObject("MainWindow"))
        {
            builder.Autoconnect(this);

            DeleteEvent += Window_DeleteEvent;
            _buttonOk.Clicked += ButtonOk_Clicked;
            _buttonTestAudio.Clicked += ButtonTestAudio_Clicked;
            _inputHour.Changed += TimeInputHour_ButtonPressed;
            _inputMinute.Changed += TimeInputMinute_ButtonPressed;
            _settings = Settings.Read().GetAwaiter().GetResult();
            this.Shown += (s,e) => {
                if (_settings.StartAt.HasValue)
                {
                    
                    //var msg = new Gtk.MessageDialog(this, DialogFlags.Modal | DialogFlags.DestroyWithParent, MessageType.Question, ButtonsType.YesNo, false, "Hello");
                    var msg = new ContinueLastDialog();
                    msg.Response += async (s, e) => {
                        if (e.ResponseId == ResponseType.Yes)
                        {
                            await Task.Delay(10);
                            Start();
                        }
                        else
                        {
                            _settings.StartAt = null;
                            await _settings.Save();
                        }
                    };
                    msg.Run();
                    msg.Destroy();
                }
            };
        }

        private void Window_DeleteEvent(object sender, DeleteEventArgs a)
        {
            Application.Quit();
        }

        private void Start()
        {
            var timer = new TimerDialog(ref _settings);
            timer.Resizable = true;
            timer.Fullscreen();
            timer.Run();
            timer.Destroy();
        }

        private async void ButtonOk_Clicked(object sender, EventArgs a)
        {
            if (!string.IsNullOrWhiteSpace(_inputHour.Text) && !string.IsNullOrWhiteSpace(_inputMinute.Text))
            {
                var now = DateTime.Now;
                _settings.StartAt = new DateTime(now.Year, now.Month, now.Day, int.Parse(_inputHour.Text), int.Parse(_inputMinute.Text), 0);
                await _settings.Save();
            }
            Start();
        }

        private async void ButtonTestAudio_Clicked(object sender, EventArgs a)
        {
            await _signalPlayer.Play();
        }

        private void TimeInputHour_ButtonPressed(object sender, EventArgs a)
        {
            var entry = (Entry)sender;
            var ok = false;
            if (!string.IsNullOrWhiteSpace(entry.Text))
            {
                if (int.TryParse(entry.Text, out var num))
                {
                    if (num >= 0 && num < 24)
                        ok = true;
                }
            }
            if (!ok)
                entry.Text = string.Empty;
        }
        private void TimeInputMinute_ButtonPressed(object sender, EventArgs a)
        {
            var entry = (Entry)sender;
            var ok = false;
            if (!string.IsNullOrWhiteSpace(entry.Text))
            {
                if (int.TryParse(entry.Text, out var num))
                {
                    if (num >= 0 && num < 60)
                        ok = true;
                }
            }
            if (!ok)
                entry.Text = string.Empty;
        }

    }
}
