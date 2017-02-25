using System.ServiceProcess;
using System.Timers;

namespace MenuSelector
{
    public partial class Scheduler : ServiceBase
    {
        private Timer _timer;
        private readonly MenuSelector _menuSelector = new MenuSelector();

        public Scheduler()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            if (_menuSelector.Initialize() == false)
                return;

            _timer = new Timer { Interval = 30000 };
            _timer.Elapsed += Timer_OnTick;
            _timer.Enabled = true;
        }

        private async void Timer_OnTick(object sender, ElapsedEventArgs e)
        {
            await _menuSelector.Update();
        }

        protected override void OnStop()
        {
            _timer.Enabled = false;
            _timer.Elapsed -= Timer_OnTick;
        }
    }
}
