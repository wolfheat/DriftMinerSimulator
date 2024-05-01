using UnityEngine;

public enum GameStates{Running,Paused}

public static class GameState
{
	public static GameStates state;

    public static bool IsPaused { get { return state == GameStates.Paused; } }

}
