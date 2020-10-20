namespace EmuCore
{
    public class System
    {
        private readonly ICPU _cpu;
        private readonly IGPU _gpu;
        private readonly IBus _bus;

        public System()
        {
            _cpu = new CPU();
            _gpu = new GPU();
            _bus = new Bus(_cpu, _gpu);

            _cpu.Bus = _bus;
            _gpu.Bus = _bus;
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
