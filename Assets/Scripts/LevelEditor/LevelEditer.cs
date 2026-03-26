using UnityEngine;
using System.IO;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System.Runtime.InteropServices;
using System;
using FMODUnity;
using FMOD.Studio;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
//using SFB;

public class LevelEditer : MonoBehaviour
{
    public UnityEvent OnNoteHit = new UnityEvent();

    public int currentMusicTime = 0;

    public EventInstance eventInstance;
    public EventInstance hitSoundInstance;

    public SaveManager saveManager;

    public GameObject normalPrefab;
    public GameObject bellPrefab;
    public GameObject longPrefab;

    public GameObject notesFolder;
    public GameObject gridFolder;

    [SerializeField] private GameObject buttonsFolder;
    [SerializeField] private GameObject inputsFolder;
    [SerializeField] private GameObject fileLoadFolder;

    public GameObject addIndicator;
    public GameObject removeIndicator;

    public GameObject settingsPanel;

    public RectTransform rectTransform;
    public Canvas canvasComponent;

    public string selectedBeat;

    private Coroutine currentMoveSliderer;

    public int madi;
    public int madi2;
    public int madi3;

    private bool isRemoving;
    public bool isMusicPlaying;

    private float scrollSpeed;

    public float BPM;
    public string artist;
    public string title;
    public string fileName;
    public string eventName;
    public float level;
    public string difficulty;

    //private string[] paths;

    public string noteType;

    private GameObject beat13;
    private GameObject beat14;
    private GameObject beat16;
    private GameObject beat18;
    private GameObject beat112;
    private GameObject beat116;
    private GameObject beat124;
    private GameObject beat132;
    private GameObject beat148;
    private GameObject beat164;

    public GameObject canvas;

    public TMP_Dropdown dropdown;

    public List<GameObject> beats13;
    public List<GameObject> beats14;
    public List<GameObject> beats16;
    public List<GameObject> beats18;
    public List<GameObject> beats112;
    public List<GameObject> beats116;
    public List<GameObject> beats124;
    public List<GameObject> beats132;

    public GameObject gridPrefab;
    public Transform gridContainer;
    public Camera uiCamera;
    public int maxVisibleRows = 40;
    public float cellHeight = 160f; // ?? beat prefab?? ????

    private List<GameObject> gridPool = new();
    private float totalHeight;
    public float scrollY;
    public int scrollYInt;
    private int currentTopIndex = -1;

    private Dictionary<string, GameObject> beatPrefabMap;

    public static LevelEditer Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        beat13 = Resources.Load<GameObject>("Prefabs/LevelEditor/Beats/Beat13");
        beat14 = Resources.Load<GameObject>("Prefabs/LevelEditor/Beats/Beat14");
        beat16 = Resources.Load<GameObject>("Prefabs/LevelEditor/Beats/Beat16");
        beat18 = Resources.Load<GameObject>("Prefabs/LevelEditor/Beats/Beat18");
        beat112 = Resources.Load<GameObject>("Prefabs/LevelEditor/Beats/Beat112");
        beat116 = Resources.Load<GameObject>("Prefabs/LevelEditor/Beats/Beat116");
        beat124 = Resources.Load<GameObject>("Prefabs/LevelEditor/Beats/Beat124");
        beat132 = Resources.Load<GameObject>("Prefabs/LevelEditor/Beats/Beat132");
        beat148 = Resources.Load<GameObject>("Prefabs/LevelEditor/Beats/Beat148");
        beat164 = Resources.Load<GameObject>("Prefabs/LevelEditor/Beats/Beat164");

