using Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.ServiceModel;

namespace Client
{
    class Program
    {
        static string logPath = "log.txt";

        static void Main(string[] args)
        {
            string dataFolder = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "Data");

            if (!Directory.Exists(dataFolder))
            {
                Console.WriteLine("[KLIJENT] Data folder nije pronadjen!");
                Console.ReadLine();
                return;
            }

            string[] folderi = Directory.GetDirectories(dataFolder);
            if (folderi.Length == 0)
            {
                Console.WriteLine("[KLIJENT] Nema dostupnih vozila!");
                Console.ReadLine();
                return;
            }

            Console.WriteLine("=== Dostupna vozila ===");
            for (int i = 0; i < folderi.Length; i++)
            {
                Console.WriteLine($"{i + 1}. {Path.GetFileName(folderi[i])}");
            }

            Console.Write("\nIzaberite vozilo (unesite broj): ");
            int izbor;
            while (!int.TryParse(Console.ReadLine(), out izbor)
                || izbor < 1 || izbor > folderi.Length)
            {
                Console.Write("Neispravan unos. Pokusajte ponovo: ");
            }

            string izabraniFolder = folderi[izbor - 1];
            string vehicleId = Path.GetFileName(izabraniFolder);
            string csvPath = Path.Combine(izabraniFolder, "Charging_Profile.csv");

            if (!File.Exists(csvPath))
            {
                Console.WriteLine($"[KLIJENT] Fajl Charging_Profile.csv nije pronadjen!");
                Console.ReadLine();
                return;
            }

            ChannelFactory<IEVChargingService> factory = null;
            IEVChargingService kanal = null;

            try
            {
                factory = new ChannelFactory<IEVChargingService>("EVChargingEndpoint");
                kanal = factory.CreateChannel();

                kanal.StartSession(vehicleId);
                Console.WriteLine($"\n[KLIJENT] Sesija zapoceta za: {vehicleId}");

                List<ChargingSample> uzorci = ProcitajCSV(csvPath, vehicleId);

                foreach (ChargingSample uzorak in uzorci)
                {
                    try
                    {
                        kanal.PushSample(uzorak);
                        Console.WriteLine($"[KLIJENT] Poslat red {uzorak.RowIndex}");
                    }
                    catch (FaultException<string> ex)
                    {
                        string poruka = $"Red {uzorak.RowIndex} odbijen: {ex.Detail}";
                        Console.WriteLine($"[KLIJENT] {poruka}");
                        ZabeležiGrešku(poruka);
                    }
                }

                kanal.EndSession(vehicleId);
                Console.WriteLine($"[KLIJENT] Sesija zavrsena za: {vehicleId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[KLIJENT] Greska u komunikaciji: {ex.Message}");
                ZabeležiGrešku($"Greska u komunikaciji: {ex.Message}");
            }
            finally
            {
                if (kanal != null)
                {
                    IClientChannel clientChannel = (IClientChannel)kanal;

                    if (clientChannel.State == CommunicationState.Faulted)
                        clientChannel.Abort();
                    else
                        clientChannel.Close();
                }

                if (factory != null)
                {
                    if (factory.State == CommunicationState.Faulted)
                        factory.Abort();
                    else
                        factory.Close();
                }
            }

            Console.WriteLine("\n[KLIJENT] Gotovo. Pritisni Enter za izlaz.");
            Console.ReadLine();
        }

        static List<ChargingSample> ProcitajCSV(string putanja, string vehicleId)
        {
            List<ChargingSample> lista = new List<ChargingSample>();
            int rowIndex = 0;

            using (StreamReader reader = new StreamReader(putanja))
            {
                string header = reader.ReadLine();
                string linija;

                while ((linija = reader.ReadLine()) != null)
                {
                    rowIndex++;
                    try
                    {
                        ChargingSample uzorak = ParsirajLiniju(
                            linija, rowIndex, vehicleId);
                        lista.Add(uzorak);
                    }
                    catch (Exception ex)
                    {
                        string poruka = $"Red {rowIndex} nije mogao biti parsiran: {ex.Message}";
                        Console.WriteLine($"[KLIJENT] {poruka}");
                        ZabeležiGrešku(poruka);
                    }
                }
            }

            Console.WriteLine($"[KLIJENT] Procitano {lista.Count} validnih redova iz CSV-a.");
            return lista;
        }

        static ChargingSample ParsirajLiniju(
            string linija, int rowIndex, string vehicleId)
        {
            string[] delovi = linija.Split(',');

            if (delovi.Length < 19)
                throw new Exception($"Nedovoljan broj kolona: {delovi.Length}");

            return new ChargingSample
            {
                VehicleId = vehicleId,
                RowIndex = rowIndex,
                Timestamp = delovi[0].Trim(),
                VoltageMin = double.Parse(delovi[1].Trim(),
                                     CultureInfo.InvariantCulture),
                VoltageAvg = double.Parse(delovi[2].Trim(),
                                     CultureInfo.InvariantCulture),
                VoltageMax = double.Parse(delovi[3].Trim(),
                                     CultureInfo.InvariantCulture),
                CurrentMin = double.Parse(delovi[4].Trim(),
                                     CultureInfo.InvariantCulture),
                CurrentAvg = double.Parse(delovi[5].Trim(),
                                     CultureInfo.InvariantCulture),
                CurrentMax = double.Parse(delovi[6].Trim(),
                                     CultureInfo.InvariantCulture),
                RealPowerMin = double.Parse(delovi[7].Trim(),
                                     CultureInfo.InvariantCulture),
                RealPowerAvg = double.Parse(delovi[8].Trim(),
                                     CultureInfo.InvariantCulture),
                RealPowerMax = double.Parse(delovi[9].Trim(),
                                     CultureInfo.InvariantCulture),
                ReactivePowerMin = double.Parse(delovi[10].Trim(),
                                     CultureInfo.InvariantCulture),
                ReactivePowerAvg = double.Parse(delovi[11].Trim(),
                                     CultureInfo.InvariantCulture),
                ReactivePowerMax = double.Parse(delovi[12].Trim(),
                                     CultureInfo.InvariantCulture),
                ApparentPowerMin = double.Parse(delovi[13].Trim(),
                                     CultureInfo.InvariantCulture),
                ApparentPowerAvg = double.Parse(delovi[14].Trim(),
                                     CultureInfo.InvariantCulture),
                ApparentPowerMax = double.Parse(delovi[15].Trim(),
                                     CultureInfo.InvariantCulture),
                FrequencyMin = double.Parse(delovi[16].Trim(),
                                     CultureInfo.InvariantCulture),
                FrequencyAvg = double.Parse(delovi[17].Trim(),
                                     CultureInfo.InvariantCulture),
                FrequencyMax = double.Parse(delovi[18].Trim(),
                                     CultureInfo.InvariantCulture),
            };
        }

        static void ZabeležiGrešku(string poruka)
        {
            using (StreamWriter writer = new StreamWriter(logPath, append: true))
            {
                writer.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {poruka}");
            }
        }
    }
}