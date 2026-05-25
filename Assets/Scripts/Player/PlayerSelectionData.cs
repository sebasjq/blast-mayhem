using UnityEngine;

// Sirve para guardar datos entre escenas. Además, es static porque se requiere acceder
// a estos datos desde cualquier script (en este caso PlayerSpawnManager y CharacterSelectionManager) sin necesidad de crear una instancia
public static class PlayerSelectionData
{
    public static GameObject player1Prefab;
    public static GameObject player2Prefab;

    public static Sprite player1Sprite;
    public static Sprite player2Sprite;
}