        beatPrefabMap = new Dictionary<string, GameObject>
        {
            { "1_3", beat13 },
            { "1_4", beat14 },
            { "1_6", beat16 },
            { "1_8", beat18 },
            { "1_12", beat112 },
            { "1_16", beat116 },
            { "1_24", beat124 },
            { "1_32", beat132 },
            { "1_48", beat148 },
            { "1_64", beat164 }
        };
    }

    private void SetHitSoundInstance()
    {
        hitSoundInstance = RuntimeManager.CreateInstance($"event:/tamb");

        hitSoundInstance.set3DAttributes(RuntimeUtils.To3DAttributes(gameObject));

        hitSoundInstance.setVolume(1f);
        hitSoundInstance.start();
    }

    private void Start()
    {
        selectedBeat = "1_4";
        cellHeight = 160f;

        CreateGridPool(4);
        totalHeight = 9217 * cellHeight;

        dropdown.onValueChanged.AddListener(OnDropdownValueChanged);

        SetNoteType("Normal");
        DeselectNote();

        isRemoving = false;
        isMusicPlaying = false;

        scrollSpeed = 10f;

        BPM = 120f;

        madi = 192;
        madi2 = 288;
        madi3 = madi + madi2;

        SetHitSoundInstance();
    }

    private void ButtonClickHandler(int position, int beat, Transform buttonT)
    {
        canvas.transform.localScale = Vector3.one;

        float realBeat = 0f;

        int index = currentTopIndex + gridPool.IndexOf(buttonT.gameObject);

        if (index != -1)
        {
            realBeat = (2f / (float)beat) * (index);
        }

        Debug.Log($"Position : {position}, Beat : {realBeat} 1/{beat}, Type : {noteType}");

        DeselectNote();

        if (isRemoving)
        {
            saveManager.notes.Remove(saveManager.notes.Find(note => note.beat == realBeat && note.position == position));
            foreach (Transform child in notesFolder.transform)
            {
                LevelEditerNoteManager noteManager = child.GetComponent<LevelEditerNoteManager>();
                if (noteManager.noteClass.beat == realBeat && noteManager.noteClass.position == position)
                {
                    Destroy(child.gameObject);
                }
            }
            return;
        }

        // 이미 같은 위치에 노트가 있는지 확인
        NoteClass existingNote = saveManager.notes.Find(note => note.beat == realBeat && note.position == position);
        if (existingNote != null)
        {
            // 해당 노트를 찾아서 선택
            foreach (Transform child in notesFolder.transform)
            {
                LevelEditerNoteManager noteManager = child.GetComponent<LevelEditerNoteManager>();
                if (noteManager != null && noteManager.noteClass.beat == realBeat && noteManager.noteClass.position == position)
                {
                    SelectNote(noteManager);
                    Debug.Log($"Selected existing note at Position {position}, Beat {realBeat}");
                    return;
                }
            }
            Debug.LogWarning($"Note exists in data but GameObject not found at Position {position}, Beat {realBeat}");
            return;
        }

        float positionX = 0f;
        if (position == 1)
        {
            positionX = -158f;
        }
        if (position == 2)
        {
            positionX = 158f / 3f * -1f;
        }
        if (position == 3f)
        {
            positionX = 158f / 3f;
        }
        if (position == 4f)
        {
            positionX = 158f;
        }

        float positionY = buttonT.position.y;

        //Debug.Log(positionY);

        GameObject prefab = noteType switch
        {
            "Normal" => normalPrefab,
            "Bell" => bellPrefab,
            "Long" => longPrefab,
            _ => null
        };

        GameObject instantiateObject = Instantiate(prefab, new Vector3(positionX, positionY, 0f), Quaternion.identity, notesFolder.transform);
        LevelEditerNoteManager levelEditerNoteManager = instantiateObject.GetComponent<LevelEditerNoteManager>();
        levelEditerNoteManager.ms = (60f / BPM * 1000f) * realBeat;
        levelEditerNoteManager.noteClass.position = position;
        levelEditerNoteManager.noteClass.beat = realBeat;
        levelEditerNoteManager.noteClass.type = noteType.ToLower();

        // 롱노트는 length를 4로 초기화
        if (noteType.ToLower() == "long")
        {
            levelEditerNoteManager.noteClass.length = 4f;
            UpdateLongNoteVisualLength(levelEditerNoteManager, 4f);
        }

        // Bell 노트는 width를 1.0으로 초기화
        if (noteType.ToLower() == "bell")
        {
            levelEditerNoteManager.noteClass.width = 1f;
        }

        saveManager.notes.Add(levelEditerNoteManager.noteClass);
    }

    private void OnDropdownValueChanged(int index)
    {
        selectedBeat = dropdown.options[index].text;
        int beatNum = 4;
        switch (dropdown.options[index].text)
        {
            case "1/3":
                selectedBeat = "1_3";
                beatNum = 3;
                break;
            case "1/4":
                selectedBeat = "1_4";
                beatNum = 4;
                break;
            case "1/6":
                selectedBeat = "1_6";
                beatNum = 6;
                break;
            case "1/8":
                selectedBeat = "1_8";
                beatNum = 8;
                break;
            case "1/12":
                selectedBeat = "1_12";
                beatNum = 12;
                break;
            case "1/16":
                selectedBeat = "1_16";
                beatNum = 16;
                break;
            case "1/24":
                selectedBeat = "1_24";
                beatNum = 24;
                break;
            case "1/32":
                selectedBeat = "1_32";
                beatNum = 32;
                break;
            case "1/48":
                selectedBeat = "1_48";
                beatNum = 48;
                break;
            case "1/64":
                selectedBeat = "1_64";
                beatNum = 64;
                break;
        }

        GameObject beatPrefab = selectedBeat switch
        {
            "1_3" => beat13,
            "1_4" => beat14,
            "1_6" => beat16,
            "1_8" => beat18,
            "1_12" => beat112,
            "1_16" => beat116,
            "1_24" => beat124,
            "1_32" => beat132,
            _ => beat14
        };

        cellHeight = beatPrefab.transform.Find("Btn 1").gameObject.GetComponent<RectTransform>().rect.height;
        totalHeight = 9217 * cellHeight;

        CreateGridPool(beatNum);

        canvas.transform.localScale = Vector3.one;

        scrollY = Mathf.Clamp(scrollY, 0, totalHeight - cellHeight * maxVisibleRows);
        gridContainer.localPosition = new Vector3(0, -scrollY - 500, 0);

        int newTopIndex = Mathf.FloorToInt(scrollY / cellHeight);
        if (newTopIndex != currentTopIndex)
        {
            currentTopIndex = newTopIndex;
            UpdateVisibleGrids(newTopIndex);
        }
    }

    private void InputHandler()
    {
        canvas.transform.localScale = Vector3.one;

        if (Input.GetKey(KeyCode.W))
        {
            scrollY += 1000f * Time.deltaTime;
            CalcCurrentMusicTime();
        }
        if (Input.GetKey(KeyCode.S))
        {
            scrollY -= 1000f * Time.deltaTime;
            CalcCurrentMusicTime();
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            scrollY += 1000f * scroll;
            CalcCurrentMusicTime();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetIsRemoving("Add");
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SetIsRemoving("Remove");
        }

        if (Input.GetKeyDown(KeyCode.E) && canvas.activeSelf)
        {
            isMusicPlaying = !isMusicPlaying;
            eventInstance.setPaused(!isMusicPlaying);

            OnNoteHit.Invoke();

            if (currentMoveSliderer == null)
            {
                currentMoveSliderer = StartCoroutine(MoveSlider());
            }
            else
            {
                StopCoroutine(currentMoveSliderer);
                currentMoveSliderer = null;
            }
        }
    }

    private void ScrollHandler()
    {
        scrollY = Mathf.Clamp(scrollY, 0, totalHeight - cellHeight * maxVisibleRows);
        gridContainer.localPosition = new Vector3(0, -scrollY - 500f, 0);

        notesFolder.transform.position = new Vector3(0, -scrollY + 280f, 0);

        int newTopIndex = Mathf.FloorToInt(scrollY / cellHeight);
        if (newTopIndex != currentTopIndex)
        {
            currentTopIndex = newTopIndex;
            UpdateVisibleGrids(newTopIndex);
        }
    }

    private void Update()
    {
        InputHandler();

        ScrollHandler();

        UpdateMusicTime();
        if (settingsPanel.activeSelf)
        {
            UpdateInputFieldValue();
        }
    }

    private void CreateGridPool(int beatNum)
    {
        gridPool.Clear(); // ???? ?? ????

        // ?????? ?????? ???????? ????
        foreach (Transform child in gridContainer)
        {
            Destroy(child.gameObject);
        }

        if (!beatPrefabMap.TryGetValue(selectedBeat, out GameObject prefab))
        {
            Debug.LogError($"?????? Beat ??????({selectedBeat})?? ???????? ???????? ????????.");
            return;
        }

        for (int i = 0; i < maxVisibleRows + 2; i++)
        {
            GameObject go = Instantiate(prefab, gridContainer);
            go.SetActive(false);

            // ?? ????(1~4)?? ?????? ?????? ??????
            for (int j = 1; j <= 4; j++)
            {
                Transform buttonTransform = go.transform.Find($"Btn {j}");
                if (buttonTransform != null)
                {
                    int pos = j; // ?????? ???? ????
                    Button btn = buttonTransform.GetComponent<Button>();
                    if (btn != null)
                    {
                        btn.onClick.RemoveAllListeners(); // ???? ?????? ?????? ??????
                        btn.onClick.AddListener(() => ButtonClickHandler(pos, beatNum, go.transform));
                    }
                    else
                    {
                        Debug.LogWarning($"Button component not found in Btn {j}");
                    }
                }
                else
                {
                    Debug.LogWarning($"Btn {j} not found in prefab.");
                }
            }

            gridPool.Add(go);
        }
    }

    private void UpdateVisibleGrids(int topIndex)
    {
        for (int i = 0; i < gridPool.Count; i++)
        {
            int beatIndex = topIndex + i;
            if (beatIndex >= 9217)
            {
                gridPool[i].SetActive(false);
                continue;
            }
            gridPool[i].SetActive(true);
            gridPool[i].transform.localPosition = new Vector3(0, beatIndex * cellHeight, 0);
        }
    }

    public TMP_InputField BPMInput;
    public TMP_InputField artistInput;
    public TMP_InputField titleInput;
    public TMP_InputField eventNameInput;
    public TMP_InputField difficultyInput;
    public TMP_InputField levelInput;

    private void UpdateInputFieldValue()
    {
        BPMInput.text = $"{BPM}";
        artistInput.text = $"{artist}";
        titleInput.text = $"{title}";
        eventNameInput.text = $"{eventName}";
        difficultyInput.text = $"{difficulty}";
        levelInput.text = $"{level}";
    }

    private void UpdateMusicTime()
    {
        if (eventInstance.isValid())
        {
            eventInstance.getTimelinePosition(out currentMusicTime);
        }
    }

    private void SetMusicTime(int musicTime)
    {
        currentMusicTime = musicTime;
        //Debug.Log(currentMusicTime);
        eventInstance.setTimelinePosition(currentMusicTime);
    }

    private void CalcCurrentMusicTime()
    {
        canvas.transform.localScale = Vector3.one;
        float musicTime = scrollY / scrollSpeed / 2f;
        SetMusicTime((int)musicTime);
    }

    IEnumerator MoveSlider()
    {
        while (isMusicPlaying)
        {
            canvas.transform.localScale = Vector3.one;
            UpdateMusicTime();
            //gridFolder.transform.Translate(scrollSpeed * Time.deltaTime * Vector2.down);
            scrollY = (scrollSpeed * currentMusicTime) * 2f;
            scrollY = Mathf.Clamp(scrollY, 0, totalHeight - cellHeight * maxVisibleRows);
            gridContainer.localPosition = new Vector3(0, -scrollY - 500f, 0);

            notesFolder.transform.position = new Vector3(0, -scrollY + 280f, 0);

            int newTopIndex = Mathf.FloorToInt(scrollY / cellHeight);
            if (newTopIndex != currentTopIndex)
            {
                currentTopIndex = newTopIndex;
                UpdateVisibleGrids(newTopIndex);
            }

            yield return null;
        }

        yield break;
    }

    public void SaveLevel()
    {
        saveManager.SaveToJson(Path.Combine(Application.streamingAssetsPath, $"{artist}-{title}.json"), BPM, artist, title, eventName, level, difficulty);
    }

    public void LoadLevel()
    {
        fileLoadFolder.SetActive(false);
        LoadFromJson(Path.Combine(Application.streamingAssetsPath, $"{fileName}.json"));
    }

    private void LoadFromJson(string filePath)
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);

            NotesContainer container = JsonUtility.FromJson<NotesContainer>(json);
            BPM = container.info.bpm;
            artist = container.info.artist;
            title = container.info.title;
            difficulty = container.info.difficulty;
            level = container.info.level;
            eventName = container.info.eventName;

            SetEventInstance(eventName);
            SetScrollSpeed();

            saveManager.notes = container.notes;

            Debug.Log("Chart loaded successfully!");

            scrollY = 0;
            gridContainer.localPosition = new Vector3(0, -scrollY - 500f, 0);

            int newTopIndex = Mathf.FloorToInt(scrollY / cellHeight);
            if (newTopIndex != currentTopIndex)
            {
                currentTopIndex = newTopIndex;
                UpdateVisibleGrids(newTopIndex);
            }

            PlaceNotesFromLoadedFile();
        }
        else
        {
            Debug.LogError("File not found at: " + filePath);
        }
    }

    private void PlaceNotesFromLoadedFile()
    {
        foreach (NoteClass note in saveManager.notes)
        {
            canvas.transform.localScale = Vector3.one;

            float positionX = 0f;

            // Bell/Hold 노트는 float position을 사용하여 연속적인 위치 계산
            if (note.type.ToLower() == "hold" || note.type.ToLower() == "bell")
            {
                // position 1 = -158f, position 4 = 158f
                // 선형 보간: positionX = -158 + (position - 1) * (316 / 3)
                positionX = -158f + (note.position - 1f) * (316f / 3f);
            }
            else
            {
                // Normal, Long 노트는 정수 position 사용
                if (note.position == 1)
                {
                    positionX = -158f;
                }
                else if (note.position == 2)
                {
                    positionX = 158f / 3f * -1f;
                }
                else if (note.position == 3f)
                {
                    positionX = 158f / 3f;
                }
                else if (note.position == 4f)
                {
                    positionX = 158f;
                }
            }

            float positionY = -211f + (320f * note.beat);
            notesFolder.transform.position = new Vector3(0, 280f, 0);

            GameObject prefab = note.type switch
            {
                "normal" => normalPrefab,
                "hold" => bellPrefab,
                "bell" => bellPrefab,
                "long" => longPrefab,
                _ => null
            };

            GameObject instantiateObject = Instantiate(prefab, new Vector3(positionX, positionY, 0f), Quaternion.identity, notesFolder.transform);
            LevelEditerNoteManager levelEditerNoteManager = instantiateObject.GetComponent<LevelEditerNoteManager>();
            levelEditerNoteManager.ms = (60f / BPM * 1000f) * note.beat;
            levelEditerNoteManager.noteClass.position = note.position;
            levelEditerNoteManager.noteClass.beat = note.beat;
            levelEditerNoteManager.noteClass.type = note.type;

            // Long 노트의 length 적용
            if (note.type.ToLower() == "long")
            {
                levelEditerNoteManager.noteClass.length = note.length;
                UpdateLongNoteVisualLength(levelEditerNoteManager, note.length);
            }

            // Bell/Hold 노트의 width 적용
            if (note.type.ToLower() == "hold" || note.type.ToLower() == "bell")
            {
                levelEditerNoteManager.noteClass.width = note.width;
                UpdateBellNoteVisualWidth(levelEditerNoteManager, note.width);
            }
        }
    }

    [System.Serializable]
    private class NotesContainer
    {
        public SongInfoClass info;
        public List<NoteClass> notes;
    }

    public void SetLevel(string inputed)
    {
        float.TryParse(inputed, out level);
    }

    public void SetDifficulty(string inputed)
    {
        difficulty = inputed;
    }

    public void SetFileName(string inputed)
    {
        fileName = inputed;
    }

    public void SetBPM(string inputed)
    {
        float.TryParse(inputed, out BPM);
        SetScrollSpeed();
    }

    private void SetScrollSpeed()
    {
        scrollSpeed = 160f / (1000f * 60f / BPM);
    }

    public void SetNoteType(string type)
    {
        Debug.Log($"Note type changed from: {noteType} to: {type}");
        if (!string.IsNullOrEmpty(noteType))
        {
            Transform previousButton = buttonsFolder.transform.Find(noteType);
            if (previousButton != null)
            {
                previousButton.GetComponent<Image>().color = Color.white;
            }
        }
        noteType = type;
        buttonsFolder.transform.Find(type).GetComponent<Image>().color = Color.yellow;
    }

    public void SetArtist(string inputed)
    {
        artist = inputed;
    }

    public void SetTitle(string inputed)
    {
        title = inputed;
    }

    public void SetEventName(string inputed)
    {
        eventName = inputed;
        SetEventInstance(eventName);
    }

    private void SetEventInstance(string name)
    {
        eventInstance = RuntimeManager.CreateInstance($"event:/{name}");

        eventInstance.set3DAttributes(RuntimeUtils.To3DAttributes(gameObject));

        eventInstance.setVolume(0.5f);
        eventInstance.start();

        eventInstance.setTimelinePosition(0);
        eventInstance.setPaused(true);
    }

    //public void SetFilePath()
    //{
    //    paths = StandaloneFileBrowser.OpenFolderPanel("Select Folder", "", false);
    //    if (paths.Length > 0)
    //    {
    //        Debug.Log(paths[0]);
    //    }
    //}

    public void SetIsRemoving(string inputed)
    {
        if (inputed == "Remove")
        {
            isRemoving = true;
            removeIndicator.SetActive(true);
            addIndicator.SetActive(false);
        }

        if (inputed == "Add")
        {
            isRemoving = false;
            removeIndicator.SetActive(false);
            addIndicator.SetActive(true);
        }
    }

    public void ChangeToTestScene()
    {
        canvas.SetActive(false);

        SceneManager.LoadSceneAsync("InGame", LoadSceneMode.Additive);
        Scene testScene = SceneManager.GetSceneByName("InGame");
        if (testScene.IsValid() && testScene.isLoaded)
        {
            saveManager.notes.Sort((note1, note2) => note1.beat.CompareTo(note2.beat));
            SceneManager.SetActiveScene(testScene);
        }
    }

    public void OpenSettingsPanel()
    {
        settingsPanel.SetActive(!settingsPanel.activeSelf);
        if (settingsPanel.activeSelf)
        {

        }
    }

    // ========== 노트 세부 설정 기능 ==========

    // 현재 선택된 노트 (UI에서 클릭하여 선택)
    private LevelEditerNoteManager selectedNote;

    public void SelectNote(LevelEditerNoteManager note)
    {
        selectedNote = note;
        inputsFolder.SetActive(true);
        Transform inputsTransform = inputsFolder.transform;
        inputsTransform.Find("LengthInput").GetComponent<TMP_InputField>().text = $"{note.noteClass.length}";
        inputsTransform.Find("TickInput").GetComponent<TMP_InputField>().text = $"{note.noteClass.tick}";
        inputsTransform.Find("WidthInput").GetComponent<TMP_InputField>().text = $"{note.noteClass.width}";
        inputsTransform.Find("PositionInput").GetComponent<TMP_InputField>().text = $"{note.noteClass.position}";
        Debug.Log($"Note selected: Type={note.noteClass.type}, Beat={note.noteClass.beat}, Position={note.noteClass.position}");
    }

    public void DeselectNote()
    {
        selectedNote = null;
        inputsFolder.SetActive(false);
        Debug.Log("Note deselected");
    }

    // 롱노트 length 설정
    public void SetLongNoteLength(string inputed)
    {
        if (selectedNote == null)
        {
            Debug.LogWarning("No note selected!");
            return;
        }

        if (selectedNote.noteClass.type.ToLower() != "long")
        {
            Debug.LogWarning("Selected note is not a long note!");
            return;
        }

        if (float.TryParse(inputed, out float length))
        {
            selectedNote.noteClass.length = length;
            Debug.Log($"Long note length set to {length}");

            // SaveManager의 notes 리스트에서 해당 노트를 찾아 업데이트
            NoteClass note = saveManager.notes.Find(n =>
                n.beat == selectedNote.noteClass.beat &&
                n.position == selectedNote.noteClass.position &&
                n.type == selectedNote.noteClass.type);
            if (note != null)
            {
                note.length = length;
            }

            // 시각적 길이 업데이트 (RectTransform의 height 조절)
            UpdateLongNoteVisualLength(selectedNote, length);
        }
        else
        {
            Debug.LogWarning($"Invalid length value: {inputed}");
        }
    }

    // 롱노트 tick 설정
    public void SetLongNoteTick(string inputed)
    {
        if (selectedNote == null)
        {
            Debug.LogWarning("No note selected!");
            return;
        }

        if (selectedNote.noteClass.type.ToLower() != "long")
        {
            Debug.LogWarning("Selected note is not a long note!");
            return;
        }

        if (float.TryParse(inputed, out float tick))
        {
            selectedNote.noteClass.tick = tick;
            Debug.Log($"Long note tick updated: tick = {tick}");
        }
        else
        {
            Debug.LogWarning($"Invalid tick value: {inputed}");
        }
    }

    // 롱노트의 시각적 길이 업데이트
    private void UpdateLongNoteVisualLength(LevelEditerNoteManager note, float length)
    {
        RectTransform rectTransform = note.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            // 기본 height = 15, length에 비례하여 height 증가
            // length는 beat 단위이므로, beat당 픽셀 수를 곱해야 함
            // 1 beat = 320f (positionY 계산에서 사용한 값)
            float baseHeight = 15f;
            float heightPerBeat = 320f;
            float newHeight = baseHeight + (length * heightPerBeat);

            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, newHeight);
            Debug.Log($"Long note visual length updated: height = {newHeight}");
        }
    }

    // Bell 노트 width 설정
    public void SetBellNoteWidth(string inputed)
    {
        if (selectedNote == null)
        {
            Debug.LogWarning("No note selected!");
            return;
        }

        if (selectedNote.noteClass.type.ToLower() != "bell" && selectedNote.noteClass.type.ToLower() != "hold")
        {
            Debug.LogWarning("Selected note is not a bell/hold note!");
            return;
        }

        if (float.TryParse(inputed, out float width))
        {
            selectedNote.noteClass.width = width;
            Debug.Log($"Bell note width set to {width}");

            // SaveManager의 notes 리스트에서 해당 노트를 찾아 업데이트
            NoteClass note = saveManager.notes.Find(n =>
                n.beat == selectedNote.noteClass.beat &&
                n.position == selectedNote.noteClass.position &&
                n.type == selectedNote.noteClass.type);
            if (note != null)
            {
                note.width = width;
            }

            // 시각적 너비 업데이트 (RectTransform의 width 조절)
            UpdateBellNoteVisualWidth(selectedNote, width);
        }
        else
        {
            Debug.LogWarning($"Invalid width value: {inputed}");
        }
    }

    // Bell 노트의 시각적 너비 업데이트
    private void UpdateBellNoteVisualWidth(LevelEditerNoteManager note, float width)
    {
        RectTransform rectTransform = note.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            // 기본 width = 95, width 속성에 비례하여 조절
            // width = 1.0이면 95, width = 2.0이면 190
            float baseWidth = 95f;
            float newWidth = baseWidth * width;

            rectTransform.sizeDelta = new Vector2(newWidth, rectTransform.sizeDelta.y);
            Debug.Log($"Bell note visual width updated: width = {newWidth}");
        }
    }

    // Bell 노트 position 설정 (float, 1~4 범위)
    public void SetBellNotePosition(string inputed)
    {
        if (selectedNote == null)
        {
            Debug.LogWarning("No note selected!");
            return;
        }

        if (selectedNote.noteClass.type.ToLower() != "bell" && selectedNote.noteClass.type.ToLower() != "hold")
        {
            Debug.LogWarning("Selected note is not a bell/hold note!");
            return;
        }

        if (float.TryParse(inputed, out float position))
        {
            if (position >= 1f && position <= 4f)
            {
                selectedNote.noteClass.position = position;
                Debug.Log($"Bell note position set to {position}");

                // SaveManager의 notes 리스트에서 해당 노트를 찾아 업데이트
                NoteClass note = saveManager.notes.Find(n =>
                    n.beat == selectedNote.noteClass.beat &&
                    n.position == selectedNote.noteClass.position &&
                    n.type == selectedNote.noteClass.type);
                if (note != null)
                {
                    note.position = position;
                }

                // 노트의 X 위치도 업데이트
                UpdateNoteVisualPosition(selectedNote);
            }
            else
            {
                Debug.LogWarning($"Position must be between 1 and 4! Got: {position}");
            }
        }
        else
        {
            Debug.LogWarning($"Invalid position value: {inputed}");
        }
    }

    // 노트의 시각적 위치 업데이트
    private void UpdateNoteVisualPosition(LevelEditerNoteManager note)
    {
        if (note.noteClass.type.ToLower() == "bell" || note.noteClass.type.ToLower() == "hold")
        {
            // Bell 노트의 경우 position이 float이므로 연속적인 위치 계산
            // position 1 = -158f, position 4 = 158f
            // 선형 보간: positionX = -158 + (position - 1) * (316 / 3)
            float positionX = -158f + (note.noteClass.position - 1f) * (316f / 3f);
            note.transform.localPosition = new Vector3(positionX, note.transform.localPosition.y, note.transform.localPosition.z);
        }
    }
}
