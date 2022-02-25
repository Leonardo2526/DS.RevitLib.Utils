using System;
using System.IO;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace iUtils
{
    /// <summary>
    /// Класс для получения настроек из файла
    /// с автоматическим отслеживанием изменений в файле
    /// </summary>
    public class Configurations
    {
        private FileSystemWatcher w;
        private JObject settings;

        /// <summary>
        /// Индексатор для доступа к значениям настроке в файле
        /// Поддерживается запись через двоеточине, например 'connectionStrings:Default:first'
        /// </summary>
        /// <param name="path">Путь к параметру</param>
        /// <returns>Возвращает строковое представление настройки</returns>
        public string this[string path]
        {
            get
            {
                if (path == null)
                    return "";
                if (!path.Contains(":"))
                    return settings[path]?.ToString();
                else
                {
                    var split = path.Split(':');
                    switch (split.Length)
                    {
                        case 2:
                            return settings[split[0]]?[split[1]]?.ToString();
                        case 3:
                            return settings[split[0]]?[split[1]]?[split[2]]?.ToString();
                        case 4:
                            return settings[split[0]]?[split[1]]?[split[2]]?[split[3]]?.ToString();
                        case 5:
                            return settings[split[0]]?[split[1]]?[split[2]]?[split[3]]?[split[4]]?.ToString();
                        case 6:
                            return settings[split[0]]?[split[1]]?[split[2]]?[split[3]]?[split[4]]?[split[5]]?.ToString();
                        case 7:
                            return settings[split[0]]?[split[1]]?[split[2]]?[split[3]]?[split[4]]?[split[5]]?[split[6]].ToString();
                        case 8:
                            return settings[split[0]]?[split[1]]?[split[2]]?[split[3]]?[split[4]]?[split[5]]?[split[6]]?[split[7]].ToString();
                        case 9:
                            return settings[split[0]]?[split[1]]?[split[2]]?[split[3]]?[split[4]]?[split[5]]?[split[6]]?[split[7]]?[split[8]].ToString();
                        case 10:
                            return settings[split[0]]?[split[1]]?[split[2]]?[split[3]]?[split[4]]?[split[5]]?[split[6]]?[split[7]]?[split[8]]?[split[9]].ToString();
                        case 11:
                            return settings[split[0]]?[split[1]]?[split[2]]?[split[3]]?[split[4]]?[split[5]]?[split[6]]?[split[7]]?[split[8]]?[split[9]]?[split[10]].ToString();

                    }
                    return "";
                }
            }
        }


        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="path">путь к папке с файлом конфигурации</param>
        /// <param name="filename">имя файла конфигурации</param>
        public Configurations(string path, string filename = "appsettings.json")
        {
            CreateWatcher(path, filename);
            settings = ReadFile(Path.Combine(path, filename));

        }


        /// <summary>
        /// Создает объект, который следит за изменениями в указанной папке
        /// </summary>
        /// <param name="path">путь к папке для отслеживания изменений</param>
        /// <param name="filename">имя файла для отслеживания изменений</param>
        private void CreateWatcher(string path, string filename)
        {
            w = new FileSystemWatcher(path, filename);
            w.NotifyFilter = NotifyFilters.LastWrite;
            w.EnableRaisingEvents = true;
            w.Changed += (s, e) =>
            {
                if (e.ChangeType != WatcherChangeTypes.Changed)
                    return;

                Thread.Sleep(200);
                settings = ReadFile(e.FullPath);
            };

        }


        /// <summary>
        /// Считывает содержимое файла
        /// </summary>
        /// <param name="fullPath">путь к файлу</param>
        /// <returns>Возвращает десерелизованный обхект из файла конфигурации</returns>
        private JObject ReadFile(string fullPath)
        {
            try
            {
                using (var rs = new StreamReader(new FileStream(fullPath, FileMode.Open, FileAccess.Read)))
                {
                    return (JObject)JsonConvert.DeserializeObject(rs.ReadToEnd());
                }

            }
            catch (Exception)
            { }
            return settings;
        }
    }
}
