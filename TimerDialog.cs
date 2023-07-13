using System.Diagnostics;
using System.ComponentModel.DataAnnotations;
using System;
using Gtk;
using Cairo;
using UI = Gtk.Builder.ObjectAttribute;
using System.Timers;
using System.Threading;
using System.Threading.Tasks;

namespace EventTimer
{
    public class TimerDialog : Dialog
    {

        private readonly SignalPlayer _signalPlayer = new SignalPlayer();
        private readonly Settings _settings;

        private PeriodicTimer _periodicTimer = null;

        [UI] private DrawingArea  _drawTimer = null;
        [UI] private Button _buttonStart = null;
        [UI] private Button _buttonStop = null;
        [UI] private Button _buttonExit = null;
        [UI] private Image _image1 = null;
        [UI] private Image _image2 = null;
        [UI] private Image _image3 = null;
        [UI] private Image _image4 = null;
        [UI] private Image _image5 = null;
        [UI] private Image _image6 = null;
        [UI] private Image _imageS1 = null;
        [UI] private Image _imageS2 = null;

        private Stopwatch _stopwatch = new Stopwatch();

        private int _lastHour = -1;
        private int _lastMinute = -1;
        private int _lastSecond = -1;
        private bool _autoStarted = false;

        private readonly Gdk.RGBA _autoStartColor = new Gdk.RGBA {
            Red = 256,
            Green = 256,
            Blue = 256,
            Alpha = 256
        };
        private readonly Gdk.RGBA _timingColor = new Gdk.RGBA {
            Red = 0,
            Green = 0,
            Blue = 0,
            Alpha = 256
        };

        Gdk.Pixbuf[] _pixbufs = new Gdk.Pixbuf[] {
            Gdk.Pixbuf.LoadFromResource("num-0"),
            Gdk.Pixbuf.LoadFromResource("num-1"),
            Gdk.Pixbuf.LoadFromResource("num-2"),
            Gdk.Pixbuf.LoadFromResource("num-3"),
            Gdk.Pixbuf.LoadFromResource("num-4"),
            Gdk.Pixbuf.LoadFromResource("num-5"),
            Gdk.Pixbuf.LoadFromResource("num-6"),
            Gdk.Pixbuf.LoadFromResource("num-7"),
            Gdk.Pixbuf.LoadFromResource("num-8"),
            Gdk.Pixbuf.LoadFromResource("num-9"),
            Gdk.Pixbuf.LoadFromResource("num-sep")
        };
       
        public TimerDialog(ref Settings settings) : this(new Builder("Windows.glade")) {
            _settings = settings;
            SetBackgroundColor(settings.StartAt != null ? _autoStartColor : _timingColor);
        }

        private TimerDialog(Builder builder) : base(builder.GetRawOwnedObject("TimerDialog"))
        {
            builder.Autoconnect(this);

            Destroyed += Window_Destroyed;
            DeleteEvent += Window_DeleteEvent;
            _buttonStart.Clicked += Start_Clicked;
            _buttonStop.Clicked += Stop_Clicked;
            _buttonExit.Clicked += Exit_Clicked;
            this.Shown += (s,e) => {
                Task.Delay(100).ContinueWith(t=> Start());
            };
        }

        

        private void Start()
        {
            GetSize(out var screenWidth, out _);
            
            //var dw = builder.Application.ActiveWindow.Screen.Width / 10;
            var dw = screenWidth / 10;

            for (var i = 0; i < _pixbufs.Length -1; i++) {
                var width = dw;
                var height = width * 2;
                _pixbufs[i] = _pixbufs[i].ScaleSimple(width, height, Gdk.InterpType.Bilinear);
            }
            var swidth = dw / 4;
            var sheight = dw * 2;
            _pixbufs[10] = _pixbufs[10].ScaleSimple(swidth, sheight, Gdk.InterpType.Bilinear);

            _periodicTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(100));
            SetImages(0, 0, 0, true);
            StartTimer();  
        }

        private void SetBackgroundColor(Gdk.RGBA rgba)
        {
            this.OverrideBackgroundColor(StateFlags.Normal, rgba);
        }


        private async void Window_Destroyed(object sender, EventArgs a)
        {
        }

        private async void Window_DeleteEvent(object sender, DeleteEventArgs a)
        {
            //Application.Quit();
        }

        private TimeSpan _dif = TimeSpan.Zero;
        private async void StartTimer()
        {
            while (await _periodicTimer.WaitForNextTickAsync())
            {                
                if (_stopwatch.IsRunning)
                {
                    var current = _stopwatch.Elapsed + _dif;
                    SetImages(current.Hours, current.Minutes, current.Seconds, false);
                }
                else if (_settings.StartAt != null)
                {
                    if (_settings.StartAt.Value > DateTime.Now)
                    {
                        var remaining = _settings.StartAt.Value - DateTime.Now;
                        SetImages(remaining.Hours, remaining.Minutes, remaining.Seconds, false);
                    }
                    else if (!_autoStarted)
                    {
                        _dif = DateTime.Now - _settings.StartAt.Value;
                        _autoStarted = true;
                        SetBackgroundColor(_timingColor);
                        _stopwatch.Start();
                        if (_dif.TotalSeconds < 5.0)
                            await _signalPlayer.Play();
                    }
                }
            }
        }

        private async void Start_Clicked(object sender, EventArgs a)
        {
            if (_stopwatch.IsRunning)
                _stopwatch.Stop();
            _settings.StartAt = DateTime.Now;
            _dif = TimeSpan.Zero;
            await _settings.Save();
            _stopwatch.Reset();
            _stopwatch.Start();
            await _signalPlayer.Play();
        }

        private async void Stop_Clicked(object sender, EventArgs a)
        {
            _stopwatch.Stop();
            _settings.StartAt = null;
            _dif = TimeSpan.Zero;
            await _settings.Save();
        }

        private async void Exit_Clicked(object sender, EventArgs a)
        {
            _stopwatch.Stop();
            _settings.StartAt = null;
            _dif = TimeSpan.Zero;
            await _settings.Save();
            _periodicTimer.Dispose();
            this.Destroy();
        }

        private void SetImages(int hour, int minute, int second, bool setSpacers)
        {
            Gtk.Application.Invoke(delegate
            {
                if (_lastHour != hour)
                {
                    _lastHour = hour;
                    var h1 = (int)Math.Floor(hour / 10m);
                    var h2 = (int)Math.Floor(hour % 10m);
                    _image1.Pixbuf = _pixbufs[h1];
                    _image2.Pixbuf = _pixbufs[h2];
                }
                if (_lastMinute != minute)
                {
                    _lastMinute = minute;
                    var m1 = (int)Math.Floor(minute / 10m);
                    var m2 = (int)Math.Floor(minute % 10m);
                    _image3.Pixbuf = _pixbufs[m1];
                    _image4.Pixbuf = _pixbufs[m2];
                }
                if (_lastSecond != second)
                {
                    _lastSecond = second;
                    var s1 = (int)Math.Floor(second / 10m);
                    var s2 = (int)Math.Floor(second % 10m);
                    _image5.Pixbuf = _pixbufs[s1];
                    _image6.Pixbuf = _pixbufs[s2];
                }
                if (setSpacers)
                {
                    _imageS1.Pixbuf = _pixbufs[10];
                    _imageS2.Pixbuf = _pixbufs[10];
                }
            });
        }

    }
}