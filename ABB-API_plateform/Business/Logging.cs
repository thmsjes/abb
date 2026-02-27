namespace ABB_API_plateform.Business
{

    public interface ILogging
    {
        void LogToFile(string message);
    }
    public class Logging: ILogging
    {
        public void LogToFile(string message)
        {
            // This creates a logs folder in your API's root directory
            string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");

            if (!Directory.Exists(logPath))
            {
                Directory.CreateDirectory(logPath);
            }

            string filePath = Path.Combine(logPath, $"log_{DateTime.Now:yyyy-MM-dd}.txt");
            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}{Environment.NewLine}";

            File.AppendAllText(filePath, logEntry);
        }
    }
}
