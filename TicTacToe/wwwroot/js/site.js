const hubConn = new signalR.HubConnectionBuilder()
    .withUrl("/game")
    .build();
hubConn.serverTimeoutInMilliseconds = 100000;
hubConn
    .start()
    .then(() => console.log('Connection started!'))
    .catch(
        err => console.log('Error while establishing connection :('));

Handlebars.registerHelper("debug", function () {
    console.log("Current Context");
    console.log(this);
});

// The ID of this player
var playerId;

// Prevent players from joining again
hubConn.on("playerJoined", function (player) {
    playerId = player.Id;
    $('#usernameGroup').removeClass("has-error");
    disableInput();
});

// The username is already taken
hubConn.on("usernameTaken", function () {
    $('#status').html("The username is already taken.");
    $('#usernameGroup').addClass("has-error");
});

// The opponent left so game is over and allow player to find a new game
hubConn.on("opponentLeft", function () {
    $('#status').html("Opponent has left. Game over.");
    endGame();
});

// Notify player that they are in a waiting pool for another opponent
hubConn.on("waitingList", function (player) {
    $('#status').html("Waiting for an opponent.");
});

// Starts a new game by displaying the board and showing whose turn it is
hubConn.on("start", function (gameJson) {
    let game = JSON.parse(gameJson)
    buildBoard(game.Board);
    var opponent = getOpponent(game);
    displayOpponent(opponent);
    displayTurn(game.WhoseTurn, true);
});

// Handles the case where a user tried to place a piece not on their turn
hubConn.on("notPlayersTurn", function () {
    $('#status').html("Please wait your turn.");
});

// Display a message if move is not valid
hubConn.on("notValidMove", function () {
    $('#status').html("Please choose another location.");
});

// A piece has been placed on the board
hubConn.on("piecePlaced", function (row, col, piece) {
    $('#pos-' + row + '-' + col).html(piece);
});

// send the whole game object, render the board and update in one method
hubConn.on("updateTurn", function (gameJSON) {
    let game = JSON.parse(gameJSON)
    displayTurn(game.WhoseTurn);
});

// Handle the tie game - game over scenario
hubConn.on("tieGame", function () {
    $('#status').html("Tie!");
    endGame();
});

// Handle the tie game - game over scenario
hubConn.on("winner", function (playerName) {
    $('#status').html("Winner: " + playerName);
    endGame();
});

// CLIENT BEHAVIORS
// Call server to find a game if button is clicked
$('#findGame').click(function () {
    hubConn.invoke("FindGame", $('#username').val())
        .then(() => console.log('Send started!'))
        .catch(err => console.log('Error while establishing connection :(', err));
});

//pressing 'Enter' will automatically click Find Game button
$('#username').keypress(function (e) {
    if ((e.which && e.which == 13) || (e.keyCode && e.keyCode == 13)) {
        $('#findGame').click();
        return false;
    }

    return true;
});

// Disables username and find game inputs
function disableInput() {
    $('#username').attr('disabled', 'disabled');
    $('#findGame').attr('disabled', 'disabled');
};

// Game over business logic should disable board button handlers and 
// allow player to join a new game
function endGame() {
    // Remove click handlers from board positions
    $('td[id^=pos-]').off('click');
    enableInput();
};

// Display whose turn it is
function displayTurn(playersTurn, isDisplayingOpponent) {
    var turnMessage = "";
    if (playerId == playersTurn.Id) {
        turnMessage = "Your turn";
    } else {
        turnMessage = playersTurn.Name + "\'s turn";
    }

    // Do not overwrite opponent's name if it is being displayed
    if (isDisplayingOpponent) {
        $('#status').html($('#status').html() + turnMessage);
    } else {
        $('#status').html(turnMessage);
    }
};

// Displays the opponents name
function displayOpponent(opponent) {
    $('#status').html("You are playing against " + opponent.Name + "<br />");
};

// Build and display the board
function buildBoard(board) {
    var template = Handlebars.compile($('#board-template').html());
    $('#board').html(template(board));

    // attach click handlers to each position
    $('td[id^=pos-]').click(function (e) {
        e.preventDefault();
        var id = this.id; 
        var parts = id.split("-"); 
        var row = parts[1];
        var col = parts[2];
        hubConn.invoke("PlacePiece", row, col)
            .then(() => console.log('Send started!'))
            .catch(err => console.log('Error while establishing connection :(', err));
    });
};

// Retrieves the opponent player from the game
function getOpponent(game) {
    if (playerId == game.Player1.Id) {
        return game.Player2;
    } else {
        return game.Player1;
    }
};