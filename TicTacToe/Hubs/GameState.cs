using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicTacToe.Models;

namespace TicTacToe.Server
{
    // This class can statically persist a collection of players and
    // matches that each of the players are playing using the singleton pattern.
    public class GameState
    {
        // A reference to all players. Key is the unique ID of the player.
        private readonly ConcurrentDictionary<string, Player> players =
            new ConcurrentDictionary<string, Player>(StringComparer.OrdinalIgnoreCase);

        // A reference to all games. Key is the group name of the game.
        private readonly ConcurrentDictionary<string, Game> games =
            new ConcurrentDictionary<string, Game>(StringComparer.OrdinalIgnoreCase);

        // A queue of players that are waiting for an opponent.
        private readonly ConcurrentQueue<Player> waitingPlayers =
            new ConcurrentQueue<Player>();

        public GameState(IServiceProvider serviceProvider, IHubContext<GameHub> context)
        {
            this.Clients = context.Clients;
            this.Groups = context.Groups;
        }

        public IHubClients Clients { get; set; }

        public IGroupManager Groups { get; set; }

        public Player CreatePlayer(string username, string connectionId)
        {
            var player = new Player(username, connectionId);
            this.players[connectionId] = player;

            return player;
        }

        // Retrieves the player that has the given ID.
        public Player GetPlayer(string playerId)
        {
            Player foundPlayer;
            if (!this.players.TryGetValue(playerId, out foundPlayer))
            {
                return null;
            }

            return foundPlayer;
        }

        // Retrieves the game that the given player is playing in.
        public Game GetGame(Player player, out Player opponent)
        {
            opponent = null;
            Game foundGame = this.games.Values.FirstOrDefault(g => g.Id == player.GameId);

            if (foundGame == null)
            {
                return null;
            }

            opponent = (player.Id == foundGame.Player1.Id) ?
                foundGame.Player2 :
                foundGame.Player1;

            return foundGame;
        }

        // Retrieves a game waiting for players.
        public Player GetWaitingOpponent()
        {
            Player foundPlayer;
            if (!this.waitingPlayers.TryDequeue(out foundPlayer))
            {
                return null;
            }

            return foundPlayer;
        }

        // Forgets the specified game. Use if the game is over.
        // No need to manually remove a user from a group when the connection ends.
        public void RemoveGame(string gameId)
        {
            // Remove the game
            Game foundGame;
            if (!this.games.TryRemove(gameId, out foundGame))
            {
                throw new InvalidOperationException("Game not found.");
            }

            // Remove the players, best effort
            Player foundPlayer;
            this.players.TryRemove(foundGame.Player1.Id, out foundPlayer);
            this.players.TryRemove(foundGame.Player2.Id, out foundPlayer);
        }

        // Adds specified player to the waiting pool.
        public void AddToWaitingPool(Player player)
        {
            this.waitingPlayers.Enqueue(player);
        }

        // Determines if the username is already taken, ignoring case.
        public bool IsUsernameTaken(string username)
        {
            return this.players.Values.FirstOrDefault(player => player.Name.Equals(username, StringComparison.InvariantCultureIgnoreCase)) != null;
        }

        // Creates a new pending game which will be waiting for more players.
        public async Task<Game> CreateGame(Player firstPlayer, Player secondPlayer)
        {
            // Define the new game and add to waiting pool
            Game game = new Game(firstPlayer, secondPlayer);
            this.games[game.Id] = game;

            // Create a new group to manage communication using ID as group name
            await this.Groups.AddToGroupAsync(firstPlayer.Id, groupName: game.Id);
            await this.Groups.AddToGroupAsync(secondPlayer.Id, groupName: game.Id);

            return game;
        }
    }
}
