using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicTacToe.Models
{
    // Represents a user playing the game.
    public class Player
    {
        public Player(string name, string id)
        {
            this.Name = name;
            this.Id = id;
        }

        // A user defined username to display as a friendly name.
        public string Name { get; private set; }

        // A unique ID to identify a player. Use the SignalR connection ID.
        public string Id { get; private set; }

        // The unique ID to identify the game a player is playing; otherwise null.
        // The game will decide the game ID.
        public string GameId { get; set; }

        // Defines the piece that will be placed on the board to represent this user.
        // The game will decide the game piece.
        public string Piece { get; set; }

        public override string ToString()
        {
            return String.Format("(Id={0}, Name={1}, GameId={2}, Piece={3})",
                this.Id, this.Name, this.GameId, this.Piece);
        }

        public override bool Equals(object obj)
        {
            Player other = obj as Player;

            if (other == null)
            {
                return false;
            }

            return this.Id.Equals(other.Id) && this.Name.Equals(other.Name);
        }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode() * this.Name.GetHashCode();
        }
    }
}