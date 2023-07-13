using System;
using System.Text.Json;
using System.IO;
using System.Threading.Tasks;

namespace EventTimer
{
    public class Settings
    {
        private const string SETTINGS_FILE = "./settings.json";
        private static readonly JsonSerializerOptions _serializeOptions = new JsonSerializerOptions {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };


        public DateTime? StartAt { get; set; }

        public Settings()
        {            
        }

        public static async Task<Settings> Read()
        {
            Settings settings;
            if (!File.Exists(SETTINGS_FILE))
            {
                settings = new Settings();
                await settings.Save();
            }
            else
            {
                try
                {
                    using var stream = File.OpenRead(SETTINGS_FILE);
                    settings = JsonSerializer.Deserialize<Settings>(stream, _serializeOptions);
                    await stream.DisposeAsync();
                }
                catch (Exception ex)
                {
                    settings = new Settings();
                }
            }            
            return settings;
        }

        public async Task Save()
        {
            using var stream = File.Create(SETTINGS_FILE);
            await JsonSerializer.SerializeAsync(stream, this, _serializeOptions);
            await stream.DisposeAsync();
        }

    }
}