using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicTacToe.Models;

namespace TicTacToe.Models
{
    public class Game
    {
        private bool isFirstPlayersTurn;

        // Creates a new game object.
        public Game(Player player1, Player player2)
        {
            this.Player1 = player1;
            this.Player2 = player2;
            this.Id = Guid.NewGuid().ToString("d");
            this.Board = new Board();

            this.isFirstPlayersTurn = true;

            // Link the players to the game as well
            this.Player1.GameId = this.Id;
            this.Player2.GameId = this.Id;

            // Assign piece types to each player
            this.Player1.Piece = "X";
            this.Player2.Piece = "O";
        }

        // A unique identifier for this game. Also used as the group name.
        public string Id { get; set; }

        // One of two partipants of the game.
        public Player Player1 { get; set; }

        // One of two participants of the game.
        public Player Player2 { get; set; }

        // The board that represents the tic-tac-toe game.
        public Board Board { get; set; }

        // Returns which player is currently allowed to place a piece down.
        public Player WhoseTurn
        {
            get
            {
                return (this.isFirstPlayersTurn) ?
                    this.Player1 :
                    this.Player2;
            }
        }

        // Returns whether the game is ongoing or has completed.
        // Over states include either a tie or a player has won.
        public bool IsOver
        {
            get
            {
                return this.IsTie || this.Board.IsThreeInRow;
            }
        }

        // Returns whether the game is a tie.
        public bool IsTie
        {
            get
            {
                return !this.Board.AreSpacesLeft;
            }
        }

        // Places a piece on the board. The game knows whose turn it is so no need
        // to identify the player. Will also update whose turn it is.
        public void PlacePiece(int row, int col)
        {
            string pieceToPlace = this.isFirstPlayersTurn ?
                this.Player1.Piece :
                this.Player2.Piece;
            this.Board.PlacePiece(row, col, pieceToPlace);

            this.isFirstPlayersTurn = !this.isFirstPlayersTurn;
        }

        // Returns whether or not the specified move is valid.
        // A move is invalid if there is already a piece placed in the location or the move is off the board.
        public bool IsValidMove(int row, int col)
        {
            return
                row < this.Board.Pieces.GetLength(0) &&
                col < this.Board.Pieces.GetLength(1) &&
                string.IsNullOrWhiteSpace(this.Board.Pieces[row, col]);
        }

        public override string ToString()
        {
            return String.Format("(Id={0}, Player1={1}, Player2={2}, Board={3})",
                this.Id, this.Player1, this.Player2, this.Board);
        }
    }
}