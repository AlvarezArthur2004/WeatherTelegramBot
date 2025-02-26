using Dapper;
using System.Data;
using System.Data.Common;
using Telegram.Bot;

namespace WeatherBot.Services
{
    public class TelegramWeatherBotDbInitializer
    {
        private readonly IDbConnection _WeatherDbConnection;

        public TelegramWeatherBotDbInitializer(IDbConnection WeatherDbConnection)
        {
            _WeatherDbConnection = WeatherDbConnection;
        }

        public async Task InitializeDatabase()
        {
            var createUsersTableQuery = @"
            IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Users')
            BEGIN
                CREATE TABLE Users (
                    Id INT IDENTITY(1,1) PRIMARY KEY,        
                    TelegramId BIGINT NOT NULL UNIQUE,  
                    ChatId BIGINT NOT NULL,
                    CurrentLocation NVARCHAR(255) NOT NULL,  
                    CreatedAt DATETIME DEFAULT GETDATE()    
                );
            END";

            var createWeatherHistoryTableQuery = @"
            IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'WeatherHistory')
            BEGIN
                CREATE TABLE WeatherHistory (
                    Id INT IDENTITY(1,1) PRIMARY KEY,     
                    UserId INT NOT NULL,                   
                    Location NVARCHAR(255) NOT NULL,      
                    WeatherTokenApi NVARCHAR(500) NOT NULL, 
                    DateTime DATETIME DEFAULT GETDATE(), 
                    FOREIGN KEY (UserId) REFERENCES Users(Id) 
                );
            END";

            await _WeatherDbConnection.ExecuteAsync(createUsersTableQuery);
            await _WeatherDbConnection.ExecuteAsync(createWeatherHistoryTableQuery);
        }

    }
}



