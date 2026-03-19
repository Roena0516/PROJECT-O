using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class MenuManager : MonoBehaviour
{
    private bool isSet;

    private SettingsManager settingsManager;
    [SerializeField] private MenuAnimation _animator;

    [SerializeField] private TextMeshProUGUI playerNameText;

    // 간단한 인덱스 기반 메뉴 시스템
    [SerializeField] private List<TextMeshProUGUI> menuItems = new List<TextMeshProUGUI>();
    private int currentMenuIndex = 0;

    // 설정 관련 기능 제거
    /*
    private void Awake()
    {
        action = new MainInputAction();

        LineActions.Add(action.Temp.Line1Action);
        LineActions.Add(action.Temp.Line2Action);
        LineActions.Add(action.Temp.Line3Action);
        LineActions.Add(action.Temp.Line4Action);
    }
    */

    private void Update()
    {
        // 위 화살표 또는 W 키
        if (Keyboard.current.upArrowKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame)
        {
            ChangeMenuIndex(-1);
        }
        // 아래 화살표 또는 S 키
        else if (Keyboard.current.downArrowKey.wasPressedThisFrame || Keyboard.current.sKey.wasPressedThisFrame)
        {
            ChangeMenuIndex(1);
        }
        // Enter 키로 선택
        else if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            SelectCurrentMenu();
        }
    }

    /// <summary>
    /// 메뉴 인덱스 변경 (위/아래 이동)
    /// </summary>
    private void ChangeMenuIndex(int direction)
    {
        if (menuItems.Count == 0) return;

        // 이전 선택 메뉴의 밑줄 제거
        if (currentMenuIndex >= 0 && currentMenuIndex < menuItems.Count)
        {
            menuItems[currentMenuIndex].fontStyle &= ~FontStyles.Underline;
        }

        // 인덱스 변경 (순환)
        currentMenuIndex += direction;
        if (currentMenuIndex < 0)
        {
            currentMenuIndex = menuItems.Count - 1;
        }
        else if (currentMenuIndex >= menuItems.Count)
        {
            currentMenuIndex = 0;
        }

        // 새로운 선택 메뉴에 밑줄 추가
        menuItems[currentMenuIndex].fontStyle |= FontStyles.Underline;
    }

    /// <summary>
    /// 현재 선택된 메뉴 실행
    /// </summary>
    private void SelectCurrentMenu()
    {
        if (!isSet || menuItems.Count == 0) return;

        SelectMenu(currentMenuIndex);
    }

    private void SetPlayerInfos()
    {
        // 오프라인 모드: 로컬 플레이어 이름만 표시
        string playerName = settingsManager.GetLocalPlayerName();

        if (playerNameText != null)
        {
            playerNameText.text = string.IsNullOrEmpty(playerName) ? "Player" : playerName;
        }
    }

    /// <summary>
    /// 메뉴 선택 (인덱스 기반: 0=Settings, 1=FreePlay, 2=Rankings)
    /// </summary>
    private void SelectMenu(int index)
    {
        if (!isSet) return;

        isSet = false;

        switch (index)
        {
            case 0: // Settings
                Debug.Log("Settings selected");
                _animator.FadeIn(onComplete: () =>
                {
                    SceneManager.LoadSceneAsync("Settings");
                });
                break;

            case 1: // FreePlay
                Debug.Log("FreePlay selected");
                _animator.FadeIn(onComplete: () =>
                {
                    SceneManager.LoadSceneAsync("FreePlay");
                });
                break;

            case 2: // Rankings
                Debug.Log("Rankings selected");
                _animator.FadeIn(onComplete: () =>
                {
                    SceneManager.LoadSceneAsync("Rankings");
                });
                break;

            default:
                isSet = true; // 잘못된 인덱스면 다시 선택 가능하도록
                break;
        }
    }

    // 설정 관련 기능 제거
    /*
    private void SetSettingsPanel()
    {
        settingsPanel.SetActive(true);
        musicDelayValue.text = $"{settingsManager.settings.sync}ms";

        for (int i = 0; i < 4; i++)
        {
            RaneButtonText[i].text = $"{settingsManager.LineActions[i].bindings[0].ToDisplayString()}";
        }
    }

    public void ChangeSync(float duration)
    {
        sync += duration;
        musicDelayValue.text = $"{sync}ms";
    }

    public void SetKeyBindsInput(int rane)
    {
        Debug.Log(rane);
        settedButtonInputRane = rane;
        Rebind(settedButtonInputRane);
    }

    private void Rebind(int rane)
    {
        LineActions[rane - 1].Disable();
        LineActions[rane - 1].PerformInteractiveRebinding()
        .WithControlsExcluding("Mouse")
        .OnComplete(operation => // 리바인딩 완료 시 실행
                {
            Debug.Log($"{LineActions[rane - 1].bindings[0].effectivePath}");
            operation.Dispose(); // 메모리 해제
                    LineActions[rane - 1].Enable(); // 다시 활성화
                    RaneButtonText[rane - 1].text = $"{LineActions[rane - 1].bindings[0].ToDisplayString()}";
        })
        .Start(); // 리바인딩 시작
        settedButtonInputRane = 0;
    }

    public void SetAutoPlay(bool setted)
    {
        settingsManager.isAutoPlay = setted;
    }

    //public void SetToKR(bool setted)
    //{
    //    settingsManager.isKR = setted;
    //}

    public void SaveSettingsData()
    {
        for (int i = 0; i < 4; i++)
        {
            settingsManager.LineActions[i].ApplyBindingOverride(LineActions[i].bindings[0].effectivePath);
        }

        settingsManager.SetKeyBinds(new()
        {
#if UNITY_STANDALONE_OSX
            $"{LineActions[0].bindings[0].effectivePath}",
            $"{LineActions[1].bindings[0].effectivePath}",
            $"{LineActions[2].bindings[0].effectivePath}",
            $"{LineActions[3].bindings[0].effectivePath}"
#elif UNITY_STANDALONE_WIN
            $"{LineActions[0].bindings[0].ToDisplayString()}",
            $"{LineActions[1].bindings[0].ToDisplayString()}",
            $"{LineActions[2].bindings[0].ToDisplayString()}",
            $"{LineActions[3].bindings[0].ToDisplayString()}"
#endif
        });

        settingsManager.SetSync($"{sync}");
        //settingsManager.SetToKR(settingsManager.isKR);

        settingsManager.SaveSettings();
    }
    */

    private void Start()
    {
        isSet = true;
        settingsManager = SettingsManager.Instance;

        // 로컬 플레이어 정보 표시
        SetPlayerInfos();

        // 메뉴 초기화: 첫 번째 항목에 밑줄 추가
        InitializeMenu();
    }

    /// <summary>
    /// 메뉴 초기화 및 첫 번째 항목 선택
    /// </summary>
    private void InitializeMenu()
    {
        if (menuItems.Count == 0)
        {
            Debug.LogWarning("[MenuManager] menuItems 리스트가 비어있습니다. Inspector에서 메뉴 항목을 할당해주세요.");
            return;
        }

        currentMenuIndex = 0;

        // 모든 메뉴 항목의 밑줄 제거
        foreach (var item in menuItems)
        {
            if (item != null)
            {
                item.fontStyle &= ~FontStyles.Underline;
            }
        }

        // 첫 번째 메뉴 항목에 밑줄 추가
        menuItems[currentMenuIndex].fontStyle |= FontStyles.Underline;
    }
}
