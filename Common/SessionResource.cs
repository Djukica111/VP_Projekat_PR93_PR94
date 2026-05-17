using System;
using System.IO;

namespace Common
{
    public class SessionResource : IDisposable
    {
        private StreamWriter _writer;
        private bool _disposed = false;
        public string VehicleId { get; private set; }

        public SessionResource(string filePath, string vehicleId)
        {
            VehicleId = vehicleId;
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            _writer = new StreamWriter(filePath, append: true);
            _writer.AutoFlush = true;
        }

        public void WriteLine(string line)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(SessionResource));

            _writer.WriteLine(line);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_writer != null)
                    {
                        _writer.Close();
                        _writer = null;
                        Console.WriteLine($"[DISPOSE] StreamWriter za vozilo {VehicleId} je zatvoren.");
                    }
                }
                _disposed = true;
            }
        }

        ~SessionResource()
        {
            Dispose(false);
        }
    }
}