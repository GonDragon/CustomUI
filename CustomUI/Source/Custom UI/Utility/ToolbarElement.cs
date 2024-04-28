using Verse;
using CustomUI.Utility.Workers;

namespace CustomUI.Utility
{    public class ToolbarElement
    {
        public readonly Configs.ElementConfig config;
        private WidgetWorker _worker;

        public ToolbarElement(Configs.ElementConfig config)
        {
            this.config = config;
        }

        public bool Configurable => this.config.Configurable;

        public WidgetWorker Worker
        {
            get
            {
                if (_worker == null) _worker = config.Worker;
                return _worker;
            }
        }
    }
}