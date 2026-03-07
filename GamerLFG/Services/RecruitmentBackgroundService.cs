using GamerLFG.Models;
using GamerLFG.service;
using GamerLFG.Services.Interface;
using MongoDB.Driver;

namespace GamerLFG.Services
{
    public class RecruitmentBackgroundService : BackgroundService
    {
        private readonly MongoDBservice _database;
        private readonly ILobbyService _lobbyService;
        private readonly ILogger<RecruitmentBackgroundService> _logger;

        public RecruitmentBackgroundService(
            MongoDBservice database,
            ILobbyService lobbyService,
            ILogger<RecruitmentBackgroundService> logger)
        {
            _database = database;
            _lobbyService = lobbyService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var filter = Builders<Lobby>.Filter.And(
                        Builders<Lobby>.Filter.Lt(l => l.EndRecruiting, DateTime.UtcNow),
                        Builders<Lobby>.Filter.Eq(l => l.AutoRecruitProcessed, false),
                        Builders<Lobby>.Filter.Eq(l => l.IsComplete, false)
                    );

                    var lobbies = await _database.Lobbies.Find(filter).ToListAsync(stoppingToken);

                    foreach (var lobby in lobbies)
                    {
                        try
                        {
                            await _lobbyService.ProcessAutoRecruitAsync(lobby.Id!);
                            _logger.LogInformation("Auto-recruit processed for lobby {LobbyId} ({Title})", lobby.Id, lobby.Title);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to auto-recruit for lobby {LobbyId}", lobby.Id);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in RecruitmentBackgroundService loop");
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}
