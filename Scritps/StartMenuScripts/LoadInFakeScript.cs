using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadInFakeScript : MonoBehaviour {

    [Header("Refereces")]
    [SerializeField] private SpawnController spawner;
    [SerializeField] private GameObject otherChar;
    [SerializeField] private GameObject textObj;
    [SerializeField] private Text loadText;
    [SerializeField] private AimController playerAim;
    [SerializeField] private MovementController playerMovement;

    [Header("Load time")]
    [SerializeField] private float maxLoadTime;
    [SerializeField] private float minLoadTime;
    [SerializeField] private float playerConnectedWait;

    private void Start() {
        StartCoroutine(UpdateLoadingText());
        Invoke("ConnectPlayer", Random.Range(minLoadTime, maxLoadTime));
    }

    IEnumerator UpdateLoadingText() {

        while (true) {
            loadText.text = "Waiting for other player to connect.";
            yield return new WaitForSeconds(0.5f);

            loadText.text = "Waiting for other player to connect..";
            yield return new WaitForSeconds(0.5f);

            loadText.text = "Waiting for other player to connect...";
            yield return new WaitForSeconds(0.5f);

            loadText.text = "Waiting for other player to connect....";
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void ConnectPlayer() {
        StopAllCoroutines();
        loadText.text = "Player Connected";
        otherChar.SetActive(true);
        playerAim.enabled = false;
        playerMovement.enabled = false;
        StartCoroutine(StartGame());
        Time.timeScale = 0;
    }

    private IEnumerator StartGame() {
        yield return new WaitForSecondsRealtime(playerConnectedWait);
        Time.timeScale = 1;
        playerAim.enabled = true;
        playerMovement.enabled = true;
        textObj.SetActive(false);
        spawner.NewStart();
        yield return null;
    }

}
