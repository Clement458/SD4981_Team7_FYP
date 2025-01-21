using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles displaying the winners of the game
/// </summary>
public class EndGame : MonoBehaviour
{
    [Tooltip("Displays the top three winners.")]

    /// <summary>
    /// Shows the list of winners.
    /// </summary>
    /// <param name="winners">The list of winning players</param>
    public void Show(List<HostPlayerScript> winners)
    {
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Hides the end game display.
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}