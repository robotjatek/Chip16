namespace EmuCore
{
    public class System
    {
        private readonly ICPU _cpu;
        private readonly IBus _bus;

        public System()
        {
            _cpu = new CPU();
            _bus = new Bus(_cpu);

            _cpu.Bus = _bus;
        }

        public void LoadFile(string path)
        {
            var fileloader = new FileLoader(_bus);
            fileloader.LoadFile(path);
        }

        public void ExecuteCycle()
        {
            _bus.ExecuteCycle();
        }
    }
}
