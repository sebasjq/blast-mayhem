// Los enums se utilizan para definir un conjunto de constantes con nombre.
// En este caso, se definen tres enums: BombType, BombState y PickupType.
//
// Cada uno de estos enums representa diferentes tipos de bombas,
// estados de las bombas y tipos de objetos que se pueden recoger en el juego.

public enum BombType // Define los diferentes tipos de bombas que existen en el juego
{
    Normal,
    Gravity,
    String,
    Sticky,
}

public enum BombState // Define los diferentes estados en los que una bomba puede estar durante su ciclo de vida
{
    Pickup,
    Thrown,
    Resting,
    Exploded,
}

public enum PickupType // Define los diferentes tipos de objetos que el jugador puede recoger en el juego
{
    Health,
    GravityPickup,
    StringPickup,
    StickyPickup,
}

