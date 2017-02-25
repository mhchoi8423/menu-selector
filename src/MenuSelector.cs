using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MenuSelector
{
    public sealed class MenuSelector
    {
        private List<string> _menus = new List<string>();
        private readonly Queue<string> _shuffledMenus = new Queue<string>();

        private readonly MenuLoader _loader = new MenuLoader();
        private readonly MenuNotifier _notifier = new MenuNotifier();

        public bool Initialize()
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            if (_loader.Load(out _menus) == false)
                return false;

            if (_notifier.Initialize() == false)
                return false;

            return true;
        }

        public async Task Update()
        {
            ShuffleIfNeeded();

            if (_notifier.IsTimeUp())
            {
                var menu = Select();
                await _notifier.Notice(menu);
            }
        }

        private void ShuffleIfNeeded()
        {
            if (DateTime.Now.DayOfWeek != DayOfWeek.Sunday && _shuffledMenus.Count != 0)
                return;

            var menus = _menus.ToArray();

            FisherYates.Shuffle(menus);

            _shuffledMenus.Clear();
            foreach (var menu in menus)
            {
                _shuffledMenus.Enqueue(menu);
            }
        }

        private string Select()
        {
            return _shuffledMenus.Dequeue();
        }
    }
}
