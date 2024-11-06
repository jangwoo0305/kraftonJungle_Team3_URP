using Michsky.UI.Dark;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class LoadingManager : MonoBehaviour
{
    [SerializeField] ModalWindowManager loadingPanel;
    [SerializeField] GameObject introCanvas;

    public static LoadingManager instance;

    public bool isAniEnded = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadingStart(IEnumerator coroutine)
    {
        StartCoroutine(ExecuteCoroutineWithCallback(coroutine));
    }

    private IEnumerator ExecuteCoroutineWithCallback(IEnumerator coroutine)
    {
        isAniEnded = false;
        loadingPanel.ModalWindowIn();

        yield return StartCoroutine(coroutine);

        yield return new WaitForSeconds(1.5f);
        loadingPanel.ModalWindowOut();

        while(isAniEnded) yield return null;

        Instantiate(introCanvas);
    }

    public void LoadMainScene()
    {
        LoadingStart(LoadMainSceneCoroutine());
    }
    IEnumerator LoadMainSceneCoroutine()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(0); // 씬 로딩
        asyncLoad.allowSceneActivation = true;

        // 로딩이 완료될 때까지 대기
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        while (PhotonNetwork.NetworkClientState
        != Photon.Realtime.ClientState.JoinedLobby)
        {
            yield return null;
        }
    }
}
