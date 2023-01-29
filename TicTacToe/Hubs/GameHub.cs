using TicTacToe.Models;
using Microsoft.AspNetCore.SignalR;
using TicTacToe.Server;
using Newtonsoft.Json;

namespace TicTacToe
{
    public class GameHub : Hub
    {
        public GameState gameState { get; set; }
        public GameHub(GameState state)
        {
            gameState = state;
        }

        // The starting point for a client looking to join a new game.
        // Player either starts a game with a waiting opponent or joins the waiting pool.
        public async Task FindGame(string username)
        {
            if (gameState.IsUsernameTaken(username))
            {
                string methodToCall1 = "usernameTaken";
                IClientProxy proxy1 = Clients.Caller;
                await proxy1.SendAsync(methodToCall1);

                return;
            }

            Player joiningPlayer =
                gameState.CreatePlayer(username, this.Context.ConnectionId);
            await Clients.Caller.SendAsync("playerJoined");

            // Find any pending games if any
            Player opponent = gameState.GetWaitingOpponent();
            if (opponent == null)
            {
                // No waiting players so enter the waiting pool
                gameState.AddToWaitingPool(joiningPlayer);
                await Clients.Caller.SendAsync("waitingList");
            }
            else
            {
                // An opponent was found so join a new game and start the game
                // Opponent is first player since they were waiting first
                Game newGame = await gameState.CreateGame(opponent, joiningPlayer);
                var a = JsonConvert.SerializeObject(newGame);
                await Clients.Group(newGame.Id).SendAsync("start", a);
            }
        }

        // Client has requested to place a piece down in the following position.
        public async Task PlacePiece(string rowA, string colA)
        {
            int row = int.Parse(rowA);
            int col = int.Parse(colA);
            Player playerMakingTurn = gameState.GetPlayer(playerId: this.Context.ConnectionId);
            Player opponent;
            Game game = gameState.GetGame(playerMakingTurn, out opponent);

            if (game == null || !game.WhoseTurn.Equals(playerMakingTurn))
            {
                await Clients.Caller.SendAsync("notPlayersTurn");
                return;
            }

            if (!game.IsValidMove(row, col))
            {
                await Clients.Caller.SendAsync("notValidMove");
                return;
            }

            // Notify everyone of the valid move. Only send what is necessary (instead of sending whole board)
            game.PlacePiece(row, col);
            await Clients.Group(game.Id).SendAsync("piecePlaced", row, col, playerMakingTurn.Piece);

            // check if game is over (won or tie)
            if (!game.IsOver)
            {
                // Update the turn like normal if the game is still ongoing
                var a = JsonConvert.SerializeObject(game);
                await Clients.Group(game.Id).SendAsync("updateTurn", a);
            }
            else
            {
                // Determine how the game is over in order to display correct message to client
                if (game.IsTie)
                {
                    await Clients.Group(game.Id).SendAsync("tieGame");
                }
                else
                {
                    // Player outright won
                    await Clients.Group(game.Id).SendAsync("winner", playerMakingTurn.Name);
                }

                // Remove the game (in any game over scenario) to reclaim resources
                gameState.RemoveGame(game.Id);
            }
        }

        // A player that is leaving should end all games and notify the opponent.
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            Player leavingPlayer = gameState.GetPlayer(playerId: this.Context.ConnectionId);

            // Only handle cases where user was a player in a game or waiting for an opponent
            if (leavingPlayer != null)
            {
                Player opponent;
                Game ongoingGame = gameState.GetGame(leavingPlayer, out opponent);
                if (ongoingGame != null)
                {
                    await Clients.Group(ongoingGame.Id).SendAsync("opponentLeft");
                    gameState.RemoveGame(ongoingGame.Id);
                }
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}