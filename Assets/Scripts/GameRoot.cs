using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utilities;

namespace UnknownWorldsTest
{
    public class GameRoot : SingletonComponent<GameRoot>
    {
        [SerializeField] private GameObject MainMenuDialog;
        [SerializeField] private GameObject GameOverlayDialog;
        [SerializeField] private TextMeshProUGUI DebugButtonText;
        [SerializeField] private TextMeshProUGUI GameLevelText;

        public bool ShowDebug { get; private set; }
        private int curLevelIndex = -1;
        
        private void Start()
        {
            MainMenuDialog.SetActive(true);
            GameOverlayDialog.SetActive(false);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                Utils.QuitApplication();
#if !UNITY_EDITOR
            
#endif
        }

        public void OnStartClicked()
        {
            MainMenuDialog.SetActive(false);
            StartCoroutine(LoadLevel(1));
        }

        public void OnShowDebugClicked()
        {
            ShowDebug = !ShowDebug;
            DebugButtonText.text = ShowDebug ? "Hide Debug" : "Show Debug";
        }

        public IEnumerator LoadLevel(int index)
        {
            if (curLevelIndex > -1)
                SceneManager.UnloadSceneAsync(curLevelIndex);

            var asyncOp = SceneManager.LoadSceneAsync(index, LoadSceneMode.Additive);

            while (!asyncOp.isDone)
                yield return null;
            
            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(index));
            GridBuilder.Instance.LoadGrid();
            curLevelIndex = index;
            
            GameOverlayDialog.SetActive(true);
            GameLevelText.text = $"Level - {index}";
        }
    }
}
