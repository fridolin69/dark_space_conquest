using UnityEngine;
using System.Collections;

public interface I_StateSynchronisationObserver
{
    void OnPlayerReadyChanged(Player player, GameState gameState);
}