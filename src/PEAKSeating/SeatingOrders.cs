namespace PEAKSeating;

public enum SeatingOrders
{
    // Vanilla uses the order at which player joined the game
    VANILLA,
    // The closest player to the Helicopter is first and so on...
    CLOSEST,
    // Completely random
    RANDOM
}