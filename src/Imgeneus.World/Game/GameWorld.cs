﻿using Imgeneus.Core.DependencyInjection;
using Imgeneus.Database;
using Imgeneus.Database.Preload;
using Imgeneus.DatabaseBackgroundService;
using Imgeneus.Network.Packets.Game;
using Imgeneus.Network.Server;
using Imgeneus.World.Game.Blessing;
using Imgeneus.World.Game.Chat;
using Imgeneus.World.Game.Duel;
using Imgeneus.World.Game.Dyeing;
using Imgeneus.World.Game.Linking;
using Imgeneus.World.Game.PartyAndRaid;
using Imgeneus.World.Game.Player;
using Imgeneus.World.Game.Trade;
using Imgeneus.World.Game.Zone;
using Imgeneus.World.Game.Zone.MapConfig;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Imgeneus.World.Game
{
    /// <summary>
    /// The virtual representation of game world.
    /// </summary>
    public class GameWorld : IGameWorld
    {
        private readonly ILogger<GameWorld> _logger;
        private readonly IBackgroundTaskQueue _taskQueue;
        private readonly IDatabasePreloader _databasePreloader;
        private readonly IMapsLoader _mapsLoader;
        private readonly CharacterConfiguration _characterConfig;

        public GameWorld(ILogger<GameWorld> logger, IBackgroundTaskQueue taskQueue, IDatabasePreloader databasePreloader, IMapsLoader mapsLoader, CharacterConfiguration characterConfig)
        {
            _logger = logger;
            _taskQueue = taskQueue;
            _databasePreloader = databasePreloader;
            _mapsLoader = mapsLoader;
            _characterConfig = characterConfig;

            InitMaps();
        }

        #region Maps 

        /// <summary>
        /// Thread-safe dictionary of maps. Where key is map id.
        /// </summary>
        public ConcurrentDictionary<ushort, Map> Maps { get; private set; } = new ConcurrentDictionary<ushort, Map>();

        /// <summary>
        /// Initializes maps with startup values like mobs, npc, areas, obelisks etc.
        /// </summary>
        private void InitMaps()
        {
            var mapDefinitions = _mapsLoader.LoadMapDefinitions();
            foreach (var mapDefinition in mapDefinitions.Maps)
            {
                var config = _mapsLoader.LoadMapConfiguration(mapDefinition.Id);

                if (mapDefinition.CreateType == CreateType.Default)
                {
                    config.Obelisks = _mapsLoader.GetObelisks(mapDefinition.Id);

                    var map = new Map(mapDefinition.Id, mapDefinition, config, DependencyContainer.Instance.Resolve<ILogger<Map>>(), _databasePreloader);
                    if (Maps.TryAdd(mapDefinition.Id, map))
                        _logger.LogInformation($"Map {map.Id} was successfully loaded.");
                }
            }
        }

        #endregion

        #region Players

        /// <inheritdoc />
        public ConcurrentDictionary<int, Character> Players { get; private set; } = new ConcurrentDictionary<int, Character>();

        public ConcurrentDictionary<int, TradeManager> TradeManagers { get; private set; } = new ConcurrentDictionary<int, TradeManager>();

        public ConcurrentDictionary<int, PartyManager> PartyManagers { get; private set; } = new ConcurrentDictionary<int, PartyManager>();

        public ConcurrentDictionary<int, DuelManager> DuelManagers { get; private set; } = new ConcurrentDictionary<int, DuelManager>();

        /// <inheritdoc />
        public async Task<Character> LoadPlayer(int characterId, WorldClient client)
        {
            using var database = DependencyContainer.Instance.Resolve<IDatabase>();
            var dbCharacter = await database.Characters.Include(c => c.Skills).ThenInclude(cs => cs.Skill)
                                                       .Include(c => c.Items).ThenInclude(ci => ci.Item)
                                                       .Include(c => c.ActiveBuffs).ThenInclude(cb => cb.Skill)
                                                       .Include(c => c.Friends).ThenInclude(cf => cf.Friend)
                                                       .Include(c => c.Quests)
                                                       .Include(c => c.QuickItems)
                                                       .Include(c => c.User)
                                                       .FirstOrDefaultAsync(c => c.Id == characterId);
            var newPlayer = Character.FromDbCharacter(dbCharacter,
                                                     DependencyContainer.Instance.Resolve<ILogger<Character>>(),
                                                     this,
                                                     _characterConfig,
                                                     _taskQueue,
                                                     _databasePreloader,
                                                     DependencyContainer.Instance.Resolve<IChatManager>(),
                                                     DependencyContainer.Instance.Resolve<ILinkingManager>(),
                                                     DependencyContainer.Instance.Resolve<IDyeingManager>());
            newPlayer.Client = client;

            Players.TryAdd(newPlayer.Id, newPlayer);
            TradeManagers.TryAdd(newPlayer.Id, new TradeManager(this, newPlayer));
            PartyManagers.TryAdd(newPlayer.Id, new PartyManager(this, newPlayer));
            DuelManagers.TryAdd(newPlayer.Id, new DuelManager(this, newPlayer));

            _logger.LogDebug($"Player {newPlayer.Id} connected to game world");
            newPlayer.Client.OnPacketArrived += Client_OnPacketArrived;

            return newPlayer;
        }

        private void Client_OnPacketArrived(ServerClient sender, IDeserializedPacket packet)
        {
            switch (packet)
            {
                case CharacterEnteredMapPacket enteredMapPacket:
                    LoadPlayerInMap(((WorldClient)sender).CharID);
                    break;
            }

        }

        /// <inheritdoc />
        public void LoadPlayerInMap(int characterId)
        {
            var player = Players[characterId];
            Maps[player.MapId].LoadPlayer(player);
        }

        /// <inheritdoc />
        public void RemovePlayer(int characterId)
        {
            Character player;
            if (Players.TryRemove(characterId, out player))
            {
                _logger.LogDebug($"Player {characterId} left game world");

                TradeManagers.TryRemove(characterId, out var tradeManager);
                tradeManager.Dispose();

                PartyManagers.TryRemove(characterId, out var partyManager);
                partyManager.Dispose();

                DuelManagers.TryRemove(characterId, out var duelManager);
                duelManager.Dispose();

                player.Client.OnPacketArrived -= Client_OnPacketArrived;

                var map = Maps[player.MapId];
                map.UnloadPlayer(player);
                player.Dispose();
            }
            else
            {
                // 0 means, that connection with client was lost, when he was in character selection screen.
                if (characterId != 0)
                {
                    _logger.LogError($"Couldn't remove player {characterId} from game world");
                }
            }

        }

        #endregion


        #region Bless

        /// <summary>
        /// Goddess bless.
        /// </summary>
        public Bless Bless { get; private set; } = Bless.Instance;

        #endregion
    }
}
